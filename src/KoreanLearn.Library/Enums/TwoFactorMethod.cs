namespace KoreanLearn.Library.Enums;

/// <summary>兩步驟驗證方式</summary>
public enum TwoFactorMethod
{
    /// <summary>未啟用</summary>
    None = 0,

    /// <summary>TOTP 驗證器 App（Google Authenticator / Microsoft Authenticator）</summary>
    Authenticator = 1,

    /// <summary>Email OTP 驗證碼</summary>
    Email = 2
}
