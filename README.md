# 🏫 SchoolInfo — Okul Öncesi Bilgi Sistemi

> Anaokulları ve kreşler için AI destekli, multi-tenant okul yönetim platformu.  
> Öğretmen-veli iletişimini güçlendirir, günlük rutinleri takip eder ve yapay zeka ile kişiselleştirilmiş özetler üretir.

---

## 🚀 Kullanılan Teknolojiler

### 🖥️ Backend & Framework
| Teknoloji | Versiyon | Kullanım Amacı |
|---|---|---|
| **.NET** | 10.0 | Ana platform |
| **Minimal API** | .NET 10 built-in | HTTP endpoint'leri (Controller yok) |
| **C#** | Latest | Programlama dili |

### 🗄️ Veritabanı
| Teknoloji | Versiyon | Kullanım Amacı |
|---|---|---|
| **PostgreSQL** | Latest | Ana ilişkisel veritabanı |
| **EF Core** | 9.x → 10 | ORM (Code First, Fluent API) |
| **Npgsql** | 9.x | PostgreSQL EF Core provider |

### 🤖 Yapay Zeka
| Teknoloji | Versiyon | Kullanım Amacı |
|---|---|---|
| **Microsoft.Agents.AI** | 1.0 (preview) | AI Agent orkestrasyon framework |
| **Microsoft.Agents.AI.Foundry** | 1.0 (preview) | Azure AI Foundry bağlantısı |
| **Azure.AI.Projects** | preview | Azure AI proje yönetimi |
| **Azure.Identity** | latest | Azure kimlik doğrulama |
| **GPT-4o** | — | Günlük özet üretimi için LLM modeli |

### 🔐 Güvenlik & Auth
| Teknoloji | Versiyon | Kullanım Amacı |
|---|---|---|
| **JWT Bearer** | 9.x | Token tabanlı kimlik doğrulama |
| **BCrypt.Net-Next** | 4.2.0 | Şifre hashleme |
| **Role-Based Auth** | .NET built-in | Admin / Teacher / Parent rolleri |

### 📦 Uygulama Katmanı Kütüphaneleri
| Teknoloji | Versiyon | Kullanım Amacı |
|---|---|---|
| **MediatR** | 12.x | CQRS mediator implementasyonu |
| **FluentValidation** | 11.x | Command validasyon kuralları |
| **Microsoft.Extensions.Logging** | 9.x | Structured logging |

### 📬 Bildirimler
| Teknoloji | Versiyon | Kullanım Amacı |
|---|---|---|
| **Firebase Admin SDK** | 3.x | Push notification (FCM) |

### 🧪 Test
| Teknoloji | Versiyon | Kullanım Amacı |
|---|---|---|
| **xUnit** | — | Unit test framework |
| **Moq** | — | Mock/stub kütüphanesi |
| **FluentAssertions** | — | Okunabilir assertion'lar |

---

## 📊 İş Akışları (Visual Workflows)

### 1. 🏫 Sistem Kurulumu ve Yönetim
Admin tarafından okulun ve sınıfların yapılandırılması.

```mermaid
graph TD
    A[Admin Girişi] --> B[Okul Tanımlama]
    B --> C[Sınıf Oluşturma]
    C --> D[Öğretmen Hesabı Oluşturma]
    D --> E[Öğretmeni Sınıfa Atama]
    E --> F[Öğrenci Kaydı ve Sınıf Ataması]
    F --> G[Veli Hesabı Oluşturma]
    G --> H[Veli ve Öğrenciyi Eşleştirme]
    style A fill:#f9f,stroke:#333,stroke-width:2px
```

### 2. 📝 Günlük Operasyon ve AI Özetleme
Öğretmenin veri girişi ve sistemin gün sonunda AI özeti üretmesi.

```mermaid
sequenceDiagram
    participant T as Öğretmen
    participant API as SchoolInfo API
    participant AI as AI Agent (GPT-4o)

    T->>API: Günlük Kayıtlar (Yemek, Uyku, Aktivite)
    API->>API: Verileri Konsolide Et
    Note over API, AI: Gün Sonu Tetikleyicisi
    API->>AI: Öğrenci Verilerini Gönder
    AI-->>API: Kişiselleştirilmiş Gün Özeti
    API->>API: Velilere Bildirimi Hazırla
```

### 3. 👨‍👩‍👧 Veli Bilgilendirme
Velinin sistem üzerinden çocuğunu takip etmesi.

```mermaid
graph LR
    P[Veli] -->|Giriş| APP[Mobil/Web App]
    APP -->|İncele| Y[Yemek & Uyku]
    APP -->|İncele| A[Aktiviteler]
    APP -->|Oku| S[AI Gün Sonu Özeti]
    style S fill:#bbf,stroke:#333,stroke-width:2px
```

---

## 🏗️ Mimari Yapı — Clean Architecture + DDD

Proje **Clean Architecture** prensiplerine ve **Domain-Driven Design (DDD)** kurallarına uygun olarak 4 ana katmandan oluşur. Bağımlılık yönü her zaman içe doğrudur: **API → Application → Domain**. Infrastructure da sadece Domain'e bağlıdır.

```mermaid
graph TB
    subgraph API["🌐 SchoolInfo.API (En Dış Katman)"]
        EP[Endpoints]
        MW[Middleware]
        EXT[Extensions]
    end

    subgraph APP["⚙️ SchoolInfo.Application"]
        CMD[Commands]
        QRY[Queries]
        DTO[DTOs]
        VAL[Validators]
        IFACE[Interfaces]
    end

    subgraph DOMAIN["🎯 SchoolInfo.Domain (En İç Katman)"]
        ENT[Entities]
        VO[Value Objects]
        EVT[Domain Events]
        EXC[Domain Exceptions]
        ENUM[Enums]
    end

    subgraph INFRA["🔧 SchoolInfo.Infrastructure"]
        DB[(PostgreSQL\nEF Core)]
        AI[AI Agent\nMicrosoft.Agents.AI]
        AUTH[JWT Auth\nBCrypt]
        NOTIF[Firebase\nNotifications]
        BG[Background\nServices]
    end

    API --> APP
    API --> DOMAIN
    APP --> DOMAIN
    INFRA --> DOMAIN
    INFRA --> APP

    style DOMAIN fill:#1a1a2e,color:#e94560,stroke:#e94560
    style APP fill:#16213e,color:#0f3460,stroke:#0f3460
    style API fill:#0f3460,color:#e94560,stroke:#e94560
    style INFRA fill:#533483,color:#fff,stroke:#533483
```

### Katman Sorumlulukları

| Katman | Proje | Sorumluluk |
|---|---|---|
| **Domain** | `SchoolInfo.Domain` | Entity'ler, Value Object'ler, Domain Event'ler, iş kuralları |
| **Application** | `SchoolInfo.Application` | CQRS handlers, DTO'lar, validasyon, interface tanımları |
| **Infrastructure** | `SchoolInfo.Infrastructure` | DB, AI, Auth, Bildirim implementasyonları |
| **API** | `SchoolInfo.API` | Minimal API endpoint'leri, middleware, DI kaydı |

---

## 🎨 Tasarım Desenleri (Design Patterns)

### 1. 🏛️ Clean Architecture

Bağımlılıklar her zaman dıştan içe akar. Domain katmanı hiçbir dış bağımlılık taşımaz.

```mermaid
graph LR
    A[🌐 API] -->|bağlı| B[⚙️ Application]
    B -->|bağlı| C[🎯 Domain]
    D[🔧 Infrastructure] -->|bağlı| C
    D -->|bağlı| B
    A -.->|YASAK| D

    style C fill:#e94560,color:#fff,stroke:#c73652
    style A fill:#0f3460,color:#fff
    style B fill:#16213e,color:#fff
    style D fill:#533483,color:#fff
```

---

### 2. 📨 CQRS (Command Query Responsibility Segregation)

Okuma ve yazma işlemleri birbirinden ayrılmıştır. Her feature kendi `Commands/` ve `Queries/` klasörüne sahiptir.

```mermaid
graph TD
    REQ[HTTP Request] --> API[Minimal API Endpoint]
    API -->|MediatR.Send| MED{MediatR\nMediator}

    MED -->|Command| CH[Command Handler\n✏️ Yazma İşlemi]
    MED -->|Query| QH[Query Handler\n📖 Okuma İşlemi]

    CH --> DB[(PostgreSQL\nWrite Model)]
    CH --> EVT[Domain Events\nFirlat]

    QH --> DB2[(PostgreSQL\nRead Model)]
    QH --> DTO[DTO Response]

    EVT --> NOT[Bildirim / AI]

    style MED fill:#e94560,color:#fff
    style CH fill:#0f3460,color:#fff
    style QH fill:#16213e,color:#fff
```

**Örnek Feature Yapısı (DailySummary):**
```
Features/
└── DailySummary/
    ├── Commands/
    │   └── GenerateDailySummary/
    │       ├── GenerateDailySummaryCommand.cs
    │       ├── GenerateDailySummaryHandler.cs
    │       └── GenerateDailySummaryValidator.cs
    └── Queries/
        └── GetDailySummary/
            ├── GetDailySummaryQuery.cs
            └── GetDailySummaryHandler.cs
```

---

### 3. 🗃️ Repository Pattern

Domain nesneleri her zaman repository arayüzü üzerinden erişilir. Infrastructure detayları Application katmanından gizlenir.

```mermaid
graph LR
    HAND[Application\nHandler] -->|interface| REPO_I[IStudentRepository\n Domain Interface]
    REPO_I -.->|DI ile çözümlenir| REPO_IMPL[StudentRepository\nInfrastructure Impl]
    REPO_IMPL --> EF[EF Core\nDbContext]
    EF --> DB[(PostgreSQL)]

    style REPO_I fill:#e94560,color:#fff
    style REPO_IMPL fill:#533483,color:#fff
```

---

### 4. 🌐 Domain-Driven Design (DDD)

#### Aggregate Yapısı

```mermaid
graph TD
    subgraph AGG1["Aggregate: School"]
        SCH[School\n🏫 Root]
        SCH --> CLS[Classroom]
        CLS --> CT[ClassroomTeachers\nJoin Table]
    end

    subgraph AGG2["Aggregate: Student"]
        STU[Student\n👦 Root]
        STU --> DR[DailyRecord]
        DR --> MR[MealRecord]
        DR --> ACT[Activity]
        DR --> DS[DailySummary\n🤖 AI Generated]
    end

    subgraph AGG3["Aggregate: User"]
        USR[User\n👤 Root]
        USR --> PAR[Parent Role]
        USR --> TEA[Teacher Role]
        USR --> ADM[Admin Role]
    end

    style SCH fill:#e94560,color:#fff
    style STU fill:#0f3460,color:#fff
    style USR fill:#533483,color:#fff
```

#### DDD Kuralları
- **Entity'ler:** Tüm setter'lar `private` — değişim sadece domain metotları ile
- **Value Object'ler:** `immutable record` — EF Owned Entity olarak map edilir
- **Domain Events:** MediatR ile fırlatılır, iş kuralı ihlalleri `DomainException` üretir

---

### 5. 🔒 Multi-Tenant Pattern

Her okul verisi `school_id` ile tamamen izole edilmiştir.

```mermaid
sequenceDiagram
    participant C as İstemci
    participant MW as TenantMiddleware
    participant JWT as JWT Token
    participant REPO as Repository
    participant DB as PostgreSQL

    C->>MW: HTTP Request + Bearer Token
    MW->>JWT: Token'ı Parse Et
    JWT-->>MW: school_id claim
    MW->>MW: ICurrentUserService'e SchoolId Set Et
    MW->>REPO: Handler → Repository çağrısı
    REPO->>DB: WHERE school_id = {schoolId}
    DB-->>REPO: Sadece ilgili okul verisi
    REPO-->>C: Güvenli Response
```

---

### 6. 🤖 AI Agent Pattern (Microsoft Agent Framework)

Gün sonu tetikleyicisi ile AI agent, her öğrenci için kişiselleştirilmiş özet üretir.

```mermaid
sequenceDiagram
    participant BG as Background Service\n(Gün Sonu Trigger)
    participant AG as AI Agent\n(Microsoft.Agents.AI)
    participant AZ as Azure AI Foundry\n(GPT-4o)
    participant DB as PostgreSQL
    participant FCM as Firebase\nNotifications

    BG->>DB: Günlük kayıtları çek
    DB-->>BG: DailyRecord + MealRecord + Activities
    BG->>AG: Öğrenci verilerini gönder
    AG->>AZ: GPT-4o'ya prompt
    AZ-->>AG: Kişiselleştirilmiş özet metni
    AG-->>BG: DailySummary

    alt Başarılı
        BG->>DB: DailySummary kaydet
        BG->>FCM: Veliye push notification gönder
    else AI Hatası
        BG->>DB: Fallback metin ile kaydet
    end
```

---

### 7. 🛡️ Hata Yönetimi Pattern

Tüm exception'lar merkezi middleware tarafından yakalanır ve standart formata dönüştürülür.

```mermaid
graph TD
    EXC[Exception Fırlatıldı] --> MW{Exception\nMiddleware}

    MW -->|DomainException| R1[400 Bad Request]
    MW -->|ValidationException| R2[422 Unprocessable Entity]
    MW -->|NotFoundException| R3[404 Not Found]
    MW -->|UnauthorizedException| R4[403 Forbidden]
    MW -->|Diğer| R5[500 Internal Server Error]

    R1 & R2 & R3 & R4 & R5 --> FMT["{ success: false,\n  message: '...',\n  errors: [] }"]

    style MW fill:#e94560,color:#fff
    style FMT fill:#1a1a2e,color:#eee
```

---

## 📁 Proje Klasör Yapısı

```
SchoolInfo/
├── src/
│   ├── SchoolInfo.Domain/           🎯 DDD Domain Katmanı
│   │   ├── Entities/                   (School, Classroom, Student, User...)
│   │   ├── ValueObjects/               (immutable records)
│   │   ├── Enums/                      (UserRole, MealType, SleepStatus...)
│   │   ├── Events/                     (Domain Events - MediatR)
│   │   ├── Exceptions/                 (DomainException, NotFoundException...)
│   │   └── Interfaces/                 (IRepository<T>, ICurrentUserService...)
│   │
│   ├── SchoolInfo.Application/      ⚙️ CQRS + İş Mantığı
│   │   ├── Common/                     (BaseResponse, PaginatedResult...)
│   │   └── Features/                   (Her feature kendi klasöründe)
│   │       ├── Students/
│   │       │   ├── Commands/
│   │       │   └── Queries/
│   │       ├── Classrooms/
│   │       ├── DailyRecords/
│   │       ├── MealRecords/
│   │       ├── Activities/
│   │       ├── DailySummary/
│   │       ├── Schools/
│   │       └── Users/
│   │
│   ├── SchoolInfo.Infrastructure/   🔧 Dış Servis Implementasyonları
│   │   ├── Persistence/
│   │   │   ├── AppDbContext.cs
│   │   │   ├── Configurations/         (Fluent API EF Config)
│   │   │   └── Repositories/           (Repository implementasyonları)
│   │   ├── AI/                         (Microsoft.Agents.AI entegrasyonu)
│   │   ├── Auth/                       (JWT + BCrypt)
│   │   ├── Notifications/              (Firebase FCM)
│   │   ├── BackgroundServices/         (Gün sonu AI trigger)
│   │   └── DependencyInjection.cs
│   │
│   └── SchoolInfo.API/              🌐 Minimal API Katmanı
│       ├── Endpoints/                  (IEndpoint implement eden gruplar)
│       ├── Middleware/                 (Auth, Tenant, Exception handling)
│       └── Extensions/                 (WebApplication extension'ları)
│
└── tests/
    └── SchoolInfo.Tests/            🧪 xUnit + Moq + FluentAssertions
```

---

## 📊 Domain Model (Varlık İlişkileri)

```mermaid
erDiagram
    School ||--o{ Classroom : "sahip olur"
    School ||--o{ User : "barındırır"

    Classroom }o--o{ User : "ClassroomTeachers (çoktan-çoğa)"
    Classroom ||--o{ Student : "içerir"

    Student ||--o{ DailyRecord : "sahiptir"
    Student }o--|| User : "Parent (veli)"

    DailyRecord ||--o{ MealRecord : "içerir"
    DailyRecord ||--o{ Activity : "içerir"
    DailyRecord ||--o| DailySummary : "üretir (AI)"
```

---

## 🔐 Güvenlik & Yetkilendirme

```mermaid
graph TD
    subgraph Roller["👥 Kullanıcı Rolleri"]
        ADM[🔴 Admin\nTüm yetkiler]
        TEA[🟡 Teacher\nKendi sınıfı]
        PAR[🟢 Parent\nKendi çocuğu]
    end

    ADM -->|Yönetebilir| SCH[Okul & Sınıflar]
    ADM -->|Yönetebilir| USR[Kullanıcılar]
    TEA -->|Girebilir| DR[Günlük Kayıtlar]
    TEA -->|Görüntüler| STU_T[Kendi Sınıfı Öğrencileri]
    PAR -->|Görüntüler| STU_P[Kendi Çocuğu]
    PAR -->|Okur| SUM[AI Günlük Özet]

    style ADM fill:#e94560,color:#fff
    style TEA fill:#f5a623,color:#000
    style PAR fill:#4caf50,color:#fff
```

### Multi-Tenant Güvenlik Kuralları
- Her tabloda `school_id` kolonu zorunlu
- JWT token içinde `school_id` claim taşınır
- Her repository metodu `school_id` filtresi uygular
- Farklı okul verileri **kesinlikle** birbirine karışmaz

---

## 🛠️ Kurulum

### Önkoşullar
- .NET 10 SDK
- PostgreSQL 15+
- Azure AI Foundry erişimi (AI özelliği için)
- Firebase projesi (push notification için)

### Adımlar

**1. Yapılandırma**
```json
// appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=schoolinfo;Username=postgres;Password=..."
  },
  "Jwt": {
    "SecretKey": "...",
    "Issuer": "SchoolInfo",
    "Audience": "SchoolInfoApp"
  },
  "AgentFramework": {
    "Endpoint": "https://<your-foundry>.openai.azure.com/",
    "ApiKey": "...",
    "Model": "gpt-4o"
  }
}
```

**2. Migration Uygulama**
```bash
dotnet ef database update --project src/SchoolInfo.Infrastructure --startup-project src/SchoolInfo.API
```

**3. Projeyi Çalıştırma**
```bash
dotnet run --project src/SchoolInfo.API
```

**4. Test Çalıştırma**
```bash
dotnet test tests/SchoolInfo.Tests
```

---

## 📝 Lisans
Bu proje özel bir mülkiyettir. Tüm hakları saklıdır.
