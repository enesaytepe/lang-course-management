namespace LanguageCourseManagement.Domain.Enums;

/// <summary>
/// Kayıt tahsilatında kullanılan yöntem
/// </summary>
public enum PaymentMethod
{
    /// <summary>
    /// Nakit tahsilat
    /// </summary>
    Cash = 1,

    /// <summary>
    /// Kredi kartı ile tahsilat
    /// </summary>
    CreditCard = 2,

    /// <summary>
    /// Banka havalesi ile tahsilat
    /// </summary>
    BankTransfer = 3
}
