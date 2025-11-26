using CommunityToolkit.Mvvm.Input;
using Hardcodet.Wpf.TaskbarNotification;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using ToastPop.App.Services;
using ToastPop.App.ViewModels;

namespace ToastPop.App.Views;

/// <summary>
/// MainWindow (MFC CToastpopDlg 변환)
/// </summary>
public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;
    // private System.Windows.Forms.NotifyIcon? _notifyIcon;
    private TaskbarIcon? _notifyIcon;

    public MainWindow()
    {
        InitializeComponent();

        _viewModel = (MainViewModel)DataContext;

        // 윈도우 이벤트 핸들러
        Loaded += MainWindow_Loaded;
        Closing += MainWindow_Closing;
        StateChanged += MainWindow_StateChanged;

        // 시스템 트레이 아이콘 설정
        SetupNotifyIcon();

#if !DEBUG
        // 릴리즈 모드에서는 숨김 시작
        WindowState = WindowState.Minimized;
        ShowInTaskbar = false;
#endif
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // 초기 탭 추가
        if (_viewModel.Tabs.Count == 0)
        {
            _viewModel.AddNewTab("about:blank");
        }

        // 서비스 초기화
        await InitializeServicesAsync();

        // 인터넷 연결 확인 및 시작
        await CheckInternetAndStartAsync();
    }

    private void MainWindow_Closing(object? sender, CancelEventArgs e)
    {
        // 트레이로 최소화
        if (_viewModel.MinimizeToTray)
        {
            e.Cancel = true;
            WindowState = WindowState.Minimized;
            ShowInTaskbar = false;
            return;
        }

        // 정리
        _notifyIcon?.Dispose();
        AppServices.Dispose();
    }

    private void MainWindow_StateChanged(object? sender, EventArgs e)
    {
        if (WindowState == WindowState.Minimized)
        {
            ShowInTaskbar = false;
        }
        else
        {
            ShowInTaskbar = true;
        }
    }

    /// <summary>
    /// 시스템 트레이 아이콘 설정
    /// </summary>
    private void SetupNotifyIcon()
    {
        _notifyIcon = new TaskbarIcon
        {
            ToolTipText = "ToastPop",
            Visibility = Visibility.Visible
        };

        // 아이콘 설정k
        try
        {
            var iconUri = new Uri("pack://application:,,,/Resources/app.ico");
            _notifyIcon.IconSource = new System.Windows.Media.Imaging.BitmapImage(iconUri);
        }
        catch { }

        // 컨텍스트 메뉴
        var menu = new ContextMenu();
        menu.Items.Add(new MenuItem { Header = "열기", Command = new RelayCommand(() => ShowWindow()) });
        menu.Items.Add(new Separator());
        menu.Items.Add(new MenuItem { Header = "종료", Command = new RelayCommand(() => ExitApplication()) });
        _notifyIcon.ContextMenu = menu;

        // 더블클릭
        _notifyIcon.TrayMouseDoubleClick += (s, e) => ShowWindow();
    }

    /// <summary>
    /// 윈도우 표시
    /// </summary>
    private void ShowWindow()
    {
        Show();
        WindowState = WindowState.Normal;
        ShowInTaskbar = true;
        Activate();
    }

    /// <summary>
    /// 애플리케이션 종료
    /// </summary>
    private void ExitApplication()
    {
        _viewModel.MinimizeToTray = false;
        _notifyIcon?.Dispose();
        Application.Current.Shutdown();
    }

    /// <summary>
    /// 서비스 초기화
    /// </summary>
    private async Task InitializeServicesAsync()
    {
        try
        {
            // 백그라운드 서비스 시작
            await AppServices.Initialize();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Service initialization failed: {ex.Message}");
        }
    }

    /// <summary>
    /// 인터넷 연결 확인 및 시작
    /// </summary>
    private async Task CheckInternetAndStartAsync()
    {
        // 인터넷 연결 대기 (MFC OnInitDialog에서 101 타이머 역할)
        int retryCount = 0;
        const int maxRetries = 10;

        while (retryCount < maxRetries)
        {
            if (await Core.Network.HttpHelper.CheckInternetConnectionAsync())
            {
                _viewModel.StatusMessage = "인터넷 연결됨";
                await _viewModel.LoadConfigAndStartAsync();
                return;
            }

            _viewModel.StatusMessage = $"인터넷 연결 대기 중... ({retryCount + 1}/{maxRetries})";
            await Task.Delay(3000);
            retryCount++;
        }

        _viewModel.StatusMessage = "인터넷 연결 실패";
    }
}
