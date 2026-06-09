import 'package:flutter/material.dart';

class AnnouncementScreen extends StatelessWidget {
  const AnnouncementScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: const Color(0xFFF4F6FB),
      appBar: AppBar(
        backgroundColor: const Color(0xFF6C5CE7),
        title: const Text('Duyurular'),
        elevation: 0,
      ),
      body: ListView(
        padding: const EdgeInsets.all(20),
        children: const [
          SizedBox(height: 8),
          Text(
            'Güncel Duyurular',
            style: TextStyle(fontSize: 22, fontWeight: FontWeight.bold),
          ),
          SizedBox(height: 8),
          Text(
            'Okulla ilgili son bilgileri, planları ve hatırlatmaları buradan görebilirsiniz.',
            style: TextStyle(color: Colors.grey),
          ),
          SizedBox(height: 18),
          _AnnouncementCard(
            title: 'Resim Atölyesi Hatırlatma',
            description: 'Çocuklar 15:00’te resim atölyesine katılacak.',
          ),
          _AnnouncementCard(
            title: 'Yarım Gün Uyarısı',
            description: 'Yarın okul 12:00’de kapanacaktır.',
          ),
          _AnnouncementCard(
            title: 'Yeni Menü',
            description: 'Pazartesi günü menümüzde karnıyarık var.',
          ),
        ],
      ),
    );
  }
}

class _AnnouncementCard extends StatelessWidget {
  final String title;
  final String description;

  const _AnnouncementCard({
    required this.title,
    required this.description,
  });

  @override
  Widget build(BuildContext context) {
    return Container(
      margin: const EdgeInsets.only(bottom: 14),
      padding: const EdgeInsets.all(18),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(20),
        boxShadow: const [
          BoxShadow(
            blurRadius: 12,
            color: Color.fromRGBO(0, 0, 0, 0.05),
            offset: Offset(0, 7),
          ),
        ],
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(
            title,
            style: const TextStyle(
              fontSize: 17,
              fontWeight: FontWeight.bold,
            ),
          ),
          const SizedBox(height: 6),
          Text(
            description,
            style: const TextStyle(color: Colors.grey),
          ),
        ],
      ),
    );
  }
}
