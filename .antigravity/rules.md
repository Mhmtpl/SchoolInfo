## MİMARİ KURALLAR
- Proje Clean Architecture + DDD ile geliştirilmektedir
- Katmanlar: SchoolInfo.Core, SchoolInfo.Infrastructure, SchoolInfo.API, SchoolInfo.Tests
- Katman bağımlılık sırası: API → Core, Infrastructure → Core
- API katmanı hiçbir zaman Infrastructure'a direkt erişemez
- Her katman sadece bir alt katmanı referans alır

## PROJE YAPISI
SchoolInfo/
├── src/
│   ├── SchoolInfo.Core/
│   │   ├── Entities/
│   │   ├── ValueObjects/
│   │   ├── Enums/
│   │   ├── Events/
│   │   ├── Exceptions/
│   │   ├── Interfaces/
│   │   └── Features/
│   │       ├── DailyRecords/
│   │       ├── MealRecords/
│   │       ├── Activities/
│   │       ├── DailySummary/
│   │       ├── Students/
│   │       ├── Classrooms/
│   │       ├── Schools/
│   │       └── Users/
│   ├── SchoolInfo.Infrastructure/
│   │   ├── Persistence/
│   │   │   ├── AppDbContext.cs
│   │   │   ├── Configurations/
│   │   │   └── Repositories/
│   │   ├── AI/
│   │   ├── Notifications/
│   │   ├── Auth/
│   │   └── BackgroundServices/
│   └── SchoolInfo.API/
│       ├── Endpoints/
│       ├── Middleware/
│       └── Extensions/
└── tests/
    └── SchoolInfo.Tests/

## DDD KURALLARI
- Entity'lerde tüm setter'lar private olacak
- Değişim sadece Entity metotları üzerinden yapılacak
- Value Object'ler immutable record olacak
- Domain Event'ler MediatR ile fırlatılacak
- Repository'ler sadece Domain nesnesi döndürecek
- DTO dönüşümü sadece Application/Core katmanında yapılacak

## KOD YAZIM KURALLARI
- .NET 10 kullanılacak
- Minimal API kullanılacak, Controller yazılmayacak
- Her endpoint grubu IEndpoint interface'ini implement edecek
- MediatR ile CQRS pattern uygulanacak
- FluentValidation ile tüm command'ler validate edilecek
- IUnitOfWork kullanılmayacak, direkt DbContext.SaveChangesAsync()
- Her servis için interface + implementation ayrımı yapılacak
- Dependency Injection her zaman interface üzerinden olacak

## VERİTABANI KURALLARI
- PostgreSQL kullanılacak
- ORM olarak EF Core 10 kullanılacak
- Her tablo konfigürasyonu ayrı Fluent API dosyasında olacak
- Value Object'ler owned entity olarak map edilecek
- Her tabloda school_id (multi-tenant) olacak
- Migration'lar EF Core Code First ile yönetilecek
- Connection string appsettings.json'dan okunacak

## MULTI-TENANT KURALLARI
- Her repository metodunda school_id filtresi zorunlu
- JWT token içinde school_id claim'i taşınacak
- ICurrentUserService her zaman SchoolId döndürecek
- Farklı okul verileri kesinlikle karışmayacak
- Okul yönetim ekranı henüz geliştirilmeyecek

## AI ENTEGRASYON KURALLARI
- Semantic Kernel kullanılmayacak
- Microsoft Agent Framework 1.0 kullanılacak
- NuGet: Microsoft.Agents.AI
- Agent konfigürasyonu appsettings.json'dan okunacak:
  "AgentFramework": {
    "Endpoint": "",
    "ApiKey": "",
    "Model": "gpt-4o"
  }
- Agent hata verirse fallback metin üretilecek
- Agent instance DI ile singleton register edilecek

## GÜVENLİK KURALLARI
- JWT + Refresh Token kullanılacak
- Password hash için BCrypt kullanılacak
- Hiçbir değer hardcode yazılmayacak
- API key, connection string, secret appsettings'de olacak
- Her endpoint RequireAuthorization ile korunacak
- Rol bazlı yetki: admin, teacher, parent
- Teacher sadece kendi sınıfına erişebilir
- Parent sadece kendi çocuğuna erişebilir

## HATA YÖNETİMİ
- DomainException → 400 Bad Request
- ValidationException → 422 Unprocessable Entity
- NotFoundException → 404 Not Found
- UnauthorizedException → 403 Forbidden
- Diğer → 500 Internal Server Error
- Hata response formatı:
  { "success": false, "message": "...", "errors": [] }
- Hata mesajları Türkçe olacak

## YORUM VE DOKÜMANTASYON
- Tüm public class ve metotlara Türkçe XML yorum eklenecek
- Karmaşık iş mantığı satır içi Türkçe yorum ile açıklanacak
- appsettings.json'daki tüm alanlar açıklamalı olacak

## YASAKLAR
- Controller kullanmak yasak
- IUnitOfWork kullanmak yasak
- Semantic Kernel kullanmak yasak
- Hardcode değer yazmak yasak
- Farklı okulların verisine erişmek yasak
- SaveChanges'i Repository içinde çağırmak yasak
- Entity setter'larını public yapmak yasak
- Infrastructure'a Core dışından direkt erişmek yasak

## TEST KURALLARI
- xUnit kullanılacak
- Moq ile mock'lama yapılacak
- FluentAssertions kullanılacak
- Her pozitif test için negatif test yazılacak
- Test isimleri Türkçe olacak
- Arrange / Act / Assert yapısı zorunlu
