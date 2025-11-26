using ToastPop.Core.Config;
using ToastPop.Core.Network;

namespace ToastPop.App.Services;

/// <summary>
/// 애플리케이션 서비스 컨테이너
/// </summary>
public static class AppServices
{
    private static bool _initialized;

    public static UpdateService UpdateService { get; } = new();
    public static LogService LogService { get; } = new();

    /// <summary>
    /// 서비스 초기화
    /// </summary>
    public static async Task Initialize()
    {
        if (_initialized) return;

        await Task.Run(() =>
        {
            // 서비스 초기화 로직
        });

        _initialized = true;
    }

    /// <summary>
    /// 서비스 정리
    /// </summary>
    public static void Dispose()
    {
        UpdateService.Dispose();
    }
}

/// <summary>
/// 로그 서비스
/// </summary>
public class LogService
{
    /// <summary>
    /// 로그 전송
    /// </summary>
    public static async Task PostLogAsync(string action)
    {
        try
        {
            var url = AppConstants.UrlLogHeader.Replace("%BID", GlobalInfo.Instance.ClientId);
            var content = $"action={action}&mac={GlobalInfo.Instance.MacAddress}";

            await HttpHelper.PostAsync(url, content);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Log post failed: {ex.Message}");
        }
    }
}

/// <summary>
/// 업데이트 서비스 (Squirrel 기반)
/// </summary>
public class UpdateService : IDisposable
{
    private bool _disposed;

    /// <summary>
    /// 업데이트 확인
    /// </summary>
    public async Task CheckForUpdatesAsync()
    {
        try
        {
            // Squirrel 업데이트 확인
            // 실제 구현시 Clowd.Squirrel 사용

            /*
            using var mgr = new Squirrel.UpdateManager("https://your-update-server.com/releases");
            var updateInfo = await mgr.CheckForUpdate();
            
            if (updateInfo.ReleasesToApply.Any())
            {
                await mgr.UpdateApp();
                
                // 재시작 알림
                MessageBox.Show("업데이트가 완료되었습니다. 프로그램이 재시작됩니다.");
                Squirrel.UpdateManager.RestartApp();
            }
            */

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Update check failed: {ex.Message}");
        }
    }

    /// <summary>
    /// 업데이트 적용
    /// </summary>
    public async Task ApplyUpdatesAsync()
    {
        try
        {
            // Squirrel 업데이트 적용
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Update apply failed: {ex.Message}");
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
    }
}
