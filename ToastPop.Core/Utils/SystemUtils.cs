using Microsoft.Win32;
using System.Diagnostics;
using System.Runtime.InteropServices;
using ToastPop.Core.Config;

namespace ToastPop.Core.Utils;

/// <summary>
/// 레지스트리 유틸리티
/// </summary>
public static class RegistryHelper
{
    /// <summary>
    /// 시작 프로그램에 등록
    /// </summary>
    public static bool AddToStartup(string keyName, string exePath, string args = "")
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(AppConstants.RegistryRunPath, true);
            if (key == null) return false;

            var value = string.IsNullOrEmpty(args) ? $"\"{exePath}\"" : $"\"{exePath}\" {args}";
            key.SetValue(keyName, value);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 시작 프로그램에서 제거
    /// </summary>
    public static bool RemoveFromStartup(string keyName)
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(AppConstants.RegistryRunPath, true);
            key?.DeleteValue(keyName, false);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 시작 프로그램 등록 여부 확인
    /// </summary>
    public static bool IsInStartup(string exePath)
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(AppConstants.RegistryRunPath);
            if (key == null) return false;

            foreach (var name in key.GetValueNames())
            {
                var value = key.GetValue(name) as string;
                if (value?.Contains(exePath, StringComparison.OrdinalIgnoreCase) == true)
                    return true;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 레지스트리 값 가져오기
    /// </summary>
    public static string? GetValue(string subKey, string valueName)
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(subKey);
            return key?.GetValue(valueName) as string;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 레지스트리 값 설정
    /// </summary>
    public static bool SetValue(string subKey, string valueName, string value)
    {
        try
        {
            using var key = Registry.CurrentUser.CreateSubKey(subKey);
            key?.SetValue(valueName, value);
            return true;
        }
        catch
        {
            return false;
        }
    }
}

/// <summary>
/// 파일 시스템 유틸리티
/// </summary>
public static class FileHelper
{
    /// <summary>
    /// 파일 존재 확인
    /// </summary>
    public static bool IsFileThere(string path) => File.Exists(path);

    /// <summary>
    /// 디렉토리 확인 및 생성
    /// </summary>
    public static bool CheckDirectory(string path)
    {
        try
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 경로 필터 (환경 변수 확장)
    /// </summary>
    public static string PathFilter(string path)
    {
        if (string.IsNullOrEmpty(path))
            return path;

        var result = path;

        // 환경 변수 치환
        result = result.Replace("%PROGRAMFILES", Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles));
        result = result.Replace("%APPDATA", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
        result = result.Replace("%LOCALAPPDATA", Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
        result = result.Replace("%TEMP", Path.GetTempPath());
        result = result.Replace("%WINDIR", Environment.GetFolderPath(Environment.SpecialFolder.Windows));

        return result;
    }

    /// <summary>
    /// 다운로드 파일 유효성 검사
    /// </summary>
    public static bool IsValidDownloadFile(string filePath, long minSize = 0)
    {
        try
        {
            if (!File.Exists(filePath))
                return false;

            var fileInfo = new FileInfo(filePath);
            if (minSize > 0 && fileInfo.Length < minSize)
                return false;

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 안전한 파일 복사
    /// </summary>
    public static bool SafeCopyFile(string source, string destination)
    {
        try
        {
            if (!File.Exists(source))
                return false;

            var destDir = Path.GetDirectoryName(destination);
            if (!string.IsNullOrEmpty(destDir) && !Directory.Exists(destDir))
                Directory.CreateDirectory(destDir);

            File.Copy(source, destination, true);
            return true;
        }
        catch
        {
            return false;
        }
    }
}

/// <summary>
/// 프로세스 유틸리티
/// </summary>
public static class ProcessHelper
{
    /// <summary>
    /// 프로세스 실행
    /// </summary>
    public static bool StartProcess(string filePath, string args = "", bool hidden = false)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = filePath,
                Arguments = args,
                UseShellExecute = true,
                WindowStyle = hidden ? ProcessWindowStyle.Hidden : ProcessWindowStyle.Normal
            };

            Process.Start(startInfo);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 현재 프로세스 경로 가져오기
    /// </summary>
    public static string GetCurrentExePath()
    {
        return Environment.ProcessPath ?? Process.GetCurrentProcess().MainModule?.FileName ?? string.Empty;
    }

    /// <summary>
    /// 현재 프로세스 폴더 가져오기
    /// </summary>
    public static string GetCurrentExeFolder()
    {
        var exePath = GetCurrentExePath();
        return Path.GetDirectoryName(exePath) ?? string.Empty;
    }
}

/// <summary>
/// OS 버전 유틸리티
/// </summary>
public static class OsHelper
{
    /// <summary>
    /// Vista 이상인지 확인
    /// </summary>
    public static bool IsOverVista => Environment.OSVersion.Version.Major >= 6;

    /// <summary>
    /// Windows 10 이상인지 확인
    /// </summary>
    public static bool IsWindows10OrGreater => Environment.OSVersion.Version.Major >= 10;

    /// <summary>
    /// 64비트 OS인지 확인
    /// </summary>
    public static bool Is64BitOperatingSystem => Environment.Is64BitOperatingSystem;
}

/// <summary>
/// 클라이언트 ID 유틸리티
/// </summary>
public static class ClientIdHelper
{
    private const string ClientIdFileName = "ovid.dat";

    /// <summary>
    /// 파일에서 클라이언트 ID 가져오기
    /// </summary>
    public static string GetOvIdFromFile()
    {
        try
        {
            var filePath = Path.Combine(ProcessHelper.GetCurrentExeFolder(), ClientIdFileName);
            if (File.Exists(filePath))
            {
                return File.ReadAllText(filePath).Trim();
            }
        }
        catch { }

        return string.Empty;
    }

    /// <summary>
    /// 클라이언트 ID 저장
    /// </summary>
    public static bool SaveOvId(string clientId)
    {
        try
        {
            var filePath = Path.Combine(ProcessHelper.GetCurrentExeFolder(), ClientIdFileName);
            File.WriteAllText(filePath, clientId);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 클라이언트 ID 생성 (MAC 주소 기반)
    /// </summary>
    public static string GenerateClientId()
    {
        var mac = Network.NetworkInfo.GetMacAddress().Replace(":", "");
        var timestamp = DateTime.Now.Ticks.ToString("X");
        return $"{mac}_{timestamp}".ToLowerInvariant();
    }
}

/// <summary>
/// Win32 API 래퍼
/// </summary>
public static class NativeMethods
{
    [DllImport("user32.dll")]
    public static extern bool SetCursorPos(int x, int y);

    [DllImport("user32.dll")]
    public static extern void mouse_event(uint dwFlags, int dx, int dy, uint dwData, int dwExtraInfo);

    public const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
    public const uint MOUSEEVENTF_LEFTUP = 0x0004;
    public const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
    public const uint MOUSEEVENTF_RIGHTUP = 0x0010;

    /// <summary>
    /// 마우스 클릭 시뮬레이션
    /// </summary>
    public static void SendMouseClick(int x, int y)
    {
        SetCursorPos(x, y);
        mouse_event(MOUSEEVENTF_LEFTDOWN, x, y, 0, 0);
        Thread.Sleep(50);
        mouse_event(MOUSEEVENTF_LEFTUP, x, y, 0, 0);
    }
}

/// <summary>
/// 랜덤 유틸리티
/// </summary>
public static class RandomHelper
{
    private static readonly Random _random = new();

    /// <summary>
    /// 범위 내 랜덤 정수
    /// </summary>
    public static int Next(int minValue, int maxValue) => _random.Next(minValue, maxValue);

    /// <summary>
    /// 0 ~ maxValue 사이 랜덤 정수
    /// </summary>
    public static int Next(int maxValue) => _random.Next(maxValue);

    /// <summary>
    /// 1 ~ range 사이 랜덤 정수
    /// </summary>
    public static int NextPercent(int range) => _random.Next(1, range + 1);
}
