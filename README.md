# SchoolInfo - Okul Öncesi Bilgi Sistemi

SchoolInfo, anaokulları ve kreşler için tasarlanmış, öğretmen-veli iletişimini güçlendiren ve yapay zeka destekli günlük özetler sunan modern bir okul yönetim sistemidir.

## 🚀 Teknolojiler
- **Backend:** .NET 10 (Minimal API)
- **Mimari:** Clean Architecture + Domain Driven Design (DDD)
- **Veritabanı:** PostgreSQL + EF Core 10
- **AI:** Microsoft Agent Framework 1.0
- **Patternler:** CQRS (MediatR), Repository Pattern
- **Güvenlik:** JWT + Role Based Authorization

## 🏗️ Mimari Yapı
Proje Clean Architecture prensiplerine uygun olarak 4 ana katmandan oluşur:

1.  **SchoolInfo.Core (Domain):** Entity'ler, Value Object'ler ve domain mantığı.
2.  **SchoolInfo.Application:** İş mantığı, CQRS (Commands/Queries), DTO'lar.
3.  **SchoolInfo.Infrastructure:** Veritabanı erişimi, AI entegrasyonu, kimlik doğrulama.
4.  **SchoolInfo.API:** Minimal API endpoint'leri ve middleware yapısı.

---

## 📊 İş Akışları (Visual Workflows)

### 1. Sistem Kurulumu ve Yönetim
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

### 2. Günlük Operasyon ve AI Özetleme
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

### 3. Veli Bilgilendirme
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

## 🛠️ Kurulum

1.  **Veritabanı Yapılandırması:** `appsettings.json` içindeki PostgreSQL bağlantı dizesini güncelleyin.
2.  **Migration Uygulama:**
    ```bash
    dotnet ef database update --project src/SchoolInfo.Infrastructure --startup-project src/SchoolInfo.API
    ```
3.  **Projeyi Çalıştırma:**
    ```bash
    dotnet run --project src/SchoolInfo.API
    ```

## 🔒 Güvenlik Kuralları
- **Multi-Tenant:** Her okulun verisi `school_id` ile izole edilmiştir.
- **Roller:** `Admin`, `Teacher`, `Parent`.
- **Erişim:** Her öğretmen sadece kendi sınıflarına, her veli sadece kendi çocuğuna erişebilir.

---

## 📝 Lisans
Bu proje özel bir mülkiyettir. Tüm hakları saklıdır.
