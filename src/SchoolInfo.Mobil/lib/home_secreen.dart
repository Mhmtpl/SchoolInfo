import 'package:flutter/material.dart';

class HomeScreen extends StatefulWidget {
  const HomeScreen({super.key});

  @override
  State<HomeScreen> createState() => _HomeScreenState();
}

class _HomeScreenState extends State<HomeScreen> {
  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: SingleChildScrollView(
        child: Column(
          children: [
            // 1. ÜST GRADIENT HEADER (Profil ve Karşılama)
            _buildHeader(context),

            Padding(
              padding: const EdgeInsets.symmetric(horizontal: 20.0),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  const SizedBox(height: 25),
                  
                  // 2. BUGÜNÜN ÖZETİ (Yemek, Uyku, Etkinlik Durumu)
                  _buildDailySummaryTitle(),
                  const SizedBox(height: 15),
                  _buildStatusCards(),

                  const SizedBox(height: 30),

                  // 3. ANA MENÜ (Grid Yapısı)
                  const Text(
                    "Okul Panosu",
                    style: TextStyle(
                      fontSize: 20,
                      fontWeight: FontWeight.bold,
                      color: Color(0xFF2D3436),
                    ),
                  ),
                  const SizedBox(height: 15),
                  _buildMenuGrid(context),

                  const SizedBox(height: 30),

                  // 4. SON DUYURU BANNERI
                  _buildAnnouncementBanner(),
                  const SizedBox(height: 30),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }

  // Üst Karşılama Alanı Widget'ı
  Widget _buildHeader(BuildContext context) {
    return Container(
      width: double.infinity,
      decoration: const BoxDecoration(
        gradient: LinearGradient(
          colors: [Color(0xFF6C5CE7), Color(0xFFa29bfe)],
          begin: Alignment.topLeft,
          end: Alignment.bottomRight,
        ),
        borderRadius: BorderRadius.only(
          bottomLeft: Radius.circular(36),
          bottomRight: Radius.circular(36),
        ),
      ),
      padding: const EdgeInsets.only(top: 60, left: 24, right: 24, bottom: 35),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    "Mini Adımlar Anaokulu",
                    style: TextStyle(
                      color: const Color.fromRGBO(255, 255, 255, 0.8),
                      fontSize: 14,
                      fontWeight: FontWeight.w500,
                      letterSpacing: 1,
                    ),
                  ),
                  const SizedBox(height: 4),
                  const Text(
                    "Merhaba, Melis Hanım 👋",
                    style: TextStyle(
                      color: Colors.white,
                      fontSize: 22,
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                ],
              ),
              // Veli / Öğrenci Profil Fotoğrafı
              Container(
                decoration: BoxDecoration(
                  shape: BoxShape.circle,
                  border: Border.all(color: Colors.white, width: 2),
                ),
                child: const CircleAvatar(
                  radius: 28,
                  backgroundColor: Colors.white,
                  child: Icon(Icons.face, size: 40, color: Color(0xFF6C5CE7)),
                ),
              ),
            ],
          ),
          const SizedBox(height: 25),
          // Çocuğun aktif okulda olduğunu belirten küçük bilgi kartı
          Container(
            padding: const EdgeInsets.all(15),
            decoration: BoxDecoration(
              color: const Color.fromRGBO(255, 255, 255, 0.15),
              borderRadius: BorderRadius.circular(20),
            ),
            child: Row(
              children: [
                Container(
                  padding: const EdgeInsets.all(8),
                  decoration: const BoxDecoration(
                    color: Colors.white,
                    shape: BoxShape.circle,
                  ),
                  child: const Icon(
                    Icons.school,
                    color: Color(0xFF6C5CE7),
                    size: 20,
                  ),
                ),
                const SizedBox(width: 12),
                const Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        "Mert şu an okulda 🎒",
                        style: TextStyle(
                          color: Colors.white,
                          fontWeight: FontWeight.bold,
                          fontSize: 15,
                        ),
                      ),
                      Text(
                        "Giriş Saati: 08:45 | Sınıf: Neşeli Bulutlar",
                        style: TextStyle(
                          color: Colors.white70,
                          fontSize: 12,
                        ),
                      ),
                    ],
                  ),
                ),
              ],
            ),
          )
        ],
      ),
    );
  }

  Widget _buildDailySummaryTitle() {
    return const Row(
      mainAxisAlignment: MainAxisAlignment.spaceBetween,
      children: [
        Text(
          "Mert'in Bugünü",
          style: TextStyle(
            fontSize: 20,
            fontWeight: FontWeight.bold,
            color: Color(0xFF2D3436),
          ),
        ),
        Text(
          "Güncelleme: 14:10",
          style: TextStyle(
            fontSize: 12,
            color: Colors.grey,
          ),
        ),
      ],
    );
  }

  // Yemek, Uyku ve Aktivite Kartları
  Widget _buildStatusCards() {
    return Row(
      mainAxisAlignment: MainAxisAlignment.spaceBetween,
      children: [
        _buildStatusItem(
          icon: Icons.restaurant,
          title: "Yemek",
          value: "Hepsi Bitti",
          color: const Color(0xFFFF7675),
          bgColor: const Color(0xFFFFEAEA),
        ),
        _buildStatusItem(
          icon: Icons.hotel,
          title: "Uyku",
          value: "1.5 Saat",
          color: const Color(0xFF00CEC9),
          bgColor: const Color(0xFFE0FAF9),
        ),
        _buildStatusItem(
          icon: Icons.palette,
          title: "Etkinlik",
          value: "3 Katılım",
          color: const Color(0xFFFDCB6E),
          bgColor: const Color(0xFFFFF9E6),
        ),
      ],
    );
  }

  Widget _buildStatusItem({
    required IconData icon,
    required String title,
    required String value,
    required Color color,
    required Color bgColor,
  }) {
    return Container(
      width: 105,
      padding: const EdgeInsets.symmetric(vertical: 16, horizontal: 8),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(20),
        boxShadow: const [
          BoxShadow(
            color: Color.fromRGBO(0, 0, 0, 0.03),
            blurRadius: 10,
            offset: Offset(0, 5),
          ),
        ],
      ),
      child: Column(
        children: [
          Container(
            padding: const EdgeInsets.all(10),
            decoration: BoxDecoration(
              color: bgColor,
              shape: BoxShape.circle,
            ),
            child: Icon(icon, color: color, size: 24),
          ),
          const SizedBox(height: 10),
          Text(
            title,
            style: const TextStyle(
              fontSize: 13,
              fontWeight: FontWeight.w500,
              color: Colors.grey,
            ),
          ),
          const SizedBox(height: 4),
          Text(
            value,
            textAlign: TextAlign.center,
            style: TextStyle(
              fontSize: 14,
              fontWeight: FontWeight.bold,
              color: color,
            ),
          ),
        ],
      ),
    );
  }

  // 4'lü Grid Menü Yapısı
  Widget _buildMenuGrid(BuildContext context) {
    return GridView.count(
      shrinkWrap: true,
      physics: const NeverScrollableScrollPhysics(),
      crossAxisCount: 2,
      childAspectRatio: 1.4,
      crossAxisSpacing: 15,
      mainAxisSpacing: 15,
      children: [
        _buildMenuCard(
          icon: Icons.photo_library,
          title: "Günlük Galeri",
          subtitle: "Bugünden kareler",
          color: const Color(0xFF6C5CE7),
          onTap: () => _showComingSoon(context, "Günlük Galeri"),
        ),
        _buildMenuCard(
          icon: Icons.assignment,
          title: "Yemek Listesi",
          subtitle: "Aylık menü planı",
          color: const Color(0xFF0984E3),
          onTap: () => _showComingSoon(context, "Yemek Listesi"),
        ),
        _buildMenuCard(
          icon: Icons.medical_services,
          title: "İlaç Takibi",
          subtitle: "Öğretmene bildir",
          color: const Color(0xFFD63031),
          onTap: () => _showComingSoon(context, "İlaç Takibi"),
        ),
        _buildMenuCard(
          icon: Icons.forum,
          title: "Öğretmen Mesajı",
          subtitle: "Sınıf ile sohbet",
          color: const Color(0xFF00B894),
          onTap: () => _showComingSoon(context, "Öğretmen Mesajı"),
        ),
      ],
    );
  }

  Widget _buildMenuCard({
    required IconData icon,
    required String title,
    required String subtitle,
    required Color color,
    required VoidCallback onTap,
  }) {
    return InkWell(
      onTap: onTap,
      borderRadius: BorderRadius.circular(24),
      child: Container(
        padding: const EdgeInsets.all(16),
        decoration: BoxDecoration(
          color: Colors.white,
          borderRadius: BorderRadius.circular(24),
          boxShadow: const [
            BoxShadow(
              color: Color.fromRGBO(0, 0, 0, 0.03),
              blurRadius: 10,
              offset: Offset(0, 5),
            ),
          ],
        ),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          mainAxisAlignment: MainAxisAlignment.spaceBetween,
          children: [
            Container(
              padding: const EdgeInsets.all(8),
              decoration: BoxDecoration(
                color: color.withAlpha((0.1 * 255).round()),
                borderRadius: BorderRadius.circular(12),
              ),
              child: Icon(icon, color: color, size: 28),
            ),
            Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  title,
                  style: const TextStyle(
                    fontSize: 15,
                    fontWeight: FontWeight.bold,
                    color: Color(0xFF2D3436),
                  ),
                ),
                Text(
                  subtitle,
                  style: const TextStyle(
                    fontSize: 11,
                    color: Colors.grey,
                  ),
                ),
              ],
            )
          ],
        ),
      ),
    );
  }

  // Alt Duyuru Kartı
  Widget _buildAnnouncementBanner() {
    return Container(
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        color: const Color(0xFFFFF9E6), // Yumuşak sarı arka plan
        borderRadius: BorderRadius.circular(24),
        border: Border.all(color: const Color(0xFFFDE8A5)),
      ),
      child: Row(
        children: [
          Container(
            padding: const EdgeInsets.all(12),
            decoration: const BoxDecoration(
              color: Color(0xFFFDCB6E),
              shape: BoxShape.circle,
            ),
            child: const Icon(
              Icons.campaign,
              color: Colors.white,
              size: 28,
            ),
          ),
          const SizedBox(width: 16),
          const Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  "Önemli Duyuru",
                  style: TextStyle(
                    fontWeight: FontWeight.bold,
                    fontSize: 15,
                    color: Color(0xFFD63031),
                  ),
                ),
                SizedBox(height: 2),
                Text(
                  "Yarın gerçekleştirilecek Pijama Partisi için çocukların pijama takımlarını çantaya koymayı unutmayınız. 🐼",
                  style: TextStyle(
                    fontSize: 13,
                    color: Color(0xFF636E72),
                    height: 1.3,
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }

  // Tıklama aksiyonu için minik bir bilgi mesajı fonksiyonu
  void _showComingSoon(BuildContext context, String moduleName) {
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(
        content: Text('$moduleName modülü yakında eklenecektir! 🚀'),
        behavior: SnackBarBehavior.floating,
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
        backgroundColor: const Color(0xFF6C5CE7),
      ),
    );
  }
}