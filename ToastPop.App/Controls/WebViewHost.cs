using System.Windows;
using System.Windows.Controls;
using Microsoft.Web.WebView2.Wpf;
using Microsoft.Web.WebView2.Core;
using ToastPop.App.ViewModels;

namespace ToastPop.App.Controls;

/// <summary>
/// WebView2 호스트 컨트롤
/// </summary>
public class WebViewHost : ContentControl
{
    private WebView2? _webView;
    private bool _isInitialized;
    private string? _pendingUrl;

    public static readonly DependencyProperty TabViewModelProperty =
        DependencyProperty.Register(
            nameof(TabViewModel),
            typeof(BrowserTabViewModel),
            typeof(WebViewHost),
            new PropertyMetadata(null, OnTabViewModelChanged));

    public BrowserTabViewModel? TabViewModel
    {
        get => (BrowserTabViewModel?)GetValue(TabViewModelProperty);
        set => SetValue(TabViewModelProperty, value);
    }

    public WebViewHost()
    {
        Loaded += WebViewHost_Loaded;
        DataContextChanged += WebViewHost_DataContextChanged;
    }

    private void WebViewHost_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is BrowserTabViewModel vm)
        {
            TabViewModel = vm;

            if (_isInitialized && _webView != null)
            {
                vm.SetWebView(_webView);
                if (!string.IsNullOrEmpty(vm.CurrentUrl))
                {
                    // CoreWebView2.Source (string) 우선 사용
                    _webView.CoreWebView2?.Navigate(vm.CurrentUrl);
                }
            }
        }
    }

    private async void WebViewHost_Loaded(object sender, RoutedEventArgs e)
    {
        if (_webView != null) return;

        try
        {
            _webView = new WebView2();
            Content = _webView;

            // WebView2 환경 초기화
            var env = await CoreWebView2Environment.CreateAsync(null,
                System.IO.Path.Combine(System.IO.Path.GetTempPath(), "ToastPop.WebView2"));

            await _webView.EnsureCoreWebView2Async(env);

            if (_webView.CoreWebView2 != null)
            {
                // 설정
                _webView.CoreWebView2.Settings.IsScriptEnabled = true;
                _webView.CoreWebView2.Settings.AreDefaultScriptDialogsEnabled = false;
                _webView.CoreWebView2.Settings.IsStatusBarEnabled = false;
                _webView.CoreWebView2.Settings.AreDevToolsEnabled = false;

                // 새 창 요청 → 현재 탭에서 열기
                _webView.CoreWebView2.NewWindowRequested += (s, args) =>
                {
                    args.Handled = true;
                    _webView.CoreWebView2.Navigate(args.Uri);
                };

                // 네비게이션 완료 이벤트
                _webView.NavigationCompleted += WebView_NavigationCompleted;
                _webView.SourceChanged += WebView_SourceChanged;

                _isInitialized = true;

                // 뷰모델 연결
                if (TabViewModel != null)
                {
                    TabViewModel.SetWebView(_webView);
                }

                // 대기 중인 URL 로드
                if (!string.IsNullOrEmpty(_pendingUrl))
                {
                    _webView.CoreWebView2.Navigate(_pendingUrl);
                    _pendingUrl = null;
                }
                else if (TabViewModel != null && !string.IsNullOrEmpty(TabViewModel.CurrentUrl))
                {
                    _webView.CoreWebView2.Navigate(TabViewModel.CurrentUrl);
                }
            }
        }
        catch (Exception ex)
        {
            Content = new TextBlock
            {
                Text = $"WebView2 초기화 실패:\n{ex.Message}\n\nWebView2 런타임을 설치해주세요.",
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(20)
            };
        }
    }

    private void WebView_NavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
    {
        if (TabViewModel != null && _webView?.CoreWebView2 != null)
        {
            // 문서 제목은 기존대로 업데이트
            TabViewModel.Title = _webView.CoreWebView2.DocumentTitle ?? "새 탭";
            TabViewModel.CanGoBack = _webView.CanGoBack;
            TabViewModel.CanGoForward = _webView.CanGoForward;
            TabViewModel.IsLoading = false;

            // URL도 명확하게 업데이트 (CoreWebView2.Source 우선)
            var source = _webView.CoreWebView2.Source ?? _webView.Source?.ToString();
            if (!string.IsNullOrEmpty(source))
            {
                // UI 스레드에서 뷰모델의 CurrentUrl 갱신
                Dispatcher.Invoke(() => TabViewModel.CurrentUrl = source);
            }

            // 미디어 차단 실행
            _ = TabViewModel.BlockWebPageElementsAsync();
        }
    }

    private void WebView_SourceChanged(object? sender, CoreWebView2SourceChangedEventArgs e)
    {
        if (TabViewModel != null && _webView != null)
        {
            // CoreWebView2.Source가 더 신뢰할 수 있음
            var source = _webView.CoreWebView2?.Source ?? _webView.Source?.ToString();
            if (!string.IsNullOrEmpty(source))
            {
                Dispatcher.Invoke(() => TabViewModel.CurrentUrl = source);
            }
        }
    }

    private static void OnTabViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is WebViewHost host)
        {
            if (e.NewValue is BrowserTabViewModel newVm)
            {
                if (host._isInitialized && host._webView != null)
                {
                    newVm.SetWebView(host._webView);

                    // 새 탭으로 전환 시 해당 URL로 이동
                    if (!string.IsNullOrEmpty(newVm.CurrentUrl) && newVm.CurrentUrl != "about:blank")
                    {
                        host._webView.CoreWebView2?.Navigate(newVm.CurrentUrl);
                    }
                }
                else
                {
                    host._pendingUrl = newVm.CurrentUrl;
                }
            }
        }
    }

    public void Navigate(string url)
    {
        if (_isInitialized && _webView?.CoreWebView2 != null)
        {
            _webView.CoreWebView2.Navigate(url);
        }
        else
        {
            _pendingUrl = url;

        }
    }
}