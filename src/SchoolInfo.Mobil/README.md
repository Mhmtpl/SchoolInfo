# 📱 SchoolInfo.Mobil — Flutter Mobil Uygulaması Geliştirici Kılavuzu

Bu dizin, okul öncesi bilgi sisteminin (Veli portalı) Flutter ile geliştirilmiş mobil uygulamasını içerir.

---

## 🏗️ 1. Mimari Yapı (Riverpod & Clean Architecture)

Uygulama, Riverpod durum yönetim kütüphanesini ve temel Clean Architecture prensiplerini kullanır. Klasör yapısı aşağıdaki gibidir:

```text
lib/
├── core/
│   ├── services/
│   │   ├── auth_storage_service.dart      # JWT token saklama, otomatik yenileme (refresh session)
│   │   └── biometric_signalr_service.dart # WebSocket ile el sıkışmalı raw SignalR bağlantısı
│   └── tenant/
│       └── school_id.dart                  # Multi-tenant okul ID değeri
│
├── features/
│   ├── auth/                              # Kimlik doğrulama modülü (Giriş ekranı, API istekleri)
│   ├── home/                              # Veli ana paneli, çocuk listesi ve sağlık grafiği
│   │   └── presentation/
│   │       ├── pages/                     # home_screen.dart, biometrics_history_screen.dart, vb.
│   │       └── widgets/                   # biometric_dashboard_card.dart, ecg_live_wave.dart
│   └── teacher/                           # Öğretmen özellikleri (varsa)
│
└── theme/
    ├── app_theme.dart                     # Açık/Koyu tema tanımları
    └── theme_provider.dart                # Tema geçiş durum yöneticisi
```

---

## 🔌 2. SignalR Canlı Biyometrik Bağlantı Protokolü

Android ve iOS platformlarında harici SignalR kütüphanelerinin Windows dosya yolları ve sembolik bağlar (symlink) nedeniyle derleme hatası vermesini önlemek amacıyla, **`biometric_signalr_service.dart`** dosyasında saf WebSocket protokolü kullanılmıştır.

### Nasıl Çalışır?
1. WebSocket bağlantısı kurulur (`wss://api.veliport.com.tr/hubs/biometrics?access_token=...`).
2. El sıkışma (Handshake) mesajı gönderilir: `{"protocol": "json", "version": 1}` ve sonuna **ASCII 30 (`\x1e`)** sonlandırıcı karakteri eklenir.
3. Hub üzerindeki `JoinStudentGroup` metoduna öğrenci ID'si argüman olarak geçilerek gruptan canlı yayın dinlenmeye başlanır.
4. Gelen veriler `ReceiveBiometricUpdate` hedef ismi ile yakalanır.

---

## ⚠️ 3. Bilinmesi Gereken Kritik Noktalar

* **Hatalı Token Durumu**: JWT süresi dolduğunda `AuthStorageService.refreshSession()` tetiklenerek API `/api/auth/refresh` endpoint'ine istek atılır ve oturum otomatik tazelenir.
* **Kritik Nabız Banner Uyarısı (Geliştirme Aşamasında)**: Canlı SignalR bağlantısı açıkken, çocuğun yaş grubuna göre hesaplanan kritik nabız eşiklerinin aşılması durumunda veliye anlık `MaterialBanner` uyarısı çıkarılır. Bu uyarı `home_screen.dart` dosyasında `_checkCriticalHeartRate` fonksiyonu ile yönetilmektedir.
* **Geliştirici İpuçları**: API base url adresini yerel testlerde emulator için `http://10.0.2.2` olarak ayarlayabilirsiniz. Üretim ortamında `https://api.veliport.com.tr` kullanılır.
