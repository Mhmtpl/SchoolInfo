import 'package:flutter/foundation.dart';

@immutable
class MealDetail {
  final String mealRecordId;
  final String mealName;
  final int statusType;
  final String statusDescription;

  const MealDetail({
    required this.mealRecordId,
    required this.mealName,
    required this.statusType,
    required this.statusDescription,
  });

  factory MealDetail.fromJson(Map<String, dynamic> json) {
    return MealDetail(
      mealRecordId: json['mealRecordId']?.toString() ?? json['MealRecordId']?.toString() ?? '',
      mealName: json['mealName']?.toString() ?? json['MealName']?.toString() ?? '',
      statusType: json['statusType'] as int? ?? json['StatusType'] as int? ?? 0,
      statusDescription: json['statusDescription']?.toString() ?? json['StatusDescription']?.toString() ?? '',
    );
  }

  String get statusLabel {
    switch (statusType) {
      case 1:
        return 'Az yedi';
      case 2:
        return 'Yarı yedi';
      case 3:
        return 'Tamamını yedi';
      default:
        return 'Hiç yemedi';
    }
  }
}

@immutable
class StudentMealRecord {
  final String studentId;
  final String firstName;
  final String lastName;
  final List<MealDetail> meals;

  const StudentMealRecord({
    required this.studentId,
    required this.firstName,
    required this.lastName,
    required this.meals,
  });

  factory StudentMealRecord.fromJson(Map<String, dynamic> json) {
    final list = json['meals'] as List<dynamic>? ?? json['Meals'] as List<dynamic>? ?? [];
    return StudentMealRecord(
      studentId: json['studentId']?.toString() ?? json['StudentId']?.toString() ?? '',
      firstName: json['firstName'] as String? ?? json['FirstName'] as String? ?? '',
      lastName: json['lastName'] as String? ?? json['LastName'] as String? ?? '',
      meals: list.map((item) => MealDetail.fromJson(item as Map<String, dynamic>)).toList(),
    );
  }
}
