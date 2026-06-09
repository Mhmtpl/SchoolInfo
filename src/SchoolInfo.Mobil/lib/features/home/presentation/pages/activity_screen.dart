import 'package:flutter/material.dart';

class ActivityScreen extends StatelessWidget {
  const ActivityScreen({super.key});

  static const _rows = [
    {'time': '09:00', 'name': 'Resim Atölyesi', 'class': 'Neşeli Bulutlar', 'current': false},
    {'time': '10:30', 'name': 'Matematik Oyunu', 'class': 'Meraklı Minikler', 'current': true},
    {'time': '13:00', 'name': 'Oyun Saati', 'class': 'Neşeli Bulutlar', 'current': false},
    {'time': '15:00', 'name': 'Müzik Saati', 'class': 'Melodi Küçükleri', 'current': false},
  ];

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: const Color(0xFFF4F6FB),
      appBar: AppBar(
        backgroundColor: const Color(0xFF6C5CE7),
        title: const Text('Aktivite Kayıtları'),
        elevation: 0,
      ),
      body: Padding(
        padding: const EdgeInsets.all(20),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            const SizedBox(height: 8),
            const Text('Bugünkü etkinlik planı', style: TextStyle(fontSize: 22, fontWeight: FontWeight.bold)),
            const SizedBox(height: 6),
            const Text('Tablo görünümünde, o anki ders yeşil renkle vurgulanır.', style: TextStyle(color: Colors.grey)),
            const SizedBox(height: 16),
            Card(
              shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
              elevation: 2,
              child: SingleChildScrollView(
                scrollDirection: Axis.horizontal,
                child: DataTable(
                  columns: const [
                    DataColumn(label: Text('Saat')),
                    DataColumn(label: Text('Etkinlik')),
                    DataColumn(label: Text('Sınıf')),
                  ],
                  rows: _rows.map((r) {
                    final isCurrent = r['current'] as bool;
                    return DataRow(
                      color: isCurrent
                          ? WidgetStateProperty.all(const Color(0xFFe8f5e9))
                          : null,
                      cells: [
                        DataCell(Text(r['time'] as String)),
                        DataCell(Row(children: [
                          if (isCurrent) const Icon(Icons.check_circle, color: Color(0xFF2ECC71), size: 16) else const SizedBox.shrink(),
                          const SizedBox(width: 6),
                          Text(r['name'] as String),
                        ])),
                        DataCell(Text(r['class'] as String)),
                      ],
                    );
                  }).toList(),
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }
}
