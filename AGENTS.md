# Lang Course Management - Project Rules

## Entity Design Rules

- Entity classes must only contain properties that map directly to database columns.
- No computed properties (e.g., `TotalPaid => ...`, `RemainingBalance => ...`) in entity classes. Compute these in the application/service layer instead.
- Navigation properties must use `List<T>?` pattern (nullable, no initializer). Do NOT use `ICollection<T>` or `new List<T>()`.
- Example correct pattern:
  ```csharp
  public virtual List<Payment>? Payments { get; set; }
  ```
- Example incorrect patterns (DO NOT USE):
  ```csharp
  public ICollection<Payment> Payments { get; set; } = new List<Payment>();  // WRONG
  public virtual List<Payment> Payments { get; set; } = new List<Payment>(); // WRONG
  ```

## Commit Policy

- Before every commit, ALWAYS ask the user for approval.
- When planning includes commit steps, ask the user for the commit message during planning phase, not during execution.
- Never commit without explicit user confirmation.

## Commit Mesajları

- Conventional Commits ön ekleri kullanma (`feat:`, `fix:`, `docs:` vb. yasak).
- Commit mesajları doğrudan Türkçe ve yapılan işi anlatan doğal cümleler olmalıdır.
- Tercihen 1 cümle, gerektiğinde en fazla 2 kısa cümle kullan.
- Örnek mesajlar:
  - `Şube ve derslik yönetimi tamamlandı`
  - `Öğretmen müsaitlik ve ders çakışma kontrolleri eklendi`
  - `Taksitli ödeme altyapısı oluşturuldu`
- `update`, `changes`, `final`, `test123` gibi anlamsız mesajlar kullanma.

## Push ve Git Geçmişi

- Commit izni push izni değildir. Push için ayrı kullanıcı onayı gerekir.
- Aşağıdaki işlemler yalnızca kullanıcı açıkça onay verirse yapılabilir:
  - `git push`
  - `git branch -d/-D`
  - `git reset`
  - `git rebase`
  - `git commit --amend`
  - Force push veya history rewrite

## Plan ve Çalışma Notları

- Oluşturulan geçici plan, task listesi, scratch dosyaları veya çalışma notları commit edilmez.
- `docs/agent/plans/` altındaki dosyalar geliştirme sırasında kullanılır, commit kapsamında tutulmaz.
- Bir dosyanın geçici çalışma notu mu yoksa kalıcı proje dokümantasyonu mu olduğu belirsizse commit etmeden önce kullanıcıya sor.

## Issue Oluşturma

- Bu proje kapsamında hiçbir GitHub/GitLab issue oluşturma/kapatma/güncelleme yapılmaz.
- Kullanıcı açıkça istemedikçe issue, milestone, project board veya ticket oluşturma.

## Subagent Kullanımı ve Paralel Çalışma

- Önemsiz olmayan geliştirme işlerinde, göreve uygun subagent'lar kullan; geniş işi küçük, bağımsız ve ayrı ayrı incelenebilir mikro-görevlere böl.
- Her subagent'a açık kapsam, izin verilen dosya veya yüzey, dışlamalar ve kabul kriterleri içeren ayrı bir atama ver.
- Yalnızca dosyaları, şemaları, public contract'ları, migration'ları ve configuration'ları birbirine değmeyen bağımsız görevleri paralel yürüt.
- Bağımlı veya ortak yüzeyli işleri sırala.
- Tamamlanan her implementation dilimini finalizasyondan önce bağımsız review için yönlendir.

## Model Kullanımı ve Delegasyon

- Uygulama işlerini OpenChamber session'ları üzerinden daha ucuz/ücretsiz modellere delege et.
- "Ex Alpha Free" modeli opencode-go üzerinde küçük ve sınırlı görevler için kullanılabilir.
- Orchestrator yalnızca yönetir ve yönlendirir; uygulama yapmaz.
- Large work into small, independent pieces for parallel delegation.

## Mimari Standartlar

- Clean Architecture: Domain → Application → Infrastructure → MVC
- Domain katmanı EF Core veya Infrastructure bağımlılığı içermez
- Application iş mantığını ve service akışlarını yönetir
- Controller'lar ince tutulur, iş mantığı taşımaz
- MVC controller'lar sayfa/navigasyon sorumluluğundadır
- Create/update/delete mutation'ları AJAX ile /api/* endpoint'lerine gider
- MediatR/CQRS kullanılmaz, service-based yapı korunur

## Kod Standartları

- async/await ve CancellationToken tüm service methodlarında kullanılır
- Nullable reference types aktif yönetilir
- Controller içinde business logic tutulmaz
- Service layer'da validation ve iş kuralları uygulanır
- Repository pattern generic base + specialized method olarak kullanılır
- Para alanlarında decimal kullanılır, float/double kullanılmaz

## Güvenlik

- Hardcoded secret, password veya connection string commit edilmez
- Demo credentials environment variable veya user secrets ile yönetilir
- Authorization yalnız UI'da değil API endpoint'lerinde de uygulanır
- Raw SQL hata mesajları client'a dönülmez

## Test Stratejisi

- Kritik business logic için unit test yazılır
- DB constraint ve transaction testleri integration test gerektirir
- Concurrency testleri integration test gerektirir
- Mock tabanlı unit test, DB constraint testi yerine geçmez
- Kritik testler Skip durumda bırakılmaz
