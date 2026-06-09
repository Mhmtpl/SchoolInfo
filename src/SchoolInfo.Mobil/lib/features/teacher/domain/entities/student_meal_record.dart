import 'package:flutter/foundation.dart';

@immutable
class StudentMealRecord {
  final String studentId;
  final String firstName;
  final String lastName;
  final String? mealRecordId;
  final int? status;
  final String? notes;

  const StudentMealRecord({
    required this.studentId,
    required this.firstName,
    required this.lastName,
    this.mealRecordId,
    this.status,
    this.notes,
  });

  factory StudentMealRecord.fromJson(Map<String, dynamic> json) {
    return StudentMealRecord(
      studentId: json['studentId']?.toString() ?? json['StudentId']?.toString() ?? '',
      firstName: json['firstName'] as String? ?? json['FirstName'] as String? ?? '',
      lastName: json['lastName'] as String? ?? json['LastName'] as String? ?? '',
      mealRecordId: json['mealRecordId']?.toString() ?? json['MealRecordId']?.toString(),
      status: json['status'] as int? ?? json['Status'] as int?,
      notes: json['notes'] as String? ?? json['Notes'] as String?,
    );
  }

  String get statusLabel {
    switch (status) {
      case 1:
        return 'Az yedi';
      case 2:
        return 'Yarı yedi';
      case 3:
        return 'Tamamını yedi';
      default:
        return 'Durum yok';
    }
  }
}
