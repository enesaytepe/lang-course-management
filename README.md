# Dil Kursu Otomasyon Yazılımı

**Bir Lisan Bir İnsan** dil kursu için geliştirilen, birden fazla şubenin merkezi olarak yönetildiği dil kursu otomasyon sistemidir.

## Özellikler

### Şube ve Tesis Yönetimi
- Şube CRUD (ad, adres, telefon, koordinat)
- Şubelere ait toplu taşıma ve özel araç ulaşım talimatları
- Sosyal olanak yönetimi (çoklu ilişki ile şubelere tanımlama)
- Şube bazlı veri filtreleme (üst çubuk şube seçici)
- Derslik yönetimi (şube içinde kapasite ile birlikte)

### Öğretmen Yönetimi
- Öğretmen CRUD (ad, soyad, telefonlar, işe başlama tarihi)
- Öğretebileceği diller (çoklu ilişki)
- Ders verebileceği şubeler (çoklu ilişki)
- Haftalık müsaitlik zamanları (gün/saat aralığı)
- Çakışma kontrolü: ders açarken müsaitliği olmayan öğretmenler otomatik filtrelenir

### Kurs ve Ders Programı
- Kurs oluşturma ve düzenleme (dil, seviye, şube, öğretmen, derslik, kapasite, ücret)
- Kur seviyesi yönetimi (dillere bağlı seviye hiyerarşisi)
- Haftalık ders programı (gün/saat/başlangıç-bitiş)
- Otomatik öğretmen ve derslik uygunluk kontrolü (seçilen programa göre müsait öğretmen ve boş derslik listesi)
- Kurs durumu yönetimi (Açık/Kapalı/Tamamlandı)

### Öğrenci Kayıt (Enrollment)
- Öğrenci CRUD (ad, soyad, telefonlar, e-posta, adres)
- Öğrenciyi herhangi bir şubedeki kursa kayıt etme
- Kayıt + nakit tahsilat tek işlemde (atomik transaction)
- Kayıt + taksitli ödeme planı oluşturma
- Tekrarlanabilirlik koruması (aynı öğrenci + aynı ders = tek kayıt)

### Ödeme Sistemi
- Peşin ve taksitli ödeme desteği
- Taksit planı oluşturma (2-12 taksit, aylık ödeme)
- Taksit durumu takibi (Ödendi/Bekliyor/Gecikmiş)
- Gecikmiş taksitlerin otomatik işaretlenmesi
- Ödeme geçmişi ve detay görünümü
- Settlement koruması: ödenen/iptal edilen ödeme değiştirilemez ve silinemez

### Dashboard ve İstatistikler
- Aktif şube, derslik, öğretmen, öğrenci, kurs sayıları
- Toplam ve aktif kayıt sayıları
- Toplam tahsilat ve aylık gelir
- Bekleyen ödeme ve gecikmiş taksit sayıları

### Güvenlik ve Yetkilendirme
- ASP.NET Core Identity ile giriş (Login)
- Rol bazlı yetkilendirme:
  - **SystemAdmin**: Tüm sistem yönetimi
  - **RegistrationOfficer**: Kayıt ve tahsilat işlemleri
- API endpoint'lerinde backend yetkilendirme
- Demo kullanıcı seeding (geliştirme ortamı için)

### Altyapı
- Soft delete (fiziksel silme yerine arşivleme)
- Audit log (tüm CRUD işlemleri kayıt altına alınır)
- Global exception middleware (hata mesajları API ve MVC için tutarlı)
- Soft delete query filter (EF Core global filtre)
- Idempotent payment (tekrarlanan istek koruması)

## Teknolojiler

### Backend
- C# / .NET 10
- ASP.NET Core MVC
- Entity Framework Core 10 (Code First)
- SQL Server
- ASP.NET Core Identity
- AutoMapper (ProjectTo ile projection)
- FluentValidation

### Frontend
- HTML5 / CSS3
- Bootstrap
- jQuery
- jQuery DataTables (server-side paging)
- AJAX (JSON API ile iletişim)
- SweetAlert2 (bildirim ve uyarılar)
- Custom JavaScript (sayfa bazlı modüler yapı)

### Mimari
Clean Architecture prensiplerine uygun katmanlı mimari:

```text
Domain  →  Application  →  Infrastructure  →  MVC
(Entity)    (Service/Logic)  (EF Core/Identity)  (UI)
```

- **Domain**: Entity, enum, repository arayüzleri — dış bağımlılık yok
- **Application**: İş mantığı, servisler, DTO'lar, validasyon
- **Infrastructure**: EF Core, repository implementasyonları, Identity, seeding
- **MVC**: Controller (ince), View, JavaScript, middleware

### Frontend Mimari
- Sayfa bazlı JavaScript modülleri (`/js/pages/*-page.js`)
- Ortak yardımcı fonksiyonlar (`/js/core/common.js`)
- Şube seçici (`/js/branch-selector.js`)
- Tüm CRUD işlemleri AJAX ile `/api/*` endpoint'lerine gider
- MVC controller'lar sayfa/navigasyon sorumluluğundadır

## Proje Yapısı

```text
src/
├── LanguageCourseManagement.Domain          # Entity, enum, repository arayüzleri
├── LanguageCourseManagement.Application     # Servisler, DTO'lar, validasyon, mapping
├── LanguageCourseManagement.Infrastructure  # EF Core, repository, Identity, seeding
├── LanguageCourseManagement.MVC             # Controller, View, JavaScript, middleware
└── LanguageCourseManagement.Shared          # Paylaşılan yardımcı sınıflar

tests/
└── LanguageCourseManagement.Tests           # Unit ve integration testler
```

## Kullanıcı Rolleri

| Rol | Yetki |
|-----|-------|
| `SystemAdmin` | Tüm sistem yönetimi (şube, derslik, öğretmen, kurs, ayarlar) |
| `RegistrationOfficer` | Öğrenci kayıt, enrollment, ödeme/tahsilat işlemleri |

## Kurulum

### Gereksinimler
- .NET 10 SDK
- SQL Server (LocalDB veya tam kurulum)
- Entity Framework Core CLI

### Adımlar

```powershell
# Depolari yukleyin
dotnet restore LanguageCourseManagement.slnx

# Connection string'i ayarlayin
$env:ConnectionStrings__DefaultConnection = "Server=(localdb)\MSSQLLocalDB;Database=LanguageCourseManagement;Trusted_Connection=True;TrustServerCertificate=True"

# Veritabani olusturun
dotnet ef database update `
  --project src/LanguageCourseManagement.Infrastructure `
  --startup-project src/LanguageCourseManagement.MVC

# Uygulamayi calistirin
dotnet run --project src/LanguageCourseManagement.MVC
```

### Demo Kullanıcı
Geliştirme ortamında demo kullanıcı seeding'i için:
```powershell
$env:Authentication__SeedDemoUsers = "true"
```

### Demo Kullanici Sifreleri (User Secrets)

Demo kullanici sifreleri `appsettings.Development.json` dosyasinda tutulmaz. Asagidaki komutlarla User Secrets'a ekleyin:

```powershell
dotnet user-secrets set "Authentication:DemoUsers:SystemAdmin:Password" "Admin@123" --project src/LanguageCourseManagement.MVC
dotnet user-secrets set "Authentication:DemoUsers:RegistrationOfficer:Password" "Officer@123" --project src/LanguageCourseManagement.MVC
```

## Testler

```powershell
# Tüm testleri çalıştırın
dotnet test tests/LanguageCourseManagement.Tests/LanguageCourseManagement.Tests.csproj

# Sadece unit testler
dotnet test --filter "FullyQualifiedName!~EnrollmentTransactionTests"

# Integration testler (SQL Server gerektirir)
dotnet test --filter "EnrollmentTransactionTests"
```