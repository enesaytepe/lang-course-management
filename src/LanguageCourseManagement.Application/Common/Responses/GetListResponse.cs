namespace LanguageCourseManagement.Application.Common.Responses;

public class GetListResponse<T> : BasePageableModel
{
    public IList<T> Items
    {
        get => _items ??= new List<T>(); // Geç başlatma: listeye ilk erişimde boş koleksiyon oluşturulur; null referans hatasını önler
        set => _items = value;
    }

    private IList<T>? _items;
}
