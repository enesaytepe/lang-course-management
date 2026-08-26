# Skill Oluşturma ve Güncelleme Şartnamesi

Bu belge, lang-course-management projesi için gerekli skill tanımlarını içerir.
Tüm skill'ler `C:\Users\enes_\.config\opencode\skills\` altında oluşturulmalıdır.

## Genel Kurallar

- Her skill bir SKILL.md dosyası ile başlar
- SKILL.md, skill'in ne zaman yükleneceğini, ne yapacağını ve nasıl çalışacağını tanımlar
- Skill'ler transient olarak yüklenir (gerekli olduğunda)
- Script'ler PowerShell (.ps1) formatında olmalıdır
- Template'ler gerçek C#/JS/HTML kodu içermelidir
- Reference'lar best practice ve anti-pattern'leri içermelidir
- Checklist'ler markdown formatında, checkbox listesi şeklinde olmalıdır

## 1. dotnet-developer (Mevcut - Güncelle)

**Durum:** SKILL.md + 3 reference + 1 script mevcut
**Hedef:** Tam kapsamlı skill haline getir

### Eklenecekler:

**templates/ klasörü:**
- `Entity.cs` - Domain entity oluşturma şablonu (BaseEntity, SoftDeletableEntity pattern)
- `EntityConfiguration.cs` - EF Core IEntityTypeConfiguration<T> şablonu
- `Dto.cs` - Request/Response DTO şablonu
- `ServiceInterface.cs` - IService<T> arayüz şablonu
- `ServiceImpl.cs` - Service implementation (async, DI, validation) şablonu
- `MvcController.cs` - MVC Controller (CRUD, antiforgery) şablonu
- `ApiController.cs` - API Controller (REST, ProducesResponseType) şablonu
- `ViewModel.cs` - ViewModel şablonu
- `Validator.cs` - FluentValidation validator şablonu

**scripts/ klasörü:**
- `build-check.ps1` - Build ve test çalıştırma
- `add-migration.ps1` - Migration oluşturma
- `new-entity.ps1` - Yeni entity oluşturma (entity + config + dto + service + controller)

**checklists/ klasörü:**
- `entity-review.md` - Entity inceleme kontrol listesi
- `service-review.md` - Service inceleme kontrol listesi

### SKILL.md Güncellemesi:
- Entity Design Rules section'ı ekle (mevcut AGENTS.md'deki kurallar)
- Migration oluşturma workflow'u ekle
- Service oluşturma adımlarını ekle
- Controller oluşturma adımlarını ekle
- Build/test validation adımlarını ekle

---

## 2. dotnet-review (Mevcut - Güncelle)

**Durum:** SKILL.md mevcut, içerik eksik
**Hedef:** Kapsamlı .NET kod inceleme skill'i

### Eklenecekler:

**references/ klasörü:**
- `async-patterns.md` - async/await usage patterns ve anti-patterns
- `null-safety.md` - Nullable reference type best practices
- `performance.md` - Performance anti-patterns (N+1, eager loading, GC)
- `solid-principles.md` - SOLID violations tespit ve düzeltme rehberi
- `ef-core-patterns.md` - EF Core query optimization patterns

**checklists/ klasörü:**
- `code-review-checklist.md` - Genel kod inceleme kontrol listesi
- `service-review.md` - Service katmanı inceleme kontrol listesi
- `security-review.md` - Güvenlik inceleme kontrol listesi

### SKILL.md Güncellemesi:
- Review workflow'u (hangi dosyadan başlanır, ne kontrol edilir)
- Severity classification (MAJOR/MINOR/OBSERVATION)
- Fix suggestion patterns
- Review report formatı

---

## 3. ef-core-review (Mevcut - Güncelle)

**Durum:** SKILL.md mevcut, içerik eksik
**Hedef:** EF Core-specific inceleme skill'i

### Eklenecekler:

**references/ klasörü:**
- `query-shape.md` - Query shape analysis rehberi
- `include-patterns.md` - Include/navigation loading patterns
- `migration-safety.md` - Migration safety checks
- `constraint-design.md` - Check constraint design patterns
- `index-design.md` - Index design best practices

**checklists/ klasörü:**
- `ef-core-review-checklist.md` - EF Core inceleme kontrol listesi
- `migration-review.md` - Migration inceleme kontrol listesi

---

## 4. api-design (Mevcut - Güncelle)

**Durum:** SKILL.md mevcut, içerik eksik
**Hedef:** REST API design skill'i

### Eklenecekler:

**references/ klasörü:**
- `rest-principles.md` - REST design principles
- `dto-design.md` - DTO design patterns
- `error-handling.md` - Error handling patterns
- `validation.md` - API validation patterns
- `pagination.md` - Pagination patterns

**templates/ klasörü:**
- `ApiController.cs` - RESTful API Controller template
- `ErrorResponse.cs` - Standard error response template
- `PagedRequest.cs` - Pagination request template
- `PagedResponse.cs` - Pagination response template

---

## 5. git-workflow (YENİ - Oluştur)

**Durum:** Mevcut değil
**Hedef:** Git operations için kapsamlı skill

### Oluşturulacaklar:

**SKILL.md:**
- Ne zaman kullanılır: merge, branch, worktree, commit işlemleri
- Merge conflict resolution stratejileri
- Worktree management patterns
- Branch naming conventions
- Commit message formatı (Türkçe doğal cümle)

**scripts/ klasörü:**
- `branch-cleanup.ps1` - Merge edilmiş branch'leri temizle
- `conflict-detect.ps1` - Conflict tespiti
- `worktree-status.ps1` - Tüm worktree'lerin durumu

**references/ klasörü:**
- `merge-strategies.md` - Merge stratejileri (fast-forward, --no-ff, squash)
- `conflict-resolution.md` - Conflict resolution rehberi
- `worktree-management.md` - Worktree best practices

**checklists/ klasörü:**
- `pre-merge-checklist.md` - Merge öncesi kontrol listesi
- `pre-commit-checklist.md` - Commit öncesi kontrol listesi

---

## 6. planning (YENİ - Oluştur)

**Durum:** Mevcut değil
**Hedef:** Görev planlama ve orchestration skill'i

### Oluşturulacaklar:

**SKILL.md:**
- Ne zaman kullanılır: çok görevli, çok aşamalı işler
- Task partitioning stratejileri
- Dependency identification
- Paralel execution eligibility assessment
- Risk assessment
- Plan lifecycle (draft → ready → in-progress → completed)

**templates/ klasörü:**
- `plan-template.md` - Plan dosyası şablonu
- `task-assignment.md` - Subagent atama şablonu

**references/ klasörü:**
- `task-decomposition.md` - Görev ayrıştırma rehberi
- `parallel-execution.md` - Paralel çalıştırma kuralları
- `worktree-isolation.md` - Worktree izolasyonu stratejileri

---

## 7. testing (YENİ - Oluştur)

**Durum:** Mevcut değil
**Hedef:** xUnit + Moq test yazımı skill'i

### Oluşturulacaklar:

**SKILL.md:**
- Ne zaman kullanılır: service/business logic testleri
- AAA (Arrange-Act-Assert) pattern
- Mock setup patterns
- In-memory database testing
- Test naming conventions

**templates/ klasörü:**
- `ServiceTest.cs` - Service unit test template
- `IntegrationTest.cs` - Integration test template
- `TestBase.cs` - Test base class template

**scripts/ klasörü:**
- `test-run.ps1` - Test çalıştırma ve raporlama

**references/ klasörü:**
- `mocking-patterns.md` - Moq mocking patterns
- `in-memory-db.md` - In-memory database testing
- `test-coverage.md` - Test coverage rehberi

---

## Uygulama Sırası

1. **dotnet-developer** (mevcut güncelleme - en kritik)
2. **dotnet-review** (mevcut güncelleme)
3. **git-workflow** (yeni oluşturma)
4. **planning** (yeni oluşturma)
5. **ef-core-review** (mevcut güncelleme)
6. **api-design** (mevcut güncelleme)
7. **testing** (yeni oluşturma)

## Notlar

- Mevcut SKILL.md dosyaları korunmalı, yalnızca genişletilmeli
- Yeni skill'ler için tam klasör yapısı oluşturulmalı
- Tüm script'ler PowerShell formatında olmalı
- Tüm template'ler gerçek kod içermeli
- Tüm reference'lar Chinese değil, English veya Türkçe olmalı
- Checklist'ler checkbox formatında olmalı
