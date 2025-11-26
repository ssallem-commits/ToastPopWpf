using System.IO;
using System.Threading;
using System.Windows;
using ToastPop.App.Services;
using ToastPop.Core.Config;
using ToastPop.Core.Crypto;
using ToastPop.Core.Network;
using ToastPop.Core.Utils;

namespace ToastPop.App;

/// <summary>
/// 애플리케이션 진입점 (MFC CToastpopApp 변환)
/// </summary>
public partial class App : Application
{
    private Mutex? _runControlMutex;
    private bool _isByUpdater;
    private bool _isByBoot;

    public static string MyPath { get; private set; } = string.Empty;
    public static string MyFolder { get; private set; } = string.Empty;
    public static string MyExeName { get; private set; } = string.Empty;
    public static string VersionMainExe { get; set; } = "1.0.0";
    public static string VersionUpdater { get; set; } = "1.0.0";
    public static string UpdaterSavePath { get; set; } = string.Empty;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // 암호화 키 초기화
        JhCrypt.SetPassword(AppConstants.CryptoPassword);

        // 전역 정보 초기화
        InitializeGlobalInfo();

        // 명령줄 인수 처리
        ProcessCommandLineArgs(e.Args);

        // Mutex 체크 (중복 실행 방지)
        if (!CheckSingleInstance())
        {
            Shutdown();
            return;
        }

        // 시작 프로그램 등록 확인
        EnsureStartupRegistration();

        // 업데이터 확인
        CheckUpdater();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        // Mutex 해제
        _runControlMutex?.ReleaseMutex();
        _runControlMutex?.Dispose();

        // 서비스 정리
        AppServices.Dispose();

        base.OnExit(e);
    }

    /// <summary>
    /// 전역 정보 초기화
    /// </summary>
    private void InitializeGlobalInfo()
    {
        // 경로 정보
        MyPath = ProcessHelper.GetCurrentExePath();
        MyFolder = ProcessHelper.GetCurrentExeFolder();
        MyExeName = Path.GetFileName(MyPath);

        // 클라이언트 ID
        var clientId = ClientIdHelper.GetOvIdFromFile();
        if (string.IsNullOrEmpty(clientId))
        {
            clientId = ClientIdHelper.GenerateClientId();
            ClientIdHelper.SaveOvId(clientId);
        }

        // GlobalInfo 설정
        GlobalInfo.Instance.ClientId = clientId;
        GlobalInfo.Instance.MacAddress = NetworkInfo.GetMacAddress();
        GlobalInfo.Instance.HostAddress = NetworkInfo.GetHostAddress();
    }

    /// <summary>
    /// 명령줄 인수 처리
    /// </summary>
    private void ProcessCommandLineArgs(string[] args)
    {
        foreach (var arg in args)
        {
            var normalized = arg.TrimStart('/', '-').ToLowerInvariant();

            switch (normalized)
            {
                case "uninstall":
                    PerformUninstall();
                    Shutdown();
                    return;

                case "update":
                    _isByUpdater = true;
                    break;

                case "byboot":
                    _isByBoot = true;
                    break;
            }
        }
    }

    /// <summary>
    /// 단일 인스턴스 체크
    /// </summary>
    private bool CheckSingleInstance()
    {
        bool createdNew;
        _runControlMutex = new Mutex(true, AppConstants.RunControlMutexName, out createdNew);

        if (!createdNew)
        {
            // 이미 실행 중
            return false;
        }

        return true;
    }

    /// <summary>
    /// 시작 프로그램 등록 확인
    /// </summary>
    private void EnsureStartupRegistration()
    {
        if (!RegistryHelper.IsInStartup(MyPath))
        {
            var keyName = Path.GetFileNameWithoutExtension(MyExeName);
            RegistryHelper.AddToStartup(keyName, MyPath, "/byboot");
        }
    }

    /// <summary>
    /// 업데이터 확인
    /// </summary>
    private async void CheckUpdater()
    {
        // Squirrel 업데이트 확인 (별도 스레드)
        await Task.Run(async () =>
        {
            try
            {
                await AppServices.UpdateService.CheckForUpdatesAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Update check failed: {ex.Message}");
            }
        });
    }

    /// <summary>
    /// 언인스톨 수행
    /// </summary>
    private void PerformUninstall()
    {
        // 로그 전송
        _ = LogService.PostLogAsync("uninstall");

        // 종료 이벤트 설정
        try
        {
            using var quitEvent = EventWaitHandle.OpenExisting(AppConstants.QuitEventName);
            quitEvent.Set();
        }
        catch { }

        try
        {
            using var quitEventUpdater = EventWaitHandle.OpenExisting(AppConstants.QuitEventUpdaterName);
            quitEventUpdater.Set();
        }
        catch { }

        Thread.Sleep(3000);
    }
}
