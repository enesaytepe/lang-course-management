# Dil Kursu Otomasyon Yazilimi

Bir Lisan Bir Insan dil kursu icin gelistirilen, birden fazla subenin merkezi olarak yonetilebildigi dil kursu otomasyon sistemidir.

## Ozellikler

- Sube ve derslik yonetimi
- Ogretmen ve musaitlik yonetimi
- Kurs olusturma ve planlama
- Ogrenci kayit ve enrollment yonetimi
- Taksitli odeme altyapisi
- Dashboard ve raporlama
- ASP.NET Core Identity ile giris ve rol bazli yetkilendirme
- MVC arayuzu ve `/api` JSON API destegi

## Teknolojiler

- .NET 10, ASP.NET Core MVC
- Entity Framework Core + SQL Server
- AutoMapper (ProjectTo)
- Dapper (aggregate queries)
- ASP.NET Core Identity
- jQuery DataTables

## Proje Yapisi

```text
src/
├── LanguageCourseManagement.Domain       # Entity ve enum'lari icerir
├── LanguageCourseManagement.Application  # Service arayuzleri ve is mantigi
├── LanguageCourseManagement.Infrastructure  # EF Core, Repository, Identity
├── LanguageCourseManagement.MVC          # Controller, View, JavaScript
└── LanguageCourseManagement.Shared

tests/
└── LanguageCourseManagement.Tests
```

## Mimari

Proje, Clean Architecture prensiplerinden yararlanan katmanli bir mimari ile gelistirilmistir. Projection pattern ile veri transferi saglanmaktadir.

```text
Domain  -->  Application  -->  Infrastructure  -->  MVC
 (Entity)    (Service/Logic)   (EF Core/Identity)   (UI)
```

## Kurulum

Gereksinimler:

- .NET 10 SDK
- SQL Server
- Entity Framework Core CLI

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

## Kullanici Rolleri

- `SystemAdmin`: Sistem ve yonetim islemleri
- `RegistrationOfficer`: Ogrenci kayit ve tahsilat islemleri

Development ortaminda demo kullanici `Authentication__SeedDemoUsers=true` ayariyla etkinlestirilebilir.

### Demo Kullanici Sifreleri (User Secrets)

Demo kullanici sifreleri `appsettings.Development.json` dosyasinda tutulmaz. Asagidaki komutlarla User Secrets'a ekleyin:

```powershell
dotnet user-secrets set "Authentication:DemoUsers:SystemAdmin:Password" "Admin@123" --project src/LanguageCourseManagement.MVC
dotnet user-secrets set "Authentication:DemoUsers:RegistrationOfficer:Password" "Officer@123" --project src/LanguageCourseManagement.MVC
```

## Testler

```powershell
dotnet test tests/LanguageCourseManagement.Tests/LanguageCourseManagement.Tests.csproj
```
