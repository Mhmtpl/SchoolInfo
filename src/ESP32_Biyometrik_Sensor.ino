/**
 * SchoolInfo IoT - ESP32 Çoklu Canlı Biyometrik (Nabız) Veri Vericisi
 * 
 * Bu yazılım, tek bir ESP32 (ESP-32S) kartı ile sınıftaki birden fazla
 * Xiaomi Akıllı Saat/Bileklik cihazına sırayla bağlanır (Connection Cycling),
 * nabız verisini okur, API'ye gönderir ve bir sonraki saate geçer.
 * 
 * Sınıfta bulunmayan (saati kapalı veya menzil dışında olan) çocukların
 * sistemi kilitlemesini önlemek için tarama ve okuma zaman aşımı (Timeout) eklenmiştir.
 */

#include <WiFi.h>
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
const char* serverUrl = "http://85.235.74.24:5100/api/iot/biometrics"; 
const char* iotDeviceToken = "DefaultSecretIoTToken1234!";

// 3. İzlenecek Saatlerin MAC Adresleri Dizisi
// Not: MAC adresindeki harfler BÜYÜK olmalıdır. İleride yeni saatler eklendikçe diziye ekleme yapabilirsiniz.
const String targetMacAddresses[] = {
    "54:2F:04:55:4F:9D"  // 1. Redmi Watch 4
};
const int deviceCount = sizeof(targetMacAddresses) / sizeof(targetMacAddresses[0]);

// 4. Zaman Aşımı Değerleri (Milisaniye)
const unsigned long scanTimeoutMs = 4000;    // Saati arama süresi (Bulunamazsa sonraki saate geçer)
const unsigned long readTimeoutMs = 5000;    // Bağlandıktan sonra veri bekleme süresi (Gecikirse bağlantıyı keser)
const unsigned long delayBetweenDevices = 1000; // İki saat geçişi arasındaki dinlenme süresi

// ======================================================

// BLE UUID Tanımları (Bluetooth Standart Heart Rate)
static BLEUUID serviceUUID("180D");        // Heart Rate Service
static BLEUUID charUUID("2A37");           // Heart Rate Measurement Characteristic

static boolean doConnect = false;
static boolean connected = false;
static BLEAdvertisedDevice* myDevice = nullptr;
static BLEClient* pClient = nullptr;

int currentDeviceIndex = 0;
volatile int latestHeartRate = 0;
volatile bool newHeartRateAvailable = false;
unsigned long connectionStartTime = 0;

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
    
    String targetMac = targetMacAddresses[currentDeviceIndex];
    targetMac.toUpperCase();

    if (foundAddress == targetMac) {
      Serial.print("   -> Hedef cihaz bulundu: ");
      Serial.println(foundAddress);
      BLEDevice::getScan()->stop();
      myDevice = new BLEAdvertisedDevice(advertisedDevice);
      doConnect = true;
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

  HTTPClient http;
  http.begin(serverUrl);
  http.addHeader("Content-Type", "application/json");
  http.addHeader("X-IoT-Device-Token", iotDeviceToken);

  // Genişletilebilir veri paketi
  String jsonPayload = "{\"macAddress\":\"" + mac + 
                       "\",\"heartRate\":" + String(hr) + 
                       ",\"spO2\":null,\"bodyTemperature\":null}";

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
  String activeMac = targetMacAddresses[currentDeviceIndex];
  Serial.print("\n[+] Cihaz taranıyor (" + String(currentDeviceIndex + 1) + "/" + String(deviceCount) + "): ");
  Serial.println(activeMac);

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
          sendBiometricData(activeMac, hr);
          successRead = true;
          break; // Okuma başarılı, döngüden çık
        }
        delay(50);
      }

      if (!successRead) {
        Serial.println("   [!] Zaman aşımı! Cihaz bağlandı ama nabız verisi göndermedi.");
      }

      // Bağlantıyı güvenli bir şekilde kes ve nesneyi temizle
      if (pClient != nullptr) {
        pClient->disconnect();
        delete pClient;
        pClient = nullptr;
      }
    }
  }

  // 3. Sonraki cihaza geç
  currentDeviceIndex = (currentDeviceIndex + 1) % deviceCount;
  
  // Geçişler arasında ESP32'yi dinlendir
  delay(delayBetweenDevices);
}
