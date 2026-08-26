namespace LanguageCourseManagement.Domain.Enums;

/// <summary>
/// Kayıt ödeme türü: nakit tek seferde veya taksitli ödeme
/// </summary>
public enum PaymentType
{
    /// <summary>
    /// Nakit tek seferde ödeme
    /// </summary>
    Cash = 1,

    /// <summary>
    /// Taksitli ödeme
    /// </summary>
    Installment = 2
}
