using System.Xml.Linq;
using ToastPop.Core.Config;
using ToastPop.Core.Crypto;
using ToastPop.Core.Network;

namespace ToastPop.Core.Xml;

/// <summary>
/// 설정 XML Reader (MFC CNewReader 변환)
/// </summary>
public class ConfigReader
{
    private XDocument? _document;
    private XElement? _root;

    // Program 섹션 속성
    public string RefDomValue { get; private set; } = string.Empty;
    public string HiddenOnOff { get; private set; } = string.Empty;
    public string HiddenBwc { get; private set; } = string.Empty;
    public string HiddenCTime { get; private set; } = string.Empty;
    public string HiddenCTimeDown { get; private set; } = string.Empty;
    public string HiddenCTimeUp { get; private set; } = string.Empty;
    public string HiddenSTimeDown { get; private set; } = string.Empty;
    public string HiddenSTimeUp { get; private set; } = string.Empty;
    public string HiddenRandA { get; private set; } = string.Empty;
    public string HiddenRandB { get; private set; } = string.Empty;
    public string HiddenRandC { get; private set; } = string.Empty;
    public string HiddenRandV { get; private set; } = string.Empty;
    public string HiddenRandNeo { get; private set; } = string.Empty;
    public string HiddenRandPower { get; private set; } = string.Empty;
    public string HiddenRandN2s { get; private set; } = string.Empty;
    public string HiddenRandAdnc { get; private set; } = string.Empty;
    public string HiddenRandCriteo { get; private set; } = string.Empty;
    public string HiddenMUserAgent { get; private set; } = string.Empty;
    public string HiddenKeyCycleB { get; private set; } = string.Empty;
    public string HiddenSubCycleB { get; private set; } = string.Empty;
    public string HiddenNeoBack3 { get; private set; } = string.Empty;

    // TimeQuery 섹션
    public string STime { get; private set; } = string.Empty;
    public string ETime { get; private set; } = string.Empty;
    public string QCnt { get; private set; } = string.Empty;
    public List<TimeQuery> TimeQueries { get; } = new();

    // SiteList
    public XElement? SiteList { get; private set; }
    public XElement? MktQuery { get; private set; }

    // Click Range
    public List<ClickRange> ClickRanges { get; } = new();

    // Sub Percent
    private readonly int[] _hiddenSubPerDown = new int[5];
    private readonly int[] _hiddenSubPerUp = new int[5];

    /// <summary>
    /// URL에서 XML 로드
    /// </summary>
    public async Task<bool> LoadUrlAsync(string url)
    {
        try
        {
            var content = await HttpHelper.GetAsync(url);
            if (string.IsNullOrEmpty(content))
                return false;

            return LoadXml(content);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 파일에서 XML 로드
    /// </summary>
    public bool LoadFile(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
                return false;

            var content = File.ReadAllText(filePath);
            return LoadXml(content);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// XML 문자열 로드
    /// </summary>
    public bool LoadXml(string xmlContent)
    {
        try
        {
            _document = XDocument.Parse(xmlContent);
            _root = _document.Root;
            return _root != null;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// XML 정보 로드
    /// </summary>
    public bool LoadInformation()
    {
        if (_root == null)
            return false;

        UnloadInformation();

        try
        {
            // Program 섹션 파싱
            var programSection = _root.Element("program");
            if (programSection != null)
            {
                // 암호화된 속성명 복호화
                RefDomValue = GetChildAttrValue(programSection, DecryptAttr("E2C0DE1E1F38"), DecryptAttr("E6C0F06FC4"));
                HiddenOnOff = GetChildAttrValue(programSection, DecryptAttr("CCD2DAF2C92D"), DecryptAttr("CBB6EDF42B3D6F"));
                HiddenBwc = GetChildAttrValue(programSection, DecryptAttr("CCD2DAF2C92D"), DecryptAttr("D2BEDB"));
                HiddenCTime = GetChildAttrValue(programSection, DecryptAttr("CCD2DAF2C92D"), DecryptAttr("D5C0431C13"));
                HiddenCTimeDown = GetChildAttrValue(programSection, DecryptAttr("CCD2DAF2C92D"), DecryptAttr("D5C0431C134575FFC3"));
                HiddenCTimeUp = GetChildAttrValue(programSection, DecryptAttr("CCD2DAF2C92D"), DecryptAttr("D5C0431C13344B"));
                HiddenSTimeDown = GetChildAttrValue(programSection, DecryptAttr("CCD2DAF2C92D"), DecryptAttr("E5D0F30CC335450FF3"));
                HiddenSTimeUp = GetChildAttrValue(programSection, DecryptAttr("CCD2DAF2C92D"), DecryptAttr("E5D0F30CC3445B"));
                HiddenRandA = GetChildAttrValue(programSection, DecryptAttr("CCD2DAF2C92D"), DecryptAttr("E2D4D2FA2348"));
                HiddenRandB = GetChildAttrValue(programSection, DecryptAttr("CCD2DAF2C92D"), DecryptAttr("E2D4D2FA2357"));
                HiddenRandC = GetChildAttrValue(programSection, DecryptAttr("CCD2DAF2C92D"), DecryptAttr("E2D4D2FA235E"));
                HiddenRandV = GetChildAttrValue(programSection, DecryptAttr("CCD2DAF2C92D"), DecryptAttr("E2D4D2FA2363"));
                HiddenRandNeo = GetChildAttrValue(programSection, DecryptAttr("CCD2DAF2C92D"), DecryptAttr("E2D4D2FA234B6527"));
                HiddenRandPower = GetChildAttrValue(programSection, DecryptAttr("CCD2DAF2C92D"), DecryptAttr("E2D4D2FA2359694B98E6"));
                HiddenRandN2s = GetChildAttrValue(programSection, DecryptAttr("CCD2DAF2C92D"), DecryptAttr("E2D4D2FA234B505E"));
                HiddenRandAdnc = GetChildAttrValue(programSection, DecryptAttr("CCD2DAF2C92D"), DecryptAttr("E2D4D2FA23487B4A7D"));
                HiddenRandCriteo = GetChildAttrValue(programSection, DecryptAttr("CCD2DAF2C92D"), DecryptAttr("E2D4D2FA235E671B99E81D"));
                HiddenMUserAgent = GetChildAttrValue(programSection, DecryptAttr("CCD2DAF2C92D"), DecryptAttr("831714FDFA5478526FE1FF"));
                HiddenKeyCycleB = GetChildAttrValue(programSection, DecryptAttr("CCD2DAF2C92D"), DecryptAttr("CDCB441D1E21583887CD"));
                HiddenSubCycleB = GetChildAttrValue(programSection, DecryptAttr("CCD2DAF2C92D"), DecryptAttr("E5D3F118FB46210598F6"));
                HiddenNeoBack3 = GetChildAttrValue(programSection, DecryptAttr("CCD2DAF2C92D"), DecryptAttr("CEDCF1F7C8373F3930"));

                // SubPercent 파싱
                int nSub = ParseInt(HiddenSubCycleB, 0);
                int nSubDown = 1;
                for (int s = 0; s < nSub && s < 5; s++)
                {
                    var percent = GetChildAttrValue(programSection, DecryptAttr("CCD2DAF2C92D"), $"subper{s + 1}");
                    if (!string.IsNullOrEmpty(percent))
                    {
                        _hiddenSubPerDown[s] = nSubDown;
                        _hiddenSubPerUp[s] = nSubDown + ParseInt(percent, 0) - 1;
                        nSubDown = _hiddenSubPerUp[s] + 1;
                    }
                }

                // ClRange 파싱
                var clRangeNode = programSection.Element("clrange");
                if (clRangeNode != null)
                {
                    foreach (var idx in clRangeNode.Elements("idx"))
                    {
                        var range = new ClickRange
                        {
                            Per = ParseInt(idx.Attribute("per")?.Value, 0),
                            GapPer = ParseInt(idx.Attribute("gaper")?.Value, 0)
                        };
                        if (range.Per > 0 && range.GapPer > 0)
                        {
                            ClickRanges.Add(range);
                        }
                    }
                }
            }

            // TimeQuery 섹션 파싱
            var tmQuerySection = _root.Element("tmquery");
            if (tmQuerySection != null)
            {
                STime = tmQuerySection.Attribute("stime")?.Value ?? string.Empty;
                ETime = tmQuerySection.Attribute("etime")?.Value ?? string.Empty;
                QCnt = tmQuerySection.Attribute("qcnt")?.Value ?? string.Empty;

                foreach (var qlist in tmQuerySection.Elements("qlist"))
                {
                    var url = qlist.Attribute("url")?.Value;
                    if (!string.IsNullOrEmpty(url))
                    {
                        TimeQueries.Add(new TimeQuery { Url = url });
                    }
                }
            }

            // SiteList 및 MktQuery
            SiteList = _root.Element("widelist");
            MktQuery = _root.Element("mktquery");

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 정보 초기화
    /// </summary>
    public void UnloadInformation()
    {
        RefDomValue = string.Empty;
        HiddenOnOff = string.Empty;
        HiddenBwc = string.Empty;
        HiddenCTime = string.Empty;
        HiddenCTimeDown = string.Empty;
        HiddenCTimeUp = string.Empty;
        HiddenSTimeDown = string.Empty;
        HiddenSTimeUp = string.Empty;
        HiddenRandA = string.Empty;
        HiddenRandB = string.Empty;
        HiddenRandC = string.Empty;
        HiddenRandV = string.Empty;
        HiddenRandNeo = string.Empty;
        HiddenRandPower = string.Empty;
        HiddenRandN2s = string.Empty;
        HiddenRandAdnc = string.Empty;
        HiddenRandCriteo = string.Empty;
        HiddenMUserAgent = string.Empty;
        HiddenKeyCycleB = string.Empty;
        HiddenSubCycleB = string.Empty;
        HiddenNeoBack3 = string.Empty;

        Array.Clear(_hiddenSubPerDown, 0, 5);
        Array.Clear(_hiddenSubPerUp, 0, 5);

        ClickRanges.Clear();
        TimeQueries.Clear();
        SiteList = null;
        MktQuery = null;
        STime = string.Empty;
        ETime = string.Empty;
        QCnt = string.Empty;
    }

    /// <summary>
    /// iLog URL 가져오기
    /// </summary>
    public string GetILogUrl()
    {
        if (_root?.Name != "global") return string.Empty;
        return _root.Element("set")?.Element("ilog")?.Attribute("url")?.Value ?? string.Empty;
    }

    /// <summary>
    /// uLog URL 가져오기
    /// </summary>
    public string GetULogUrl()
    {
        if (_root?.Name != "global") return string.Empty;
        return _root.Element("set")?.Element("ulog")?.Attribute("url")?.Value ?? string.Empty;
    }

    /// <summary>
    /// dLog URL 가져오기
    /// </summary>
    public string GetDLogUrl()
    {
        if (_root?.Name != "global") return string.Empty;
        return _root.Element("set")?.Element("dlog")?.Attribute("url")?.Value ?? string.Empty;
    }

    #region Helper Methods

    private static string GetChildAttrValue(XElement parent, string childName, string attrName)
    {
        return parent.Element(childName)?.Attribute(attrName)?.Value ?? string.Empty;
    }

    private static string DecryptAttr(string encrypted)
    {
        // 빈 값이면 그대로 반환 (실제 운영에서는 암호화된 값 사용)
        if (string.IsNullOrEmpty(encrypted))
            return encrypted;

        return JhCrypt.Decrypt(encrypted);
    }

    private static int ParseInt(string? value, int defaultValue)
    {
        if (string.IsNullOrEmpty(value))
            return defaultValue;

        return int.TryParse(value, out var result) ? result : defaultValue;
    }

    #endregion
}

/// <summary>
/// 설치 정보 Reader (MFC CInstallReader 변환)
/// </summary>
public class InstallReader : ConfigReader
{
    public string VersionMainExe { get; private set; } = string.Empty;
    public string VersionUpdater { get; private set; } = string.Empty;
    public List<NProgram> Programs { get; } = new();
    public List<NFile> Files { get; } = new();

    /// <summary>
    /// 레지스트리에서 로드
    /// </summary>
    public bool LoadRegistry(string regPath)
    {
        try
        {
            using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(AppConstants.RegistryPath);
            var value = key?.GetValue(regPath) as string;
            return !string.IsNullOrEmpty(value) && LoadXml(value);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 프로그램 찾기
    /// </summary>
    public NProgram? FindNProgram(string name)
    {
        return Programs.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// 파일 찾기
    /// </summary>
    public NFile? FindNFile(string programName, int fileId)
    {
        return Files.FirstOrDefault(f => 
            f.ProgramName.Equals(programName, StringComparison.OrdinalIgnoreCase) && 
            f.FileId == fileId);
    }
}

/// <summary>
/// 프로그램 정보
/// </summary>
public class NProgram
{
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
}

/// <summary>
/// 파일 정보
/// </summary>
public class NFile
{
    public string ProgramName { get; set; } = string.Empty;
    public int FileId { get; set; }
    public string Folder { get; set; } = string.Empty;
    public string Filename { get; set; } = string.Empty;
    public DownloadInfo Download { get; set; } = new();
    public AutoRunInfo AutoRun { get; set; } = new();
}

public class DownloadInfo
{
    public string Url { get; set; } = string.Empty;
    public string MinSize { get; set; } = string.Empty;
}

public class AutoRunInfo
{
    public string KeyName { get; set; } = string.Empty;
}
