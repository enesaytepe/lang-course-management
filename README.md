# Language Course Management

"Bir Lisan Bir İnsan" dil kursu için geliştirilen, birden fazla şubenin merkezi olarak yönetilebildiği dil kursu otomasyon sistemidir.

## Özellikler

- Şube ve derslik yönetimi
- Öğretmen, dil ve çalışma zamanı yönetimi
- Ders ve öğrenci kayıt işlemleri
- Öğretmen ve derslik müsaitlik kontrolleri
- Öğrenci kayıt ve ödeme takibi
- ASP.NET Core Identity ile giriş ve rol bazlı yetkilendirme
- `SystemAdmin` ve `RegistrationOfficer` kullanıcı rolleri
- MVC arayüzü ve `/api` JSON API desteği

> İlk sürümde ödeme işlemleri yalnızca nakit ve kurs ücretinin tamamının tek seferde tahsil edilmesi şeklinde desteklenmektedir.

## Teknolojiler

- C# / .NET 10
- ASP.NET Core MVC
- Entity Framework Core / Code First
- Microsoft SQL Server
- ASP.NET Core Identity
- Bootstrap
- jQuery / AJAX
- SweetAlert2
- jQuery DataTables
- xUnit / Moq

## Mimari

Proje, Clean Architecture prensiplerinden yararlanan katmanlı bir mimari ile geliştirilmiştir:

```text
src/
├── LanguageCourseManagement.Domain
├── LanguageCourseManagement.Application
├── LanguageCourseManagement.Infrastructure
├── LanguageCourseManagement.MVC
└── LanguageCourseManagement.Shared

tests/
└── LanguageCourseManagement.Tests
```

## Kurulum

Gereksinimler:

- .NET 10 SDK
- SQL Server
- Entity Framework Core CLI

Connection string'i ortam değişkeni olarak tanımlayın:

```powershell
$env:ConnectionStrings__DefaultConnection = "Server=(localdb)\MSSQLLocalDB;Database=LanguageCourseManagement;Trusted_Connection=True;TrustServerCertificate=True"
```

Veritabanını oluşturup uygulamayı çalıştırın:

```powershell
dotnet restore LanguageCourseManagement.slnx

dotnet ef database update `
  --project src/LanguageCourseManagement.Infrastructure `
  --startup-project src/LanguageCourseManagement.MVC

dotnet run --project src/LanguageCourseManagement.MVC
```

## Kullanıcı Rolleri

- `SystemAdmin`: Sistem ve yönetim işlemleri
- `RegistrationOfficer`: Öğrenci kayıt ve tahsilat işlemleri

Development ortamında demo kullanıcıları `Authentication__SeedDemoUsers=true` ayarıyla etkinleştirilebilir.

## Testler

```powershell
dotnet test tests/LanguageCourseManagement.Tests/LanguageCourseManagement.Tests.csproj
```