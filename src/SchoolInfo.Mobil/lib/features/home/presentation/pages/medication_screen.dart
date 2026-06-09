import 'package:flutter/material.dart';

class MedicationRecord {
  final String name;
  final String dosage;
  final String time;
  bool taken;

  MedicationRecord({
    required this.name,
    required this.dosage,
    required this.time,
    this.taken = false,
  });
}

class MedicationScreen extends StatefulWidget {
  const MedicationScreen({super.key});

  @override
  State<MedicationScreen> createState() => _MedicationScreenState();
}

class _MedicationScreenState extends State<MedicationScreen> {
  final List<MedicationRecord> _records = [
    MedicationRecord(name: 'Parol Forte', dosage: '1 tablet', time: '08:00'),
    MedicationRecord(name: 'Vitamin D', dosage: '1 kapsül', time: '12:30'),
    MedicationRecord(name: 'Soğuk Algınlığı Şurubu', dosage: '5 ml', time: '18:00'),
  ];

  final _nameController = TextEditingController();
  final _dosageController = TextEditingController();
  final _timeController = TextEditingController();

  @override
  void dispose() {
    _nameController.dispose();
    _dosageController.dispose();
    _timeController.dispose();
    super.dispose();
  }

  void _addMedication() {
    showDialog<void>(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Yeni İlaç Ekle'),
        content: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            TextField(
              controller: _nameController,
              decoration: const InputDecoration(labelText: 'İlaç Adı'),
            ),
            TextField(
              controller: _dosageController,
              decoration: const InputDecoration(labelText: 'Dozaj'),
            ),
            TextField(
              controller: _timeController,
              decoration: const InputDecoration(labelText: 'Saat'),
            ),
          ],
        ),
        actions: [
          TextButton(
            onPressed: () {
              _nameController.clear();
              _dosageController.clear();
              _timeController.clear();
              Navigator.of(context).pop();
            },
            child: const Text('İptal'),
          ),
          ElevatedButton(
            onPressed: () {
              final name = _nameController.text.trim();
              final dosage = _dosageController.text.trim();
              final time = _timeController.text.trim();
              if (name.isEmpty || dosage.isEmpty || time.isEmpty) {
                return;
              }
              setState(() {
                _records.add(MedicationRecord(name: name, dosage: dosage, time: time));
              });
              _nameController.clear();
              _dosageController.clear();
              _timeController.clear();
              Navigator.of(context).pop();
            },
            child: const Text('Ekle'),
          ),
        ],
      ),
    );
  }

  void _toggleTaken(int index) {
    setState(() {
      _records[index].taken = !_records[index].taken;
    });
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: const Color(0xFFF4F6FB),
      appBar: AppBar(
        backgroundColor: const Color(0xFF6C5CE7),
        title: const Text('İlaç Takibi'),
        elevation: 0,
      ),
      floatingActionButton: FloatingActionButton(
        onPressed: _addMedication,
        backgroundColor: const Color(0xFF6C5CE7),
        child: const Icon(Icons.add),
      ),
      body: Padding(
        padding: const EdgeInsets.all(20),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            const Text(
              'Bugünkü ilaç takibi',
              style: TextStyle(fontSize: 22, fontWeight: FontWeight.bold),
            ),
            const SizedBox(height: 8),
            const Text(
              'İlaçları işaretleyerek kaçar adet alındığını takip edin.',
              style: TextStyle(color: Colors.grey),
            ),
            const SizedBox(height: 20),
            Expanded(
              child: _records.isEmpty
                  ? const Center(
                      child: Text(
                        'Henüz ilaç eklenmedi. Sağ alt butondan ekleyebilirsiniz.',
                        textAlign: TextAlign.center,
                        style: TextStyle(color: Colors.black54),
                      ),
                    )
                  : ListView.separated(
                      itemCount: _records.length,
                      separatorBuilder: (_, __) => const SizedBox(height: 14),
                      itemBuilder: (context, index) {
                        final record = _records[index];
                        return Container(
                          decoration: BoxDecoration(
                            color: Colors.white,
                            borderRadius: BorderRadius.circular(18),
                            boxShadow: const [
                              BoxShadow(
                                color: Color.fromRGBO(0, 0, 0, 0.06),
                                blurRadius: 14,
                                offset: Offset(0, 8),
                              ),
                            ],
                          ),
                          child: ListTile(
                            contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
                            leading: Container(
                              width: 48,
                              height: 48,
                              decoration: BoxDecoration(
                                shape: BoxShape.circle,
                                color: record.taken ? const Color(0xFF2ECC71) : const Color(0xFF6C5CE7),
                              ),
                              child: Icon(
                                record.taken ? Icons.check : Icons.medication,
                                color: Colors.white,
                              ),
                            ),
                            title: Text(
                              record.name,
                              style: const TextStyle(fontWeight: FontWeight.bold),
                            ),
                            subtitle: Text('${record.dosage} • ${record.time}'),
                            trailing: Switch(
                              value: record.taken,
                              activeThumbColor: const Color(0xFF6C5CE7),
                              activeTrackColor: const Color(0xFFD6C7FF),
                              onChanged: (_) => _toggleTaken(index),
                            ),
                            onTap: () => _toggleTaken(index),
                          ),
                        );
                      },
                    ),
            ),
          ],
        ),
      ),
    );
  }
}
