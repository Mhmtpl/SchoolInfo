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
  final bool isEmbedded;
  const MedicationScreen({super.key, this.isEmbedded = false});

  @override
  State<MedicationScreen> createState() => _MedicationScreenState();
}

class _MedicationScreenState extends State<MedicationScreen> {
  final List<MedicationRecord> _records = [];

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
        automaticallyImplyLeading: !widget.isEmbedded,
        backgroundColor: Colors.white,
        foregroundColor: const Color(0xFF1E293B),
        elevation: 0,
        title: const Text(
          'İlaç Defteri',
          style: TextStyle(fontWeight: FontWeight.w800, fontSize: 18),
        ),
      ),
      floatingActionButton: FloatingActionButton(
        onPressed: _addMedication,
        backgroundColor: Theme.of(context).colorScheme.primary,
        child: const Icon(Icons.add, color: Colors.white),
      ),
      body: Padding(
        padding: const EdgeInsets.all(20),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            const Text(
              'Bugün Verilecek İlaçlar',
              style: TextStyle(fontSize: 22, fontWeight: FontWeight.bold),
            ),
            const SizedBox(height: 8),
            const Text(
              'Okulda verilmesi gereken ilaçlar için öğretmenlere talimat bırakın.',
              style: TextStyle(color: Colors.grey),
            ),
            const SizedBox(height: 20),
            Expanded(
              child: _records.isEmpty
                  ? const Center(
                      child: Text(
                        'Henüz bir ilaç talimatı bırakmadınız. Yeni talimat eklemek için + butonuna basabilirsiniz.',
                        textAlign: TextAlign.center,
                        style: TextStyle(color: Colors.black54),
                      ),
                    )
                  : ListView.separated(
                      itemCount: _records.length,
                      separatorBuilder: (_, __) => const SizedBox(height: 14),
                      itemBuilder: (context, index) {
                        final record = _records[index];
                        final theme = Theme.of(context);
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
                                color: record.taken 
                                    ? const Color(0xFFDCFCE7) 
                                    : theme.colorScheme.primary.withOpacity(0.1),
                              ),
                              child: Icon(
                                record.taken ? Icons.check : Icons.medication,
                                color: record.taken 
                                    ? const Color(0xFF15803D) 
                                    : theme.colorScheme.primary,
                              ),
                            ),
                            title: Text(
                              record.name,
                              style: const TextStyle(fontWeight: FontWeight.bold),
                            ),
                            subtitle: Text('${record.dosage} • ${record.time}'),
                            trailing: Switch(
                              value: record.taken,
                              activeColor: theme.colorScheme.primary,
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
