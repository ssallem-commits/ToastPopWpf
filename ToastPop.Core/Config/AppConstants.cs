namespace ToastPop.Core.Config;

/// <summary>
/// 애플리케이션 전역 설정 (MFC ups_def.h 변환)
/// </summary>
public static class AppConstants
{
    public const string DefaultClientId = "tga_default";
    public const string OperationTitle = "MWOTP-x64";
    public const string CryptoPassword = "TGA_util";

    // URL 설정
    public const string UrlLogHeader = "https://bustabcc.net/PRG/lg_read.php?bid=%BID";
    public const string XmlInfo = "https://bustabcc.net/SWC/tsp_read.php?client=%CLIENTID&cid=%MACADDR";
    public const string UrlRobotCheck = "https://bustabcc.net/SWC/robotcheck_read.php";
    public const string XmlBannerInfo = "https://bustabcc.net/SWC/conf_read.php?client=%CLIENTID";
    public const string XmlExtInfo = "https://bustabcc.net/SWC/dki_read.php?client=%CLIENTID";
    public const string XmlIlv = "https://bustabcc.net/SWC/livechk_read.php?client=%CLIENTID";
    public const string XmlMobileAgent = "https://bustabcc.net/SWC/mag_read.php";
    public const string XmlUpdate = "https://bustabcc.net/SWC/ups_read.php?client=%CLIENTID&cid=%MACADDR";

    // 레지스트리 경로
    public const string RegistryPath = @"Software\ToastPop";
    public const string RegistryRunPath = @"Software\Microsoft\Windows\CurrentVersion\Run";

    // 이벤트 이름
    public static string QuitEventName => $"{OperationTitle}_wq";
    public static string QuitEventUpdaterName => $"{OperationTitle}_wqu";
    public static string RunControlMutexName => $"{OperationTitle}_rctrn";
    public static string UpdateControlMutexName => $"{OperationTitle}_updctrl";
    public static string MainWindowName => $"{OperationTitle}_mwn";

    // 프로젝트 파일 경로
    public static string UpgradeCurrentXml => $"{OperationTitle}prj";
    public static string UpgradePrevXml => $"{OperationTitle}prvprj";

    // 퍼센트 범위
    public const int HListPercentRange = 1000;
    public const int RandAbcPercentRange = 1000;
    public const int MaxSaveKeyword = 70;
}

/// <summary>
/// 전역 정보 클래스
/// </summary>
public class GlobalInfo
{
    private static readonly Lazy<GlobalInfo> _instance = new(() => new GlobalInfo());
    public static GlobalInfo Instance => _instance.Value;

    public string ClientId { get; set; } = AppConstants.DefaultClientId;
    public string MacAddress { get; set; } = string.Empty;
    public string HostAddress { get; set; } = string.Empty;
    public string CurrentUserAgent { get; set; } = string.Empty;
    public string ViewWidth { get; set; } = string.Empty;
    public string ViewHeight { get; set; } = string.Empty;

    // 사이트 리스트 정보
    public SiteListInfo? SiteList { get; set; }
    public List<ClickRange> ClickRanges { get; } = new();

    private GlobalInfo() { }

    /// <summary>
    /// URL에서 변수 치환
    /// </summary>
    public string GetPackUrl(string url)
    {
        return url
            .Replace("%CLIENTID", ClientId)
            .Replace("%MACADDR", MacAddress)
            .Replace("%IP", HostAddress);
    }
}

/// <summary>
/// 사이트 리스트 정보
/// </summary>
public class SiteListInfo
{
    public string MobileAgent { get; set; } = string.Empty;
    public string ViewWidth { get; set; } = string.Empty;
    public string ViewHeight { get; set; } = string.Empty;
    public List<SiteItem> Items { get; } = new();
}

/// <summary>
/// 사이트 아이템 정보
/// </summary>
public class SiteItem
{
    public string Type { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public int PvqCycle { get; set; }
    public int MClick { get; set; }
    public int PClick { get; set; }
    public string ScrollX { get; set; } = string.Empty;
    public string ScrollRatio { get; set; } = string.Empty;
}

/// <summary>
/// 클릭 범위 정보
/// </summary>
public class ClickRange
{
    public int Per { get; set; }
    public int GapPer { get; set; }
}

/// <summary>
/// 시간 쿼리 정보
/// </summary>
public class TimeQuery
{
    public string Url { get; set; } = string.Empty;
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int QueryCount { get; set; }
}
