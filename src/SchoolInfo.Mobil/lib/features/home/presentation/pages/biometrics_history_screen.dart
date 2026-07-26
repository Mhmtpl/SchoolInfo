import 'dart:convert';
import 'package:flutter/material.dart';
import 'package:http/http.dart' as http;
import 'package:fl_chart/fl_chart.dart';

class BiometricsHistoryScreen extends StatefulWidget {
  final String studentId;
  final String studentName;
  final String token;
  final int studentAge;

  const BiometricsHistoryScreen({
    super.key,
    required this.studentId,
    required this.studentName,
    required this.token,
    required this.studentAge,
  });

  @override
  State<BiometricsHistoryScreen> createState() => _BiometricsHistoryScreenState();
}

class _BiometricsHistoryScreenState extends State<BiometricsHistoryScreen> {
  String _currentFilter = 'today';
  bool _isLoading = true;
  List<Map<String, dynamic>> _records = [];
  String _errorMessage = '';

  int _avgBpm = 0;
  int _maxBpm = 0;
  int _minBpm = 0;

  @override
  void initState() {
    super.initState();
    _fetchHistory();
  }

  Future<void> _fetchHistory() async {
    setState(() {
      _isLoading = true;
      _errorMessage = '';
    });

    try {
      String url = 'https://api.veliport.com.tr/api/students/${widget.studentId}/biometrics';
      if (_currentFilter != 'today') {
        url += '?range=$_currentFilter';
      }

      final response = await http.get(
        Uri.parse(url),
        headers: {
          'Authorization': 'Bearer ${widget.token}',
          'Content-Type': 'application/json',
        },
      );

      if (response.statusCode == 200) {
        final List<dynamic> data = jsonDecode(response.body) as List<dynamic>;
        final List<Map<String, dynamic>> loadedRecords = [];
        
        int totalBpm = 0;
        int maxBpm = 0;
        int minBpm = 999;
        int validCount = 0;

        for (var item in data) {
          final map = item as Map<String, dynamic>;
          final hr = map['heartRate'] as int?;
          if (hr != null && hr > 0) {
            loadedRecords.add(map);
            totalBpm += hr;
            if (hr > maxBpm) maxBpm = hr;
            if (hr < minBpm) minBpm = hr;
            validCount++;
          }
        }

        setState(() {
          _records = loadedRecords;
          _isLoading = false;
          if (validCount > 0) {
            _avgBpm = (totalBpm / validCount).round();
            _maxBpm = maxBpm;
            _minBpm = minBpm;
          } else {
            _avgBpm = 0;
            _maxBpm = 0;
            _minBpm = 0;
          }
        });
      } else {
        setState(() {
          _errorMessage = 'Veri çekilemedi. Kod: ${response.statusCode}';
          _isLoading = false;
        });
      }
    } catch (e) {
      setState(() {
        _errorMessage = 'Bağlantı hatası: ${e.toString()}';
        _isLoading = false;
      });
    }
  }

  String _formatDateLabel(String recordedAt) {
    try {
      final dateTime = DateTime.parse(recordedAt).toLocal();
      if (_currentFilter == 'today') {
        final hour = dateTime.hour.toString().padLeft(2, '0');
        final minute = dateTime.minute.toString().padLeft(2, '0');
        return '$hour:$minute';
      } else {
        // Turkish locale short date representation
        final months = ["Oca", "Şub", "Mar", "Nis", "May", "Haz", "Tem", "Ağu", "Eyl", "Eki", "Kas", "Ara"];
        return '${dateTime.day} ${months[dateTime.month - 1]}';
      }
    } catch (_) {
      return '';
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: const Color(0xFFF8FAFC),
      appBar: AppBar(
        title: Text(
          '${widget.studentName} - Ölçüm Geçmişi',
          style: const TextStyle(fontSize: 18, fontWeight: FontWeight.bold, color: Color(0xFF1E293B)),
        ),
        backgroundColor: Colors.white,
        elevation: 0,
        leading: IconButton(
          icon: const Icon(Icons.arrow_back_ios_new, color: Color(0xFF475569), size: 20),
          onPressed: () => Navigator.pop(context),
        ),
      ),
      body: Column(
        children: [
          _buildFilterTabs(),
          Expanded(
            child: _isLoading
                ? const Center(child: CircularProgressIndicator(color: Color(0xFF6366F1)))
                : _errorMessage.isNotEmpty
                    ? Center(child: Text(_errorMessage, style: const TextStyle(color: Colors.red)))
                    : _records.isEmpty
                        ? const Center(child: Text('Kayıtlı veri bulunamadı.'))
                        : SingleChildScrollView(
                            padding: const EdgeInsets.all(20),
                            child: Column(
                              crossAxisAlignment: CrossAxisAlignment.start,
                              children: [
                                _buildSummaryStats(),
                                const SizedBox(height: 24),
                                _buildChartSection(),
                              ],
                            ),
                          ),
          ),
        ],
      ),
    );
  }

  Widget _buildFilterTabs() {
    return Container(
      color: Colors.white,
      padding: const EdgeInsets.symmetric(vertical: 12, horizontal: 20),
      child: Container(
        decoration: BoxDecoration(
          color: const Color(0xFFF1F5F9),
          borderRadius: BorderRadius.circular(12),
        ),
        child: Row(
          children: [
            _buildTabButton('today', 'Bugün'),
            _buildTabButton('7days', '7 Gün (Ort)'),
            _buildTabButton('30days', '30 Gün (Ort)'),
          ],
        ),
      ),
    );
  }

  Widget _buildTabButton(String filterType, String label) {
    final bool isActive = _currentFilter == filterType;
    return Expanded(
      child: GestureDetector(
        onTap: () {
          if (_currentFilter == filterType) return;
          setState(() {
            _currentFilter = filterType;
          });
          _fetchHistory();
        },
        child: Container(
          padding: const EdgeInsets.symmetric(vertical: 10),
          decoration: BoxDecoration(
            color: isActive ? Colors.white : Colors.transparent,
            borderRadius: BorderRadius.circular(10),
            boxShadow: isActive
                ? [
                    const BoxShadow(
                      color: Color.fromRGBO(0, 0, 0, 0.05),
                      blurRadius: 4,
                      offset: Offset(0, 2),
                    ),
                  ]
                : null,
          ),
          child: Text(
            label,
            textAlign: TextAlign.center,
            style: TextStyle(
              fontSize: 13,
              fontWeight: isActive ? FontWeight.bold : FontWeight.w500,
              color: isActive ? const Color(0xFF1E293B) : const Color(0xFF64748B),
            ),
          ),
        ),
      ),
    );
  }

  Widget _buildSummaryStats() {
    return Row(
      children: [
        _buildStatCard('ORTALAMA', '$_avgBpm', 'BPM', const Color(0xFF6366F1)),
        const SizedBox(width: 12),
        _buildStatCard('MİNİMUM', '$_minBpm', 'BPM', const Color(0xFF0284C7)),
        const SizedBox(width: 12),
        _buildStatCard('MAKSİMUM', '$_maxBpm', 'BPM', const Color(0xFFEF4444)),
      ],
    );
  }

  Widget _buildStatCard(String label, String value, String unit, Color color) {
    return Expanded(
      child: Container(
        padding: const EdgeInsets.all(16),
        decoration: BoxDecoration(
          color: Colors.white,
          borderRadius: BorderRadius.circular(20),
          border: Border.all(color: const Color(0xFFE2E8F0)),
        ),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              label,
              style: const TextStyle(fontSize: 10, fontWeight: FontWeight.bold, color: Color(0xFF94A3B8)),
            ),
            const SizedBox(height: 8),
            Row(
              textBaseline: TextBaseline.alphabetic,
              crossAxisAlignment: CrossAxisAlignment.baseline,
              children: [
                Text(
                  value,
                  style: TextStyle(fontSize: 24, fontWeight: FontWeight.bold, color: color),
                ),
                const SizedBox(width: 2),
                Text(
                  unit,
                  style: const TextStyle(fontSize: 10, fontWeight: FontWeight.bold, color: Color(0xFF64748B)),
                ),
              ],
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildChartSection() {
    // Verileri fl_chart formatına çevirelim
    final List<FlSpot> spots = [];
    for (int i = 0; i < _records.length; i++) {
      final double y = (_records[i]['heartRate'] as num).toDouble();
      spots.add(FlSpot(i.toDouble(), y));
    }

    return Container(
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(24),
        border: Border.all(color: const Color(0xFFE2E8F0)),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          const Text(
            'Kalp Ritim Grafiği',
            style: TextStyle(fontSize: 14, fontWeight: FontWeight.bold, color: Color(0xFF1E293B)),
          ),
          const SizedBox(height: 24),
          SizedBox(
            height: 220,
            child: LineChart(
              LineChartData(
                gridData: FlGridData(
                  show: true,
                  drawVerticalLine: false,
                  getDrawingHorizontalLine: (value) {
                    return const FlLine(
                      color: Color(0xFFF1F5F9),
                      strokeWidth: 1.0,
                    );
                  },
                ),
                titlesData: FlTitlesData(
                  show: true,
                  rightTitles: const AxisTitles(sideTitles: SideTitles(showTitles: false)),
                  topTitles: const AxisTitles(sideTitles: SideTitles(showTitles: false)),
                  bottomTitles: AxisTitles(
                    sideTitles: SideTitles(
                      showTitles: true,
                      reservedSize: 30,
                      interval: (_records.length / 5).clamp(1.0, 999.0),
                      getTitlesWidget: (value, meta) {
                        final int index = value.toInt();
                        if (index < 0 || index >= _records.length) return const SizedBox();
                        return Padding(
                          padding: const EdgeInsets.only(top: 8.0),
                          child: Text(
                            _formatDateLabel(_records[index]['recordedAt'] as String),
                            style: const TextStyle(fontSize: 10, color: Color(0xFF64748B), fontFamily: 'monospace'),
                          ),
                        );
                      },
                    ),
                  ),
                  leftTitles: AxisTitles(
                    sideTitles: SideTitles(
                      showTitles: true,
                      reservedSize: 32,
                      interval: 20,
                      getTitlesWidget: (value, meta) {
                        return Text(
                          value.toInt().toString(),
                          style: const TextStyle(fontSize: 10, color: Color(0xFF64748B)),
                        );
                      },
                    ),
                  ),
                ),
                borderData: FlBorderData(show: false),
                minX: 0,
                maxX: (_records.length - 1).toDouble(),
                minY: 40,
                maxY: 160,
                lineTouchData: LineTouchData(
                  touchTooltipData: LineTouchTooltipData(
                    getTooltipColor: (spot) => const Color(0xFF1E293B),
                    tooltipRoundedRadius: 8,
                    getTooltipItems: (touchedSpots) {
                      return touchedSpots.map((barSpot) {
                        final index = barSpot.x.toInt();
                        final parsedDate = DateTime.parse(_records[index]['recordedAt'] as String).toLocal();
                        final hour = parsedDate.hour.toString().padLeft(2, '0');
                        final minute = parsedDate.minute.toString().padLeft(2, '0');
                        final String time = '$hour:$minute';
                        return LineTooltipItem(
                          'Nabız: ${barSpot.y.round()} BPM\nSaat: $time',
                          const TextStyle(color: Colors.white, fontSize: 11, fontWeight: FontWeight.w500),
                        );
                      }).toList();
                    },
                  ),
                  handleBuiltInTouches: true,
                ),
                lineBarsData: [
                  LineChartBarData(
                    spots: spots,
                    isCurved: true,
                    color: const Color(0xFF6366F1),
                    barWidth: 2,
                    isStrokeCapRound: true,
                    dotData: const FlDotData(show: false),
                    belowBarData: BarAreaData(
                      show: true,
                      gradient: LinearGradient(
                        colors: [
                          const Color(0xFF6366F1).withOpacity(0.25),
                          const Color(0xFF6366F1).withOpacity(0.0),
                        ],
                        begin: Alignment.topCenter,
                        end: Alignment.bottomCenter,
                      ),
                    ),
                  ),
                ],
              ),
            ),
          ),
        ],
      ),
    );
  }
}
