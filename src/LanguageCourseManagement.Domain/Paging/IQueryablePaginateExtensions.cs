using Microsoft.EntityFrameworkCore;

namespace LanguageCourseManagement.Domain.Paging;

/// <summary>
/// <see cref="IQueryable{T}"/> üzerinde sayfalama uzantı metotları.
/// </summary>
public static class IQueryablePaginateExtensions
{
    /// <summary>
    /// Asenkron olarak sorguyu sayfalayıp <see cref="IPaginate{T}"/> döndürür.
    /// </summary>
    public static async Task<IPaginate<T>> ToPaginateAsync<T>(
        this IQueryable<T> source,
        int index,
        int size,
        int from = 0,
        CancellationToken cancellationToken = default
    )
    {
        if (from > index)
            throw new ArgumentException($"From: {from} > Index: {index}, must from <= Index");

        int count = await source.CountAsync(cancellationToken).ConfigureAwait(false);
        List<T> items = await source.Skip((index - from) * size).Take(size).ToListAsync(cancellationToken).ConfigureAwait(false); // from: sıfır tabanlı olmayan sayfalama için başlangıç ofseti; varsayılan 0

        Paginate<T> list = new()
        {
            Index = index,
            Size = size,
            From = from,
            Count = count,
            Items = items,
            Pages = (int)Math.Ceiling(count / (double)size) // double cast: tam sayı bölümündeki kesme hatasını önler
        };

        return list;
    }

    /// <summary>
    /// Senkron olarak sorguyu sayfalayıp <see cref="IPaginate{T}"/> döndürür.
    /// </summary>
    public static IPaginate<T> ToPaginate<T>(this IQueryable<T> source, int index, int size, int from = 0)
    {
        if (from > index)
            throw new ArgumentException($"From: {from} > Index: {index}, must from <= Index");

        int count = source.Count();
        List<T> items = source.Skip((index - from) * size).Take(size).ToList(); // from: sıfır tabanlı olmayan sayfalama için başlangıç ofseti; varsayılan 0

        Paginate<T> list = new()
        {
            Index = index,
            Size = size,
            From = from,
            Count = count,
            Items = items,
            Pages = (int)Math.Ceiling(count / (double)size) // double cast: tam sayı bölümündeki kesme hatasını önler
        };

        return list;
    }
}
