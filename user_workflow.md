# SchoolInfo Kullanıcı İş Akışı

Bu doküman, sistemdeki temel rollerin (Admin, Öğretmen, Veli) ana iş akışlarını görselleştirir.

## 1. Sistem Kurulum Akışı (Admin)
Admin'in okulu, sınıfları ve kullanıcıları sisteme tanımlama sürecidir.

```mermaid
graph TD
    A[Admin Girişi] --> B[Okul Tanımlama]
    B --> C[Sınıf Oluşturma]
    C --> D[Öğretmen Hesabı Oluşturma]
    D --> E[Öğretmeni Sınıfa Atama]
    E --> F[Öğrenci Kaydı ve Sınıf Ataması]
    F --> G[Veli Hesabı Oluşturma]
    G --> H[Veli ve Öğrenciyi Eşleştirme]
```

## 2. Günlük Eğitim Akışı (Öğretmen)
Öğretmenin gün içinde yaptığı kayıtlar ve AI özet süreci.

```mermaid
sequenceDiagram
    participant T as Öğretmen
    participant API as SchoolInfo API
    participant DB as Veritabanı
    participant AI as Microsoft Agent (AI)

    T->>API: Sınıf Listesini Getir
    API->>DB: Sınıf/Öğrenci Sorgula
    DB-->>T: Öğrenci Listesi

    T->>API: Öğün Kaydı Gir (Kahvaltı/Öğle)
    T->>API: Günlük Kayıt Gir (Uyku/Özbakım)
    T->>API: Aktivite Oluştur (Boyama/Oyun)
    
    Note over T, AI: Gün Sonu
    
    API->>AI: Günlük Verileri Gönder
    AI-->>API: AI Destekli Gün Özeti Oluştur
    API->>DB: Özeti Kaydet
```

## 3. Bilgilendirme Akışı (Veli)
Velinin çocuğunun durumunu takip etme süreci.

```mermaid
graph LR
    P[Veli Girişi] --> C[Çocuklarım Listesi]
    C --> D[Günlük Akış Görüntüleme]
    D --> E[Yemek Durumu]
    D --> F[Uyku & Özbakım]
    D --> G[AI Günlük Özet]
    G --> H[Günü Değerlendirme]
```

## 4. Genel Sistem Yaşam Döngüsü

```mermaid
stateDiagram-v2
    [*] --> Kayıt: Admin Okul/Sınıf/Öğrenci Tanımlar
    Kayıt --> Eğitim: Öğretmen Günlük Veri Girişi Yapar
    Emiitim: Öğretmen Aktiviteleri Yönetir
    Eğitim --> Analiz: AI Gün Sonunda Verileri Özetler
    Analiz --> Bilgilendirme: Veli Özeti ve Kayıtları İnceler
    Bilgilendirme --> Eğitim: Yeni Gün Başlar
```
