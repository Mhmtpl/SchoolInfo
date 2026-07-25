import 'dart:convert';
import 'package:flutter/material.dart';
import 'package:http/http.dart' as http;
import '../../../../features/teacher/domain/entities/weekly_meal_plan.dart';

class MealScreen extends StatefulWidget {
  final String classroomId;
  final String token;

  const MealScreen({
    super.key,
    required this.classroomId,
    required this.token,
  });

  @override
  State<MealScreen> createState() => _MealScreenState();
}

class _MealScreenState extends State<MealScreen> {
  bool _isLoading = true;
  String? _error;
  List<WeeklyMealPlan> _allPlans = [];
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
    // Set default day based on current weekday, clamped to Mon-Fri
    final weekday = DateTime.now().weekday;
    if (weekday >= 1 && weekday <= 5) {
      _selectedDay = weekday;
    } else {
      _selectedDay = 1; // Default to Monday if weekend
    }
    _fetchWeeklyMealPlans();
  }

  Future<void> _fetchWeeklyMealPlans() async {
    if (widget.classroomId.isEmpty) {
      setState(() {
        _isLoading = false;
        _error = 'Sınıf bilgisi bulunamadı.';
      });
      return;
    }

    try {
      final response = await http.get(
        Uri.parse('https://api.veliport.com.tr/api/classrooms/${widget.classroomId}/weekly-meal-plans'),
        headers: {
          'Content-Type': 'application/json',
          'Authorization': 'Bearer ${widget.token}',
        },
      );

      if (response.statusCode == 200) {
        final List<dynamic> jsonList = jsonDecode(response.body) as List<dynamic>;
        final plans = jsonList.map((j) => WeeklyMealPlan.fromJson(j as Map<String, dynamic>)).toList();
        setState(() {
          _allPlans = plans;
          _isLoading = false;
        });
      } else {
        setState(() {
          _error = 'Yemek planı yüklenemedi (Kod: ${response.statusCode})';
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

  List<WeeklyMealPlan> get _filteredPlans {
    return _allPlans.where((p) => p.dayOfWeek == _selectedDay).toList();
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Scaffold(
      backgroundColor: const Color(0xFFF8FAFC),
      appBar: AppBar(
        backgroundColor: Colors.white,
        foregroundColor: const Color(0xFF1E293B),
        elevation: 0,
        title: const Text(
          'Haftalık Yemek Menüsü',
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
              : _filteredPlans.isEmpty
                  ? Center(
                      child: Column(
                        mainAxisAlignment: MainAxisAlignment.center,
                        children: [
                          Icon(Icons.restaurant_menu_rounded, size: 64, color: Colors.grey.shade300),
                          const SizedBox(height: 16),
                          const Text(
                            'Bu güne ait yemek planı bulunmuyor.',
                            style: TextStyle(color: Color(0xFF64748B), fontWeight: FontWeight.w600),
                          ),
                        ],
                      ),
                    )
                  : ListView.builder(
                      padding: const EdgeInsets.all(16),
                      itemCount: _filteredPlans.length,
                      itemBuilder: (context, index) {
                        final plan = _filteredPlans[index];
                        return Container(
                          margin: const EdgeInsets.only(bottom: 16),
                          padding: const EdgeInsets.all(18),
                          decoration: BoxDecoration(
                            color: Colors.white,
                            borderRadius: BorderRadius.circular(24),
                            border: Border.all(color: const Color(0xFFE2E8F0)),
                            boxShadow: const [
                              BoxShadow(
                                color: Color.fromRGBO(15, 23, 42, 0.02),
                                blurRadius: 16,
                                offset: Offset(0, 8),
                              ),
                            ],
                          ),
                          child: Column(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              Row(
                                children: [
                                  Container(
                                    padding: const EdgeInsets.all(6),
                                    decoration: BoxDecoration(
                                      color: theme.colorScheme.primary.withOpacity(0.06),
                                      shape: BoxShape.circle,
                                    ),
                                    child: Icon(Icons.restaurant_rounded, color: theme.colorScheme.primary, size: 16),
                                  ),
                                  const SizedBox(width: 8),
                                  Text(
                                    plan.mealName,
                                    style: const TextStyle(
                                      fontSize: 16,
                                      fontWeight: FontWeight.w800,
                                      color: Color(0xFF1E293B),
                                    ),
                                  ),
                                ],
                              ),
                              const SizedBox(height: 12),
                              Text(
                                plan.foodContent ?? 'İçerik belirtilmemiş',
                                style: const TextStyle(
                                  fontSize: 13.5,
                                  color: Color(0xFF475569),
                                  height: 1.5,
                                  fontWeight: FontWeight.w500,
                                ),
                              ),
                              const SizedBox(height: 18),
                              const Divider(color: Color(0xFFF1F5F9), height: 1),
                              const SizedBox(height: 12),
                              Row(
                                children: [
                                  if (plan.plannedCalories != null) ...[
                                    _buildNutriTag(Icons.local_fire_department_rounded, '${plan.plannedCalories} kcal', Colors.orange),
                                    const SizedBox(width: 12),
                                  ],
                                  if (plan.proteinGrams != null) ...[
                                    _buildNutriTag(Icons.fitness_center_rounded, '${plan.proteinGrams!.toStringAsFixed(0)}g Prot', Colors.green),
                                    const SizedBox(width: 12),
                                  ],
                                  if (plan.carbsGrams != null) ...[
                                    _buildNutriTag(Icons.grain_rounded, '${plan.carbsGrams!.toStringAsFixed(0)}g Karb', Colors.blue),
                                  ],
                                ],
                              )
                            ],
                          ),
                        );
                      },
                    ),
    );
  }

  Widget _buildNutriTag(IconData icon, String label, Color color) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 6),
      decoration: BoxDecoration(
        color: color.withOpacity(0.06),
        borderRadius: BorderRadius.circular(10),
      ),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(icon, color: color, size: 13),
          const SizedBox(width: 4),
          Text(
            label,
            style: TextStyle(
              fontSize: 11,
              fontWeight: FontWeight.bold,
              color: color,
            ),
          ),
        ],
      ),
    );
  }
}
