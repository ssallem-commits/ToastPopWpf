# ToastPop WPF

MFC 프로젝트를 C# WPF로 변환한 프로젝트입니다.

## 주요 변경 사항

### MFC → WPF 변환

| MFC | WPF/C# |
|-----|--------|
| CDialog | Window |
| CWebBrowser2 (IE) | WebView2 (Chrome 기반) |
| CString | string |
| CArray | List<T> / ObservableCollection<T> |
| XNode (XMLite) | XDocument / XElement |
| Timer (SetTimer) | DispatcherTimer / System.Timers.Timer |
| Thread (CreateThread) | Task / async-await |
| Registry (CRegKey) | Microsoft.Win32.Registry |
| HTTP (WinInet) | HttpClient |
| 팝업 WebBrowser | Tab Browser |

### 기술 스택

- **.NET 8.0** (Windows Desktop)
- **WPF** (MVVM 패턴)
- **WebView2** (Chrome 기반 브라우저)
- **CommunityToolkit.Mvvm** (MVVM 프레임워크)
- **Clowd.Squirrel** (자동 업데이트)

## 프로젝트 구조

```
ToastPopWpf/
├── ToastPop.Core/              # 공통 라이브러리
│   ├── Config/                 # 설정 및 상수
│   ├── Crypto/                 # 암호화 (XOR256, JhCrypt)
│   ├── Network/                # HTTP 통신
│   ├── Utils/                  # 시스템 유틸리티
│   └── Xml/                    # XML 파싱
│
├── ToastPop.App/               # WPF 애플리케이션
│   ├── Controls/               # 커스텀 컨트롤
│   ├── Converters/             # 값 변환기
│   ├── Services/               # 서비스 클래스
│   ├── Themes/                 # 테마 리소스
│   ├── ViewModels/             # 뷰모델 (MVVM)
│   └── Views/                  # 뷰 (XAML)
│
├── build.bat                   # 빌드 스크립트
└── README.md
```

## 빌드 요구 사항

- .NET 8.0 SDK
- Visual Studio 2022 (권장) 또는 VS Code
- Windows 10/11

## 빌드 방법

### Visual Studio

1. `ToastPopWpf.sln` 솔루션 파일 열기
2. 구성을 `Release`로 설정
3. 빌드 > 솔루션 빌드 (Ctrl+Shift+B)

### 명령줄

```bash
# 솔루션 빌드
dotnet build ToastPopWpf.sln -c Release

# Self-Contained 싱글 파일 배포
dotnet publish ToastPop.App/ToastPop.App.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=true -o publish
```

또는 `build.bat` 실행

## 배포

### Self-Contained 배포 (권장)

- .NET SDK 설치 불필요
- 싱글 EXE 파일로 배포
- 트리밍으로 최소 크기

### Framework-Dependent 배포

- .NET 8 런타임 필요
- 더 작은 배포 크기

## WebView2 런타임

WebView2 런타임이 필요합니다. Windows 11과 최신 Windows 10에는 기본 설치되어 있습니다.

없는 경우: https://developer.microsoft.com/en-us/microsoft-edge/webview2/

## 자동 업데이트 (Squirrel)

Squirrel을 사용한 자동 업데이트가 지원됩니다.

### 업데이트 서버 설정

1. GitHub Releases 또는 자체 서버 사용
2. `UpdateService.cs`에서 업데이트 URL 설정

### Squirrel 패키지 생성

```bash
# Squirrel.Windows 설치
dotnet tool install -g Clowd.Squirrel

# 릴리즈 패키지 생성
squirrel pack --packId ToastPop --packVersion 1.0.0 --packDirectory publish --releaseDir releases
```

## 주요 기능

### Tab Browser
- Chrome 기반 WebView2
- 다중 탭 지원
- 팝업 → 새 탭으로 열기

### 미디어 차단
- YouTube 자동 차단
- 비디오/오디오 자동 일시정지
- Flash, 미디어 파일 차단

### 시스템 트레이
- 최소화 시 트레이로 이동
- 트레이 아이콘 더블클릭으로 복원

### 자동 시작
- Windows 시작 시 자동 실행
- 레지스트리 자동 등록

## 설정

### 암호화 키 변경

`ToastPop.Core/Config/AppConstants.cs`:
```csharp
public const string CryptoPassword = "your_password";
```

### 서버 URL 변경

`ToastPop.Core/Config/AppConstants.cs`:
```csharp
public const string XmlBannerInfo = "https://your-server.com/api/config";
```

## 라이선스

이 프로젝트는 원본 MFC 프로젝트의 변환본입니다.

## 문의

문제가 있으시면 이슈를 등록해 주세요.
