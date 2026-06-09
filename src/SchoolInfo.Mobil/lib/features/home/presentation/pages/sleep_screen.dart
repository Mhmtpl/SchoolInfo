import 'package:flutter/material.dart';

class SleepScreen extends StatelessWidget {
  const SleepScreen({super.key});

  @override
  Widget build(BuildContext context) {
    // sample data
    const start = '22:30';
    const end = '07:00';
    const duration = '8.5h';
    const quality = 0.84; // 84%

    return Scaffold(
      backgroundColor: const Color(0xFFF4F6FB),
      appBar: AppBar(
        backgroundColor: const Color(0xFF6C5CE7),
        title: const Text('Uyku Kayıtları'),
        elevation: 0,
      ),
      body: Padding(
        padding: const EdgeInsets.all(20),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            const SizedBox(height: 8),
            const Text('Uyku Düzeni', style: TextStyle(fontSize: 22, fontWeight: FontWeight.bold)),
            const SizedBox(height: 6),
            const Text('Uyku süresini, kalite skorunu ve uyku zaman çizelgesini görüntüleyin.', style: TextStyle(color: Colors.grey)),
            const SizedBox(height: 18),
            Container(
              width: double.infinity,
              padding: const EdgeInsets.all(18),
              decoration: BoxDecoration(
                color: Colors.white,
                borderRadius: BorderRadius.circular(16),
                boxShadow: const [BoxShadow(color: Color.fromRGBO(0,0,0,0.04), blurRadius: 16, offset: Offset(0,8))],
              ),
              child: Row(
                children: [
                  SizedBox(
                    width: 120,
                    height: 120,
                    child: Stack(
                      alignment: Alignment.center,
                      children: [
                        SizedBox(
                          width: 100,
                          height: 100,
                          child: CircularProgressIndicator(
                            value: quality,
                            strokeWidth: 10,
                            backgroundColor: Colors.grey.shade200,
                            color: const Color(0xFF6C5CE7),
                          ),
                        ),
                        Column(
                          mainAxisSize: MainAxisSize.min,
                          children: [
                            Text('${(quality * 100).round()}%', style: const TextStyle(fontSize: 18, fontWeight: FontWeight.bold)),
                            const SizedBox(height: 6),
                            const Text('Kalite', style: TextStyle(color: Colors.grey)),
                          ],
                        ),
                      ],
                    ),
                  ),
                  const SizedBox(width: 18),
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text('Toplam Süre: $duration', style: const TextStyle(fontSize: 16, fontWeight: FontWeight.bold)),
                        const SizedBox(height: 6),
                        Text('Başlangıç: $start  •  Bitiş: $end', style: const TextStyle(color: Colors.grey)),
                        const SizedBox(height: 12),
                        Wrap(
                          spacing: 8,
                          children: const [
                            Chip(label: Text('Derin Uyku')),
                            Chip(label: Text('Hafif Uyku')),
                            Chip(label: Text('Kesintisiz')),
                          ],
                        ),
                        const SizedBox(height: 12),
                        const Text('Özet', style: TextStyle(fontWeight: FontWeight.bold)),
                        const SizedBox(height: 6),
                        const Text('Uyku düzeni genel olarak iyi. Geç uyku başlangıcını azaltmaya çalışın.', style: TextStyle(color: Colors.grey)),
                      ],
                    ),
                  )
                ],
              ),
            ),
            const SizedBox(height: 18),
            const Text('Uyku Zaman Çizelgesi', style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold)),
            const SizedBox(height: 10),
            Column(
              children: const [
                _TimelineItem(time: '22:30', label: 'Uykuya başlama'),
                _TimelineItem(time: '23:20', label: 'Derin uyku'),
                _TimelineItem(time: '03:10', label: 'Kısa uyanma'),
                _TimelineItem(time: '07:00', label: 'Uyanma ve kahvaltı'),
              ],
            ),
          ],
        ),
      ),
    );
  }
}

class _TimelineItem extends StatelessWidget {
  final String time;
  final String label;
  const _TimelineItem({required this.time, required this.label});

  @override
  Widget build(BuildContext context) {
    return Container(
      margin: const EdgeInsets.only(bottom: 10),
      child: Row(
        children: [
          SizedBox(width: 72, child: Text(time, style: const TextStyle(fontWeight: FontWeight.bold))),
          Expanded(
            child: Container(
              padding: const EdgeInsets.symmetric(vertical: 12, horizontal: 14),
              decoration: BoxDecoration(
                color: Colors.white,
                borderRadius: BorderRadius.circular(12),
                boxShadow: const [BoxShadow(color: Color.fromRGBO(0,0,0,0.03), blurRadius: 10, offset: Offset(0,6))],
              ),
              child: Text(label),
            ),
          ),
        ],
      ),
    );
  }
}
