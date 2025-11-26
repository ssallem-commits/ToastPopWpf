using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

namespace ToastPop.Core.Network;

/// <summary>
/// HTTP 통신 유틸리티 클래스
/// </summary>
public static class HttpHelper
{
    private static readonly HttpClient _httpClient;
    private static readonly HttpClientHandler _handler;

    static HttpHelper()
    {
        _handler = new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
            UseCookies = true,
            CookieContainer = new CookieContainer()
        };

        _httpClient = new HttpClient(_handler)
        {
            Timeout = TimeSpan.FromSeconds(30)
        };

        _httpClient.DefaultRequestHeaders.Add("User-Agent", 
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
    }

    /// <summary>
    /// GET 요청
    /// </summary>
    public static async Task<string> GetAsync(string url, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"HTTP GET Error: {ex.Message}");
            return string.Empty;
        }
    }

    /// <summary>
    /// POST 요청
    /// </summary>
    public static async Task<string> PostAsync(string url, string content, string contentType = "application/x-www-form-urlencoded", CancellationToken cancellationToken = default)
    {
        try
        {
            var httpContent = new StringContent(content, Encoding.UTF8, contentType);
            var response = await _httpClient.PostAsync(url, httpContent, cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"HTTP POST Error: {ex.Message}");
            return string.Empty;
        }
    }

    /// <summary>
    /// 파일 다운로드
    /// </summary>
    public static async Task<bool> DownloadFileAsync(string url, string filePath, long minSize = 0, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();

            var contentLength = response.Content.Headers.ContentLength ?? 0;
            if (minSize > 0 && contentLength < minSize)
                return false;

            await using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);
            await response.Content.CopyToAsync(fileStream, cancellationToken);

            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Download Error: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 인터넷 연결 확인
    /// </summary>
    public static async Task<bool> CheckInternetConnectionAsync()
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var response = await _httpClient.GetAsync("https://www.google.com", HttpCompletionOption.ResponseHeadersRead, cts.Token);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// User-Agent 설정
    /// </summary>
    public static void SetUserAgent(string userAgent)
    {
        _httpClient.DefaultRequestHeaders.Remove("User-Agent");
        if (!string.IsNullOrEmpty(userAgent))
        {
            _httpClient.DefaultRequestHeaders.Add("User-Agent", userAgent);
        }
    }
}

/// <summary>
/// 네트워크 정보 유틸리티
/// </summary>
public static class NetworkInfo
{
    /// <summary>
    /// MAC 주소 가져오기
    /// </summary>
    public static string GetMacAddress()
    {
        try
        {
            var nics = NetworkInterface.GetAllNetworkInterfaces()
                .Where(n => n.OperationalStatus == OperationalStatus.Up &&
                           n.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                .OrderByDescending(n => n.Speed)
                .FirstOrDefault();

            if (nics != null)
            {
                var mac = nics.GetPhysicalAddress().ToString();
                return string.Join(":", Enumerable.Range(0, 6).Select(i => mac.Substring(i * 2, 2)));
            }
        }
        catch { }

        return "00:00:00:00:00:00";
    }

    /// <summary>
    /// 호스트 IP 주소 가져오기
    /// </summary>
    public static string GetHostAddress()
    {
        try
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            var ip = host.AddressList.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork);
            return ip?.ToString() ?? "127.0.0.1";
        }
        catch
        {
            return "127.0.0.1";
        }
    }

    /// <summary>
    /// 공인 IP 주소 가져오기
    /// </summary>
    public static async Task<string> GetPublicIpAsync()
    {
        try
        {
            return await HttpHelper.GetAsync("https://api.ipify.org");
        }
        catch
        {
            return "0.0.0.0";
        }
    }
}
