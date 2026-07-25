/**
 * SchoolInfo IoT - ESP32 Çoklu Canlı Biyometrik (Nabız) Veri Vericisi
 * 
 * Bu yazılım, tek bir ESP32 (ESP-32S) kartı ile sınıftaki birden fazla
 * HUAWEI Band 10 Akıllı Saat/Bileklik cihazına sırayla bağlanır (Connection Cycling),
 * nabız verisini okur, API'ye gönderir ve bir sonraki saate geçer.
 * 
 * Sınıfta bulunmayan (saati kapalı veya menzil dışında olan) çocukların
 * sistemi kilitlemesini önlemek için tarama ve okuma zaman aşımı (Timeout) eklenmiştir.
 */

#include <WiFi.h>
#include <WiFiClientSecure.h>
#include <WiFiMulti.h>
#include <HTTPClient.h>
#include <BLEDevice.h>
#include <BLEUtils.h>
#include <BLEScan.h>
#include <BLEAdvertisedDevice.h>

WiFiMulti wifiMulti;

// ==================== YAPILANDIRMA ====================

// 1. Wi-Fi Bilgileri (Birden fazla ağ tanımlayabilirsiniz, en güçlü olana otomatik bağlanır)
struct WiFiNetwork {
    const char* ssid;
    const char* password;
};

const WiFiNetwork wifiNetworks[] = {
    {"Keenetic-8550", "g2e44GrKbDT5vrSGLCoY++"},
    {"TurkNet1000Mbps_16EDA", "RbZEUP7s"},
    {"TURKNET_ADBAB", "SfzufsC3"},
    {"mPala", "12345678p"} // Mobil Hotspot
};
const int wifiCount = sizeof(wifiNetworks) / sizeof(wifiNetworks[0]);

// 2. SchoolInfo API Sunucu Adresi
// Not: Canlı sunucu adresini veya yerel bilgisayar IP'sini girin
const char* serverUrl = "http://api.veliport.com.tr/api/iot/biometrics"; 
const char* iotDeviceToken = "DefaultSecretIoTToken1234!";

// 3. İzlenecek Saatlerin/Bilekliklerin MAC Adresleri veya Benzersiz İsimleri Dizisi
// Not: Cihazların MAC adreslerini veya telefonda görünen bluetooth isimlerini (örn: HUAWEI Band 10-73D) yazabilirsiniz.
const String targetDevices[] = {
    "E4:A5:87:B7:07:3D" // 1. HUAWEI Band 10-73D (Gerçek BLE MAC Adresi)
};
const int deviceCount = sizeof(targetDevices) / sizeof(targetDevices[0]);

// 4. Zaman Aşımı Değerleri (Milisaniye)
const unsigned long scanTimeoutMs = 4000;    // Saati arama süresi (Bulunamazsa sonraki saate geçer)
const unsigned long readTimeoutMs = 5000;    // Bağlandıktan sonra veri bekleme süresi (Gecikirse bağlantıyı keser)
const unsigned long delayBetweenDevices = 1000; // İki saat geçişi arasındaki dinlenme süresi

// ======================================================

// BLE UUID Tanımları (Bluetooth Standart Heart Rate)
static BLEUUID serviceUUID("180D");        // Heart Rate Service
static BLEUUID charUUID("2A37");           // Heart Rate Measurement Characteristic

volatile bool doConnect = false;
volatile bool connected = false;
static BLEAdvertisedDevice* myDevice = nullptr;
static BLEClient* pClient = nullptr;

int currentDeviceIndex = 0;
volatile int latestHeartRate = 0;
volatile bool newHeartRateAvailable = false;
unsigned long connectionStartTime = 0;
int consecutiveFailures = 0;

// BLE Bağlantı Durumu Takibi
class MyClientCallback : public BLEClientCallbacks {
  void onConnect(BLEClient* pclient) {
    Serial.println("   -> Cihaza bağlanıldı, servisler keşfediliyor...");
  }

  void onDisconnect(BLEClient* pclient) {
    connected = false;
    Serial.println("   -> Cihaz bağlantısı sonlandırıldı.");
  }
};

// BLE Bildirim (Notify) Callback'i
static void notifyCallback(
  BLERemoteCharacteristic* pBLERemoteCharacteristic,
  uint8_t* pData,
  size_t length,
  bool isNotify) {
    
    if (length < 2) return;

    uint8_t flags = pData[0];
    int heartRateValue = 0;

    // Standard BLE Heart Rate parsing
    if (flags & 0x01) {
        heartRateValue = pData[1] | (pData[2] << 8);
    } else {
        heartRateValue = pData[1];
    }

    if (heartRateValue > 30 && heartRateValue < 220) {
        latestHeartRate = heartRateValue;
        newHeartRateAvailable = true;
    }
}

// Cihaza Bağlanıp Abone Olma İşlemi
bool connectToDevice() {
    if (myDevice == nullptr) return false;

    Serial.print("   -> Bağlanılıyor: ");
    Serial.println(myDevice->getAddress().toString().c_str());
    
    pClient = BLEDevice::createClient();
    pClient->setClientCallbacks(new MyClientCallback());

    if (!pClient->connect(myDevice)) {
        Serial.println("   [!] Bağlantı denemesi başarısız oldu.");
        delete pClient;
        pClient = nullptr;
        return false;
    }

    // MTU ve bağlantı parametrelerinin güvenli müzakeresi için kısa bir gecikme ekle
    delay(300);

    // Servis Keşfi
    BLERemoteService* pRemoteService = pClient->getService(serviceUUID);
    if (pRemoteService == nullptr) {
      Serial.println("   [!] Heart Rate Servisi (0x180D) bulunamadı.");
      pClient->disconnect();
      delete pClient;
      pClient = nullptr;
      return false;
    }

    // Karakteristik Keşfi
    BLERemoteCharacteristic* pRemoteCharacteristic = pRemoteService->getCharacteristic(charUUID);
    if (pRemoteCharacteristic == nullptr) {
      Serial.println("   [!] Heart Rate Karakteristiği (0x2A37) bulunamadı.");
      pClient->disconnect();
      delete pClient;
      pClient = nullptr;
      return false;
    }

    // Abone ol (Notify)
    if(pRemoteCharacteristic->canNotify()) {
      pRemoteCharacteristic->registerForNotify(notifyCallback);
      Serial.println("   -> Veri bildirimlerine abone olundu. Ölçüm bekleniyor...");
    } else {
      Serial.println("   [!] Karakteristik bildirimleri desteklemiyor.");
      pClient->disconnect();
      delete pClient;
      pClient = nullptr;
      return false;
    }

    connected = true;
    connectionStartTime = millis();
    return true;
}

// BLE Reklam Tarama Callback'i
class MyAdvertisedDeviceCallbacks: public BLEAdvertisedDeviceCallbacks {
  void onResult(BLEAdvertisedDevice advertisedDevice) {
    String foundAddress = advertisedDevice.getAddress().toString().c_str();
    foundAddress.toUpperCase();
    
    String foundName = "";
    if (advertisedDevice.haveName()) {
      foundName = advertisedDevice.getName().c_str();
    }

    // [TANI/DIAGNOSTIC] Havada yakalanan Huawei/Band cihazlarını ekrana yazdırır
    if (foundName.indexOf("Band") != -1 || foundName.indexOf("HR") != -1 || foundAddress.startsWith("24:A4")) {
      Serial.print("   [Tarama] Cihaz Yakalandi -> MAC: ");
      Serial.print(foundAddress);
      Serial.print(" | Isim: ");
      Serial.println(foundName.length() > 0 ? foundName : "Isimsiz");
    }

    String target = targetDevices[currentDeviceIndex];
    String targetUpper = target;
    targetUpper.toUpperCase();

    // Hem MAC adresi hem de Cihaz İsmi ile eşleşme kontrolü (büyük/küçük harf duyarsız)
    bool isMatch = (foundAddress == targetUpper);
    
    if (!isMatch && foundName.length() > 0) {
      String foundNameUpper = foundName;
      foundNameUpper.toUpperCase();
      
      if (foundNameUpper == targetUpper) {
        isMatch = true;
      } else {
        // Hedef ismin sonundaki benzersiz kodu (örn: "73D" veya "8C4") alalım ve yayınlanan ismin içinde arayalım
        int dashIndex = target.lastIndexOf('-');
        String suffix = (dashIndex != -1) ? target.substring(dashIndex + 1) : target;
        suffix.toUpperCase();
        
        if (suffix.length() > 0 && foundNameUpper.indexOf(suffix) != -1) {
          isMatch = true;
        }
      }
    }

    if (isMatch) {
      Serial.print("   -> Hedef cihaz bulundu: ");
      Serial.print(target);
      Serial.print(" (Yayinlanan isim: ");
      Serial.print(foundName.length() > 0 ? foundName : "Isimsiz");
      Serial.println(")");
      myDevice = new BLEAdvertisedDevice(advertisedDevice);
      doConnect = true;
      BLEDevice::getScan()->stop(); // Wakes up main thread - must be called last!
    }
  }
};

// Wi-Fi Bağlantısı
void connectToWiFi() {
  if (WiFi.status() == WL_CONNECTED) return;
  
  Serial.print("Wi-Fi bağlantısı kuruluyor...");
  
  int attempts = 0;
  while (wifiMulti.run() != WL_CONNECTED && attempts < 10) {
    delay(1000);
    Serial.print(".");
    attempts++;
  }
  
  if (WiFi.status() == WL_CONNECTED) {
    Serial.println("\n-> Wi-Fi bağlantısı başarılı! IP: " + WiFi.localIP().toString());
  } else {
    Serial.println("\n[!] Wi-Fi bağlantısı başarısız! Sonraki döngüde denenecek.");
  }
}

// API'ye POST İsteği
void sendBiometricData(String mac, int hr) {
  if (WiFi.status() != WL_CONNECTED) {
    connectToWiFi();
    if (WiFi.status() != WL_CONNECTED) return;
  }

  Serial.print("   [Heap] Free: ");
  Serial.print(ESP.getFreeHeap());
  Serial.println(" bytes");

  HTTPClient http;
  String urlStr = String(serverUrl);
  String jsonPayload = "{\"macAddress\":\"" + mac + 
                       "\",\"heartRate\":" + String(hr) + 
                       ",\"spO2\":null,\"bodyTemperature\":null}";

  if (urlStr.startsWith("https")) {
    WiFiClientSecure client;
    client.setInsecure();
    http.begin(client, serverUrl);
    
    http.addHeader("Content-Type", "application/json");
    http.addHeader("X-IoT-Device-Token", iotDeviceToken);
    
    Serial.print("   API Gönderimi: ");
    Serial.println(jsonPayload);
    
    int httpResponseCode = http.POST(jsonPayload);
    if (httpResponseCode > 0) {
      Serial.print("   API Yanıtı: ");
      Serial.println(httpResponseCode);
    } else {
      Serial.print("   [!] API Gönderim Hatası! Kodu: ");
      Serial.println(httpResponseCode);
    }
    http.end();
  } else {
    WiFiClient client;
    http.begin(client, serverUrl);
    
    http.addHeader("Content-Type", "application/json");
    http.addHeader("X-IoT-Device-Token", iotDeviceToken);
    
    Serial.print("   API Gönderimi: ");
    Serial.println(jsonPayload);
    
    int httpResponseCode = http.POST(jsonPayload);
    if (httpResponseCode > 0) {
      Serial.print("   API Yanıtı: ");
      Serial.println(httpResponseCode);
    } else {
      Serial.print("   [!] API Gönderim Hatası! Kodu: ");
      Serial.println(httpResponseCode);
    }
    http.end();
  }
}

void setup() {
  Serial.begin(115200);
  Serial.println("\n=== SchoolInfo Çoklu Biyometrik Takip Başlatıldı ===");

  // Wi-Fi Ağlarını Ekle
  for (int i = 0; i < wifiCount; i++) {
    wifiMulti.addAP(wifiNetworks[i].ssid, wifiNetworks[i].password);
  }

  connectToWiFi();

  // BLE Başlat
  BLEDevice::init("SchoolInfo-Gateway");
  
  BLEScan* pBLEScan = BLEDevice::getScan();
  pBLEScan->setAdvertisedDeviceCallbacks(new MyAdvertisedDeviceCallbacks());
  pBLEScan->setInterval(1349);
  pBLEScan->setWindow(449);
  pBLEScan->setActiveScan(true);
}

void loop() {
  String activeDevice = targetDevices[currentDeviceIndex];
  Serial.print("\n[+] Cihaz taranıyor (" + String(currentDeviceIndex + 1) + "/" + String(deviceCount) + "): ");
  Serial.println(activeDevice);

  doConnect = false;
  connected = false;
  newHeartRateAvailable = false;
  if (myDevice != nullptr) {
    delete myDevice;
    myDevice = nullptr;
  }

  // 1. Tarama Başlat (Belirli bir süre boyunca)
  BLEScan* pBLEScan = BLEDevice::getScan();
  pBLEScan->start(scanTimeoutMs / 1000, false);
  
  // Tarama bittiğinde cihaz bulunamadıysa bir sonrakine geç
  if (!doConnect) {
    Serial.println("   [!] Cihaz bulunamadı (kapalı veya menzil dışında).");
  } 
  // 2. Cihaz bulunduysa bağlan ve oku
  else {
    if (connectToDevice()) {
      unsigned long startWait = millis();
      bool successRead = false;

      // Veri gelene kadar veya zaman aşımına uğrayana kadar bekle
      while (connected && (millis() - startWait < readTimeoutMs)) {
        if (newHeartRateAvailable) {
          int hr = latestHeartRate;
          Serial.print("   -> Nabız Değeri Okundu: ");
          Serial.print(hr);
          Serial.println(" BPM");

          // API'ye gönder
          sendBiometricData(activeDevice, hr);
          successRead = true;
          consecutiveFailures = 0; // Başarılı veri alındı, sayacı sıfırla
          break; // Okuma başarılı, döngüden çık
        }
        delay(50);
      }

      if (!successRead) {
        Serial.println("   [!] Zaman aşımı! Cihaz bağlandı ama nabız verisi göndermedi.");
        consecutiveFailures++;
      }

      // Bağlantıyı güvenli bir şekilde kes ve nesneyi temizle
      if (pClient != nullptr) {
        pClient->disconnect();
        delete pClient;
        pClient = nullptr;
      }
    } else {
      // Bağlantı kurulamadı veya karakteristik keşfi başarısız oldu
      consecutiveFailures++;
    }
  }

  // 3. Bluetooth Kilitlenme Koruması (Self-Healing)
  if (consecutiveFailures >= 10) {
    Serial.println("\n[!] Arda arda 10 baglantı/veri hatası! BLE Stack kilitlenmiş olabilir.");
    Serial.println("[+] ESP32 kendi kendini temizlemek için RESTART ediliyor...");
    delay(1000);
    ESP.restart();
  }

  // 4. Sonraki cihaza geç
  currentDeviceIndex = (currentDeviceIndex + 1) % deviceCount;
  
  // Geçişler arasında ESP32'yi dinlendir
  delay(delayBetweenDevices);
}
