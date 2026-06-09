import 'package:flutter/foundation.dart';

@immutable
class WeeklyMealPlan {
  final String id;
  final int dayOfWeek;
  final String mealName;
  final int? plannedCalories;
  final String? foodContent;
  final double? proteinGrams;
  final double? carbsGrams;

  const WeeklyMealPlan({
    required this.id,
    required this.dayOfWeek,
    required this.mealName,
    this.plannedCalories,
    this.foodContent,
    this.proteinGrams,
    this.carbsGrams,
  });

  factory WeeklyMealPlan.fromJson(Map<String, dynamic> json) {
    return WeeklyMealPlan(
      id: json['id']?.toString() ?? json['Id']?.toString() ?? '',
      dayOfWeek:
          json['dayOfWeek'] as int? ??
          int.tryParse(json['DayOfWeek']?.toString() ?? '') ??
          0,
      mealName:
          json['mealName'] as String? ?? json['MealName'] as String? ?? '',
      plannedCalories:
          json['plannedCalories'] as int? ??
          int.tryParse(json['PlannedCalories']?.toString() ?? ''),
      foodContent:
          json['foodContent'] as String? ?? json['FoodContent'] as String?,
      proteinGrams: (json['proteinGrams'] is num
          ? (json['proteinGrams'] as num).toDouble()
          : double.tryParse(json['ProteinGrams']?.toString() ?? '')),
      carbsGrams: (json['carbsGrams'] is num
          ? (json['carbsGrams'] as num).toDouble()
          : double.tryParse(json['CarbsGrams']?.toString() ?? '')),
    );
  }

  String get dayLabel {
    const dayNames = [
      'Pazar',
      'Pazartesi',
      'Salı',
      'Çarşamba',
      'Perşembe',
      'Cuma',
      'Cumartesi',
    ];
    if (dayOfWeek >= 0 && dayOfWeek < dayNames.length) {
      return dayNames[dayOfWeek];
    }
    return 'Bilinmeyen';
  }

  String get nutritionSummary {
    final items = <String>[];
    if (plannedCalories != null) items.add('$plannedCalories kcal');
    if (proteinGrams != null)
      items.add('${proteinGrams!.toStringAsFixed(0)}g protein');
    if (carbsGrams != null)
      items.add('${carbsGrams!.toStringAsFixed(0)}g karbonhidrat');
    if (foodContent != null && foodContent!.isNotEmpty) items.add(foodContent!);
    return items.join(' • ');
  }
}
