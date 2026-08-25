namespace LanguageCourseManagement.Domain.Paging;

/// <summary>
/// <see cref="IPaginate{T}"/> arayüzünün varsayılan EF Core implementasyonu.
/// </summary>
public class Paginate<T> : IPaginate<T>
{
    public Paginate(IEnumerable<T> source, int index, int size, int from)
    {
        if (from > index)
            throw new ArgumentException($"indexFrom: {from} > pageIndex: {index}, must indexFrom <= pageIndex");

        Index = index;
        Size = size;
        From = from;

        if (source is IQueryable<T> queryable)
        {
            Count = queryable.Count();
            Items = queryable.Skip((Index - From) * Size).Take(Size).ToList();
        }
        else
        {
            T[] enumerable = source as T[] ?? source.ToArray();
            Count = enumerable.Length;
            Items = enumerable.Skip((Index - From) * Size).Take(Size).ToList();
        }

        Pages = (int)Math.Ceiling(Count / (double)Size);
    }

    public Paginate()
    {
        Items = Array.Empty<T>();
    }

    public int From { get; set; }
    public int Index { get; set; }
    public int Size { get; set; }
    public int Count { get; set; }
    public int Pages { get; set; }
    public IList<T> Items { get; set; }
    public bool HasPrevious => Index - From > 0;
    public bool HasNext => Index - From + 1 < Pages;
}
