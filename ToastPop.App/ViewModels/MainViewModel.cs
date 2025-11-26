using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ToastPop.App.Services;
using ToastPop.Core.Config;
using ToastPop.Core.Network;
using ToastPop.Core.Xml;

namespace ToastPop.App.ViewModels;

/// <summary>
/// 메인 뷰모델 (MFC CToastpopDlg 로직 변환)
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly ConfigReader _configReader = new();
    private readonly Random _random = new();
    private System.Timers.Timer? _keyCycleTimer;
    private System.Timers.Timer? _executeTimer;
    private int _keyCycleB;
    private int _curSiteItem;

    #region Observable Properties

    [ObservableProperty]
    private ObservableCollection<BrowserTabViewModel> _tabs = new();

    [ObservableProperty]
    private BrowserTabViewModel? _selectedTab;

    [ObservableProperty]
    private string _statusMessage = "준비";

    [ObservableProperty]
    private bool _showInTaskbar = true;

    [ObservableProperty]
    private WindowState _windowState = WindowState.Normal;

    [ObservableProperty]
    private bool _minimizeToTray = true;

    #endregion

    #region Commands

    [RelayCommand]
    private void NewTab()
    {
        AddNewTab("about:blank");
    }

    [RelayCommand]
    private void CloseTab(BrowserTabViewModel? tab)
    {
        if (tab == null) return;

        tab.Dispose();
        Tabs.Remove(tab);

        // 탭이 없으면 새 탭 추가
        if (Tabs.Count == 0)
        {
            AddNewTab("about:blank");
        }
    }

    #endregion

    /// <summary>
    /// 새 탭 추가
    /// </summary>
    public void AddNewTab(string url, bool selectTab = true)
    {
        var tab = new BrowserTabViewModel
        {
            Title = "새 탭",
            CurrentUrl = url,
            IsCloseable = Tabs.Count > 0 // 첫 번째 탭은 닫기 불가
        };

        tab.CloseRequested += (s, e) => CloseTab(tab);
        Tabs.Add(tab);

        if (selectTab)
        {
            SelectedTab = tab;
        }
    }

    /// <summary>
    /// 설정 로드 및 시작
    /// </summary>
    public async Task LoadConfigAndStartAsync()
    {
        try
        {
            StatusMessage = "설정 로드 중...";

            // 배너 정보 로드
            var url = GlobalInfo.Instance.GetPackUrl(AppConstants.XmlBannerInfo);
            if (await _configReader.LoadUrlAsync(url) && _configReader.LoadInformation())
            {
                // User-Agent 설정
                if (_configReader.SiteList != null)
                {
                    var mobileAgent = _configReader.SiteList.Attribute("mobileagent")?.Value;
                    if (!string.IsNullOrEmpty(mobileAgent))
                    {
                        GlobalInfo.Instance.CurrentUserAgent = await ChooseUserAgentAsync(int.Parse(mobileAgent));
                        HttpHelper.SetUserAgent(GlobalInfo.Instance.CurrentUserAgent);
                    }

                    GlobalInfo.Instance.ViewWidth = _configReader.SiteList.Attribute("vw")?.Value ?? string.Empty;
                    GlobalInfo.Instance.ViewHeight = _configReader.SiteList.Attribute("vh")?.Value ?? string.Empty;
                }

                // ClickRange 복사
                GlobalInfo.Instance.ClickRanges.Clear();
                GlobalInfo.Instance.ClickRanges.AddRange(_configReader.ClickRanges);

                StatusMessage = "설정 로드 완료";

                // 키사이클 타이머 시작
                StartKeyCycleTimer();
            }
            else
            {
                StatusMessage = "설정 로드 실패";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"오류: {ex.Message}";
        }
    }

    /// <summary>
    /// User-Agent 선택
    /// </summary>
    private async Task<string> ChooseUserAgentAsync(int mobileAgentPercent)
    {
        // 모바일 에이전트 사용 여부 결정
        if (_random.Next(1, 1001) <= mobileAgentPercent)
        {
            // 모바일 에이전트 목록 가져오기
            var mobileAgentUrl = AppConstants.XmlMobileAgent;
            var content = await HttpHelper.GetAsync(mobileAgentUrl);

            if (!string.IsNullOrEmpty(content))
            {
                var agents = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                if (agents.Length > 0)
                {
                    return agents[_random.Next(agents.Length)].Trim();
                }
            }
        }

        // 기본 PC 에이전트
        return string.Empty;
    }

    #region Timer Logic (MFC OnTimer 변환)

    /// <summary>
    /// 키사이클 타이머 시작
    /// </summary>
    private void StartKeyCycleTimer()
    {
        _keyCycleTimer?.Dispose();
        _keyCycleTimer = new System.Timers.Timer(3000);
        _keyCycleTimer.Elapsed += async (s, e) => await OnTimerKeyCycleAsync();
        _keyCycleTimer.AutoReset = false;
        _keyCycleTimer.Start();
    }

    /// <summary>
    /// 키사이클 타이머 핸들러 (MFC OnTimerKeyCycle 변환)
    /// </summary>
    private async Task OnTimerKeyCycleAsync()
    {
        try
        {
            // 사이트 리스트 처리
            if (_configReader.SiteList != null)
            {
                var items = _configReader.SiteList.Elements("item").ToList();
                if (items.Count > 0)
                {
                    _keyCycleB++;
                    int keyCycleMax = ParseInt(_configReader.HiddenKeyCycleB, 1);

                    if (_keyCycleB >= keyCycleMax)
                    {
                        _keyCycleB = 0;
                        _curSiteItem++;

                        if (_curSiteItem >= items.Count)
                        {
                            _curSiteItem = 0;
                        }
                    }

                    // 실행 타이머 시작
                    StartExecuteTimer();
                    return;
                }
            }

            // 다시 키사이클 타이머 시작
            await Task.Delay(3000);
            _keyCycleTimer?.Start();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"KeyCycle error: {ex.Message}");
            _keyCycleTimer?.Start();
        }
    }

    /// <summary>
    /// 실행 타이머 시작
    /// </summary>
    private void StartExecuteTimer()
    {
        _executeTimer?.Dispose();
        _executeTimer = new System.Timers.Timer(100);
        _executeTimer.Elapsed += async (s, e) => await OnTimerExecuteAsync();
        _executeTimer.AutoReset = false;
        _executeTimer.Start();
    }

    /// <summary>
    /// 실행 타이머 핸들러 (MFC OnTimerExecute 변환)
    /// </summary>
    private async Task OnTimerExecuteAsync()
    {
        try
        {
            if (_configReader.SiteList == null)
            {
                _keyCycleTimer?.Start();
                return;
            }

            var items = _configReader.SiteList.Elements("item").ToList();
            if (_curSiteItem >= items.Count)
            {
                _keyCycleTimer?.Start();
                return;
            }

            var itemNode = items[_curSiteItem];
            var itemUrl = itemNode.Attribute("url")?.Value ?? string.Empty;
            var itemType = itemNode.Attribute("type")?.Value ?? string.Empty;

            if (!string.IsNullOrEmpty(itemUrl))
            {
                // 확률 체크
                bool shouldContinue = CheckExecuteProbability(itemNode);

                if (shouldContinue)
                {
                    // UI 스레드에서 탭에 URL 로드
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        if (SelectedTab != null)
                        {
                            SelectedTab.NavigateCommand.Execute(itemUrl);
                        }
                    });

                    StatusMessage = $"로드 중: {itemUrl}";

                    // 대기 시간 계산
                    int sTime = GetSTime();
                    await Task.Delay(sTime * 1000);

                    // 클릭 시뮬레이션 (필요한 경우)
                    // await PerformClickAsync();
                }
            }

            // 다음 사이클
            int cTime = GetCTime();
            await Task.Delay(cTime * 1000);
            _keyCycleTimer?.Start();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Execute error: {ex.Message}");
            _keyCycleTimer?.Start();
        }
    }

    /// <summary>
    /// 실행 확률 체크
    /// </summary>
    private bool CheckExecuteProbability(System.Xml.Linq.XElement itemNode)
    {
        // MClick, PClick 등 확률 체크 로직
        var mclick = ParseInt(itemNode.Attribute("mclick")?.Value, 1000);
        var rand = _random.Next(1, 1001);
        return rand <= mclick;
    }

    /// <summary>
    /// STime 가져오기 (대기 시간)
    /// </summary>
    private int GetSTime()
    {
        int down = ParseInt(_configReader.HiddenSTimeDown, 3);
        int up = ParseInt(_configReader.HiddenSTimeUp, 10);

        if (down >= up) return down;
        return _random.Next(down, up + 1);
    }

    /// <summary>
    /// CTime 가져오기 (사이클 시간)
    /// </summary>
    private int GetCTime()
    {
        int down = ParseInt(_configReader.HiddenCTimeDown, 5);
        int up = ParseInt(_configReader.HiddenCTimeUp, 15);

        if (down >= up) return down;
        return _random.Next(down, up + 1);
    }

    private static int ParseInt(string? value, int defaultValue)
    {
        if (string.IsNullOrEmpty(value)) return defaultValue;
        return int.TryParse(value, out var result) ? result : defaultValue;
    }

    #endregion

    /// <summary>
    /// 정리
    /// </summary>
    public void Dispose()
    {
        _keyCycleTimer?.Dispose();
        _executeTimer?.Dispose();

        foreach (var tab in Tabs)
        {
            tab.Dispose();
        }
    }
}
