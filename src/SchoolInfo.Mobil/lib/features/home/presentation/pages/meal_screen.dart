import 'package:flutter/material.dart';

enum EatenAmount { full, partial, none }

class MealRecord {
  final String title;
  final String details;
  final EatenAmount amount;

  const MealRecord({
    required this.title,
    required this.details,
    required this.amount,
  });
}

class MealScreen extends StatelessWidget {
  const MealScreen({super.key});

  static const List<MealRecord> sample = [
    MealRecord(title: 'Sabah Kahvaltısı', details: 'Makarna, meyve suyu', amount: EatenAmount.full),
    MealRecord(title: 'Öğle Yemeği', details: 'Mercimek çorbası, köfte, pilav', amount: EatenAmount.partial),
    MealRecord(title: 'Ara Öğün', details: 'Meyve ve süt', amount: EatenAmount.none),
  ];

  IconData _iconFor(EatenAmount a) {
    switch (a) {
      case EatenAmount.full:
        return Icons.sentiment_very_satisfied;
      case EatenAmount.partial:
        return Icons.sentiment_neutral;
      case EatenAmount.none:
        return Icons.sentiment_dissatisfied;
    }
  }

  Color _colorFor(EatenAmount a) {
    switch (a) {
      case EatenAmount.full:
        return const Color(0xFF2ECC71);
      case EatenAmount.partial:
        return const Color(0xFFFFC107);
      case EatenAmount.none:
        return const Color(0xFFEF5350);
    }
  }

  String _labelFor(EatenAmount a) {
    switch (a) {
      case EatenAmount.full:
        return 'İyi yemiş';
      case EatenAmount.partial:
        return 'Az yemiş';
      case EatenAmount.none:
        return 'Hiç yememiş';
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: const Color(0xFFF4F6FB),
      appBar: AppBar(
        backgroundColor: const Color(0xFF6C5CE7),
        title: const Text('Yemek Kayıtları'),
        elevation: 0,
      ),
      body: ListView.builder(
        padding: const EdgeInsets.all(20),
        itemCount: sample.length + 1,
        itemBuilder: (context, index) {
          if (index == 0) {
            return Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: const [
                SizedBox(height: 8),
                Text('Bugünkü Menü', style: TextStyle(fontSize: 22, fontWeight: FontWeight.bold)),
                SizedBox(height: 6),
                Text('Yeme miktarına göre hızlı durum göstergesi', style: TextStyle(color: Colors.grey)),
                SizedBox(height: 16),
              ],
            );
          }

          final item = sample[index - 1];
          final icon = _iconFor(item.amount);
          final color = _colorFor(item.amount);
          final label = _labelFor(item.amount);

          return Container(
            margin: const EdgeInsets.only(bottom: 14),
            padding: const EdgeInsets.all(14),
            decoration: BoxDecoration(
              color: Colors.white,
              borderRadius: BorderRadius.circular(18),
              boxShadow: const [
                BoxShadow(color: Color.fromRGBO(0, 0, 0, 0.04), blurRadius: 16, offset: Offset(0, 8)),
              ],
            ),
            child: Row(
              children: [
                Container(
                  width: 56,
                  height: 56,
                  decoration: BoxDecoration(
                    color: color.withAlpha(30),
                    shape: BoxShape.circle,
                  ),
                  child: Icon(icon, color: color, size: 30),
                ),
                const SizedBox(width: 14),
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(item.title, style: const TextStyle(fontSize: 16, fontWeight: FontWeight.bold)),
                      const SizedBox(height: 6),
                      Text(item.details, style: const TextStyle(color: Colors.grey)),
                    ],
                  ),
                ),
                Column(
                  crossAxisAlignment: CrossAxisAlignment.end,
                  children: [
                    Text(label, style: TextStyle(color: color, fontWeight: FontWeight.bold)),
                    const SizedBox(height: 8),
                    SizedBox(
                      width: 110,
                      child: LinearProgressIndicator(
                        value: item.amount == EatenAmount.full ? 1 : (item.amount == EatenAmount.partial ? 0.5 : 0.0),
                        color: color,
                        backgroundColor: Colors.grey.shade200,
                        minHeight: 8,
                      ),
                    ),
                  ],
                )
              ],
            ),
          );
        },
      ),
    );
  }
}
