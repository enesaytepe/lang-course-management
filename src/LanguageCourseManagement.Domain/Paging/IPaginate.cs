namespace LanguageCourseManagement.Domain.Paging;

/// <summary>
/// Sayfalanış sorgu sonuçları için arayüz.
/// </summary>
public interface IPaginate<T>
{
    /// <summary>Sayfalama başlangıç ofseti (genellikle 0).</summary>
    int From { get; }
    /// <summary>Mevcut sayfa indeksi.</summary>
    int Index { get; }
    /// <summary>Sayfa başına öğe sayısı.</summary>
    int Size { get; }
    /// <summary>Toplam öğe sayısı.</summary>
    int Count { get; }
    /// <summary>Toplam sayfa sayısı.</summary>
    int Pages { get; }
    /// <summary>Geçerli sayfadaki öğeler.</summary>
    IList<T> Items { get; }
    /// <summary>Önceki sayfa var mı?</summary>
    bool HasPrevious { get; }
    /// <summary>Sonraki sayfa var mı?</summary>
    bool HasNext { get; }
}
