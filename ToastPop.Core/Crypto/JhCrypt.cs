using System.Security.Cryptography;
using System.Text;

namespace ToastPop.Core.Crypto;

/// <summary>
/// XOR256 스트림 암호화 클래스 (MFC XOR256Stream 변환)
/// </summary>
public sealed class Xor256Stream : IDisposable
{
    private byte[] _key = new byte[256];
    private byte[] _chain = new byte[256];
    private int _blockSize = -1;
    private bool _disposed;

    public int BlockSize => _blockSize;

    public void Initialize(string password)
    {
        Initialize(Encoding.UTF8.GetBytes(password));
    }

    public void Initialize(byte[] password)
    {
        if (password == null || password.Length == 0)
            throw new ArgumentException("Password cannot be null or empty", nameof(password));

        // SHA-256을 사용하여 키 생성
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(password);

        // 256바이트 키 초기화
        for (int i = 0; i < 256; i++)
        {
            _key[i] = hash[i % hash.Length];
        }

        ResetChain();
    }

    public void ResetChain()
    {
        Array.Copy(_key, _chain, 256);
    }

    public byte[] Encrypt(byte[] data)
    {
        if (data == null) return Array.Empty<byte>();

        var result = new byte[data.Length];
        for (int i = 0; i < data.Length; i++)
        {
            int index = i % 256;
            result[i] = (byte)(data[i] ^ _chain[index]);
            _chain[index] = (byte)((_chain[index] + result[i]) & 0xFF);
        }
        return result;
    }

    public byte[] Decrypt(byte[] data)
    {
        if (data == null) return Array.Empty<byte>();

        var result = new byte[data.Length];
        for (int i = 0; i < data.Length; i++)
        {
            int index = i % 256;
            result[i] = (byte)(data[i] ^ _chain[index]);
            _chain[index] = (byte)((_chain[index] + data[i]) & 0xFF);
        }
        return result;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            Array.Clear(_key, 0, _key.Length);
            Array.Clear(_chain, 0, _chain.Length);
            _disposed = true;
        }
    }
}

/// <summary>
/// JH 암호화 유틸리티 (MFC jhcrypt 변환)
/// </summary>
public static class JhCrypt
{
    private static string _cryptoPassword = "TGA_util";

    public static void SetPassword(string password)
    {
        _cryptoPassword = password;
    }

    /// <summary>
    /// 문자열을 암호화하여 16진수 문자열로 반환
    /// </summary>
    public static string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return string.Empty;

        using var stream = new Xor256Stream();
        stream.Initialize(_cryptoPassword);
        stream.ResetChain();

        var data = Encoding.UTF8.GetBytes(plainText);
        var encrypted = stream.Encrypt(data);

        return Convert.ToHexString(encrypted);
    }

    /// <summary>
    /// 16진수 문자열을 복호화하여 원본 문자열로 반환
    /// </summary>
    public static string Decrypt(string hexString)
    {
        if (string.IsNullOrEmpty(hexString))
            return string.Empty;

        try
        {
            using var stream = new Xor256Stream();
            stream.Initialize(_cryptoPassword);
            stream.ResetChain();

            var encrypted = Convert.FromHexString(hexString);
            var decrypted = stream.Decrypt(encrypted);

            // null 문자 제거
            int length = Array.FindIndex(decrypted, b => b == 0);
            if (length < 0) length = decrypted.Length;

            return Encoding.UTF8.GetString(decrypted, 0, length);
        }
        catch
        {
            return string.Empty;
        }
    }
}
