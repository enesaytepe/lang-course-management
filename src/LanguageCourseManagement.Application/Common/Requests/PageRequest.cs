namespace LanguageCourseManagement.Application.Common.Requests;

public class PageRequest
{
    public int PageIndex { get; set; } = 0;
    public int PageSize { get; set; } = 10;

    public void Normalize()
    {
        PageIndex = Math.Max(PageIndex, 0);
        if (PageSize is < 1 or > 100)
            PageSize = 20;
    }
}
