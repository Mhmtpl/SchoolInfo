import 'dart:convert';
import 'package:flutter/material.dart';
import 'package:http/http.dart' as http;

class WeeklySchedule {
  final int dayOfWeek;
  final String title;
  final String description;
  final String startTime;
  final String endTime;
  final int type;

  const WeeklySchedule({
    required this.dayOfWeek,
    required this.title,
    required this.description,
    required this.startTime,
    required this.endTime,
    required this.type,
  });

  factory WeeklySchedule.fromJson(Map<String, dynamic> json) {
    return WeeklySchedule(
      dayOfWeek: json['dayOfWeek'] as int? ?? json['DayOfWeek'] as int? ?? 0,
      title: json['title'] as String? ?? json['Title'] as String? ?? '',
      description: json['description'] as String? ?? json['Description'] as String? ?? '',
      startTime: json['startTime'] as String? ?? json['StartTime'] as String? ?? '',
      endTime: json['endTime'] as String? ?? json['EndTime'] as String? ?? '',
      type: json['type'] as int? ?? json['Type'] as int? ?? 0,
    );
  }

  String get formattedTime {
    String format(String t) {
      final parts = t.split(':');
      if (parts.length >= 2) {
        return '${parts[0]}:${parts[1]}';
      }
      return t;
    }
    return '${format(startTime)} - ${format(endTime)}';
  }
}

class ActivityScreen extends StatefulWidget {
  final String classroomId;
  final String token;
  final bool isEmbedded;

  const ActivityScreen({
    super.key,
    required this.classroomId,
    required this.token,
    this.isEmbedded = false,
  });

  @override
  State<ActivityScreen> createState() => _ActivityScreenState();
}

class _ActivityScreenState extends State<ActivityScreen> {
  bool _isLoading = true;
  String? _error;
  List<WeeklySchedule> _allSchedules = [];
  int _selectedDay = 1; // Default to Monday (1)

  final List<Map<String, dynamic>> _days = [
    {'label': 'Pazartesi', 'value': 1},
    {'label': 'Salı', 'value': 2},
    {'label': 'Çarşamba', 'value': 3},
    {'label': 'Perşembe', 'value': 4},
    {'label': 'Cuma', 'value': 5},
  ];

  @override
  void initState() {
    super.initState();
    final weekday = DateTime.now().weekday;
    if (weekday >= 1 && weekday <= 5) {
      _selectedDay = weekday;
    } else {
      _selectedDay = 1;
    }
    _fetchWeeklySchedule();
  }

  @override
  void didUpdateWidget(covariant ActivityScreen oldWidget) {
    super.didUpdateWidget(oldWidget);
    if (oldWidget.classroomId != widget.classroomId || oldWidget.token != widget.token) {
      _isLoading = true;
      _fetchWeeklySchedule();
    }
  }

  Future<void> _fetchWeeklySchedule() async {
    if (widget.classroomId.isEmpty) {
      setState(() {
        _isLoading = false;
        _error = 'Sınıf bilgisi bulunamadı.';
      });
      return;
    }

    try {
      final response = await http.get(
        Uri.parse('https://api.veliport.com.tr/api/classrooms/${widget.classroomId}/weekly-schedule'),
        headers: {
          'Content-Type': 'application/json',
          'Authorization': 'Bearer ${widget.token}',
        },
      );

      if (response.statusCode == 200) {
        final List<dynamic> jsonList = jsonDecode(response.body) as List<dynamic>;
        final schedules = jsonList.map((j) => WeeklySchedule.fromJson(j as Map<String, dynamic>)).toList();
        setState(() {
          _allSchedules = schedules;
          _isLoading = false;
        });
      } else {
        setState(() {
          _error = 'Ders programı yüklenemedi (Kod: ${response.statusCode})';
          _isLoading = false;
        });
      }
    } catch (e) {
      setState(() {
        _error = 'Bağlantı hatası oluştu: $e';
        _isLoading = false;
      });
    }
  }

  List<WeeklySchedule> get _filteredSchedules {
    return _allSchedules.where((s) => s.dayOfWeek == _selectedDay).toList();
  }

  bool _isCurrentlyActive(WeeklySchedule s) {
    final now = DateTime.now();
    if (now.weekday != _selectedDay) return false;

    int parseMinutes(String timeStr) {
      try {
        final parts = timeStr.split(':');
        if (parts.isEmpty) return 0;
        final hour = int.parse(parts[0]);
        final minute = parts.length > 1 ? int.parse(parts[1]) : 0;
        return hour * 60 + minute;
      } catch (_) {
        return 0;
      }
    }

    final currentMinutes = now.hour * 60 + now.minute;
    final startMinutes = parseMinutes(s.startTime);
    final endMinutes = parseMinutes(s.endTime);

    return currentMinutes >= startMinutes && currentMinutes <= endMinutes;
  }

  IconData _getIconForType(int type) {
    switch (type) {
      case 1: // Education / Ders
        return Icons.menu_book_rounded;
      case 2: // Meal / Yemek
        return Icons.restaurant_rounded;
      case 3: // Sleep / Uyku
        return Icons.bedtime_rounded;
      case 4: // Game / Play
        return Icons.sports_esports_rounded;
      case 5: // Outdoor
        return Icons.forest_rounded;
      default:
        return Icons.star_rounded;
    }
  }

  Color _getColorForType(int type) {
    switch (type) {
      case 1:
        return const Color(0xFF3B82F6); // Blue
      case 2:
        return const Color(0xFFF97316); // Orange
      case 3:
        return const Color(0xFFF59E0B); // Amber
      case 4:
        return const Color(0xFF8B5CF6); // Purple
      case 5:
        return const Color(0xFF10B981); // Green
      default:
        return const Color(0xFF6366F1); // Indigo
    }
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Scaffold(
      backgroundColor: const Color(0xFFF8FAFC),
      appBar: AppBar(
        automaticallyImplyLeading: !widget.isEmbedded,
        backgroundColor: Colors.white,
        foregroundColor: const Color(0xFF1E293B),
        elevation: 0,
        title: const Text(
          'Ders & Aktivite Programı',
          style: TextStyle(fontWeight: FontWeight.w800, fontSize: 18),
        ),
        bottom: PreferredSize(
          preferredSize: const Size.fromHeight(60),
          child: Container(
            color: Colors.white,
            padding: const EdgeInsets.only(bottom: 12),
            height: 52,
            child: ListView.builder(
              scrollDirection: Axis.horizontal,
              padding: const EdgeInsets.symmetric(horizontal: 16),
              itemCount: _days.length,
              itemBuilder: (context, index) {
                final d = _days[index];
                final isSelected = d['value'] == _selectedDay;
                return GestureDetector(
                  onTap: () {
                    setState(() {
                      _selectedDay = d['value'] as int;
                    });
                  },
                  child: AnimatedContainer(
                    duration: const Duration(milliseconds: 200),
                    margin: const EdgeInsets.only(right: 10),
                    padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
                    decoration: BoxDecoration(
                      color: isSelected ? theme.colorScheme.primary : const Color(0xFFF1F5F9),
                      borderRadius: BorderRadius.circular(12),
                    ),
                    child: Center(
                      child: Text(
                        d['label'] as String,
                        style: TextStyle(
                          color: isSelected ? Colors.white : const Color(0xFF64748B),
                          fontWeight: FontWeight.bold,
                          fontSize: 13,
                        ),
                      ),
                    ),
                  ),
                );
              },
            ),
          ),
        ),
      ),
      body: _isLoading
          ? const Center(child: CircularProgressIndicator())
          : _error != null
              ? Center(
                  child: Padding(
                    padding: const EdgeInsets.all(24.0),
                    child: Text(
                      _error!,
                      style: const TextStyle(color: Colors.red, fontWeight: FontWeight.bold),
                      textAlign: TextAlign.center,
                    ),
                  ),
                )
              : _filteredSchedules.isEmpty
                  ? Center(
                      child: Column(
                        mainAxisAlignment: MainAxisAlignment.center,
                        children: [
                          Icon(Icons.event_busy_rounded, size: 64, color: Colors.grey.shade300),
                          const SizedBox(height: 16),
                          const Text(
                            'Bu güne ait aktivite planı bulunmuyor.',
                            style: TextStyle(color: Color(0xFF64748B), fontWeight: FontWeight.w600),
                          ),
                        ],
                      ),
                    )
                  : ListView.builder(
                      padding: const EdgeInsets.all(20),
                      itemCount: _filteredSchedules.length,
                      itemBuilder: (context, index) {
                        final sched = _filteredSchedules[index];
                        final isActive = _isCurrentlyActive(sched);
                        final typeColor = _getColorForType(sched.type);
                        final typeIcon = _getIconForType(sched.type);

                        return IntrinsicHeight(
                          child: Row(
                            crossAxisAlignment: CrossAxisAlignment.stretch,
                            children: [
                              // Left Timeline Indicator
                              SizedBox(
                                width: 72,
                                child: Column(
                                  mainAxisAlignment: MainAxisAlignment.start,
                                  crossAxisAlignment: CrossAxisAlignment.end,
                                  children: [
                                    const SizedBox(height: 18),
                                    Text(
                                      sched.startTime.substring(0, 5),
                                      style: TextStyle(
                                        fontSize: 14,
                                        fontWeight: isActive ? FontWeight.w900 : FontWeight.w700,
                                        color: isActive ? typeColor : const Color(0xFF1E293B),
                                      ),
                                    ),
                                    const SizedBox(height: 4),
                                    Text(
                                      sched.endTime.substring(0, 5),
                                      style: const TextStyle(
                                        fontSize: 11,
                                        fontWeight: FontWeight.w600,
                                        color: Color(0xFF64748B),
                                      ),
                                    ),
                                  ],
                                ),
                              ),
                              const SizedBox(width: 14),
                              // Middle Line & Dot
                              Column(
                                children: [
                                  const SizedBox(height: 20),
                                  AnimatedContainer(
                                    duration: const Duration(milliseconds: 300),
                                    width: isActive ? 16 : 10,
                                    height: isActive ? 16 : 10,
                                    decoration: BoxDecoration(
                                      color: isActive ? typeColor : Colors.white,
                                      shape: BoxShape.circle,
                                      border: Border.all(
                                        color: typeColor,
                                        width: isActive ? 3 : 2,
                                      ),
                                      boxShadow: isActive
                                          ? [
                                              BoxShadow(
                                                color: typeColor.withOpacity(0.4),
                                                blurRadius: 10,
                                                spreadRadius: 2,
                                              )
                                            ]
                                          : null,
                                    ),
                                  ),
                                  Expanded(
                                    child: Container(
                                      width: 2,
                                      color: index == _filteredSchedules.length - 1
                                          ? Colors.transparent
                                          : const Color(0xFFE2E8F0),
                                    ),
                                  ),
                                ],
                              ),
                              const SizedBox(width: 14),
                              // Right Activity Card
                              Expanded(
                                child: Container(
                                  margin: const EdgeInsets.only(bottom: 16),
                                  padding: const EdgeInsets.all(16),
                                  decoration: BoxDecoration(
                                    color: Colors.white,
                                    borderRadius: BorderRadius.circular(20),
                                    border: Border.all(
                                      color: isActive ? typeColor : const Color(0xFFE2E8F0),
                                      width: isActive ? 1.5 : 1,
                                    ),
                                    boxShadow: [
                                      BoxShadow(
                                        color: isActive
                                            ? typeColor.withOpacity(0.04)
                                            : const Color.fromRGBO(15, 23, 42, 0.02),
                                        blurRadius: 16,
                                        offset: const Offset(0, 8),
                                      ),
                                    ],
                                  ),
                                  child: Column(
                                    crossAxisAlignment: CrossAxisAlignment.start,
                                    children: [
                                      Row(
                                        children: [
                                          Icon(typeIcon, color: typeColor, size: 16),
                                          const SizedBox(width: 6),
                                          Expanded(
                                            child: Text(
                                              sched.title,
                                              style: TextStyle(
                                                fontSize: 14.5,
                                                fontWeight: FontWeight.w800,
                                                color: isActive ? typeColor : const Color(0xFF1E293B),
                                              ),
                                            ),
                                          ),
                                          if (isActive) ...[
                                            const SizedBox(width: 6),
                                            Container(
                                              padding: const EdgeInsets.symmetric(horizontal: 6, vertical: 2),
                                              decoration: BoxDecoration(
                                                color: const Color(0xFF2ECC71).withOpacity(0.12),
                                                borderRadius: BorderRadius.circular(6),
                                              ),
                                              child: const Text(
                                                'ŞU AN',
                                                style: TextStyle(
                                                  fontSize: 8,
                                                  fontWeight: FontWeight.w800,
                                                  color: Color(0xFF16A34A),
                                                  letterSpacing: 0.5,
                                                ),
                                              ),
                                            ),
                                          ]
                                        ],
                                      ),
                                      if (sched.description.isNotEmpty) ...[
                                        const SizedBox(height: 8),
                                        Text(
                                          sched.description,
                                          style: const TextStyle(
                                            fontSize: 12,
                                            color: Color(0xFF475569),
                                            height: 1.4,
                                            fontWeight: FontWeight.w500,
                                          ),
                                        ),
                                      ],
                                    ],
                                  ),
                                ),
                              ),
                            ],
                          ),
                        );
                      },
                    ),
    );
  }
}
