using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;

namespace ToastPop.App.ViewModels;

/// <summary>
/// 브라우저 탭 뷰모델
/// </summary>
public partial class BrowserTabViewModel : ObservableObject, IDisposable
{
    private WebView2? _webView;
    private bool _disposed;

    public event EventHandler? CloseRequested;

    #region Observable Properties

    [ObservableProperty]
    private string _title = "새 탭";

    [ObservableProperty]
    private string _currentUrl = "about:blank";

    [ObservableProperty]
    private BitmapImage? _favicon;

    [ObservableProperty]
    private bool _isCloseable = true;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _canGoBack;

    [ObservableProperty]
    private bool _canGoForward;

    #endregion

    #region Commands

    [RelayCommand]
    private void GoBack()
    {
        if (_webView?.CanGoBack == true)
        {
            _webView.GoBack();
        }
    }

    [RelayCommand]
    private void GoForward()
    {
        if (_webView?.CanGoForward == true)
        {
            _webView.GoForward();
        }
    }

    [RelayCommand]
    private void Refresh()
    {
        _webView?.Reload();
    }

    [RelayCommand]
    private void GoHome()
    {
        NavigateCommand.Execute("about:blank");
    }

    [RelayCommand]
    private void Navigate(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return;

        var normalizedUrl = NormalizeUrl(url);
        CurrentUrl = normalizedUrl;
        IsLoading = true;

        if (_webView?.CoreWebView2 != null)
        {
            _webView.CoreWebView2.Navigate(normalizedUrl);
        }
    }

    [RelayCommand]
    private void Close()
    {
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }

    #endregion

    /// <summary>
    /// WebView2 설정
    /// </summary>
    public void SetWebView(WebView2 webView)
    {
        _webView = webView;
    }

    /// <summary>
    /// URL 정규화
    /// </summary>
    private static string NormalizeUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return "about:blank";

        url = url.Trim();

        if (url.Equals("about:blank", StringComparison.OrdinalIgnoreCase))
            return url;

        // 프로토콜이 없으면 추가
        if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
            !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase) &&
            !url.StartsWith("file://", StringComparison.OrdinalIgnoreCase))
        {
            url = "http://" + url;
        }

        return url;
    }

    #region HTML Block (미디어 차단)

    /// <summary>
    /// 웹 페이지 미디어 요소 차단
    /// </summary>
    public async Task BlockWebPageElementsAsync()
    {
        if (_webView?.CoreWebView2 == null) return;

        try
        {
            var script = @"
                (function() {
                    // 미디어 확장자
                    var mediaExt = ['.mp4', '.mp3', '.avi', '.mkv', '.wmv', '.swf', '.flv', '.mpg', '.mpeg'];
                    
                    // Video 태그 일시정지 및 숨기기
                    document.querySelectorAll('video').forEach(function(v) {
                        v.pause();
                        v.style.display = 'none';
                    });
                    
                    // Audio 태그 일시정지
                    document.querySelectorAll('audio').forEach(function(a) {
                        a.pause();
                    });
                    
                    // YouTube iframe 숨기기
                    document.querySelectorAll('iframe').forEach(function(f) {
                        if (f.src && f.src.indexOf('youtube.com') !== -1) {
                            f.style.display = 'none';
                        }
                    });
                    
                    // Object/Embed 숨기기
                    document.querySelectorAll('object, embed').forEach(function(o) {
                        o.style.display = 'none';
                    });
                })();
            ";

            await _webView.CoreWebView2.ExecuteScriptAsync(script);
        }
        catch { }
    }

    #endregion

    public void Dispose()
    {
        if (_disposed) return;
        _webView = null;
        _disposed = true;
    }
}