import 'package:flutter/foundation.dart';

@immutable
class ClassroomDailyRecord {
  final String studentId;
  final String firstName;
  final String lastName;
  final bool hasRecordToday;
  final String? dailyRecordId;
  final int? sleepStatus;
  final int? waterIntake;
  final String? teacherNotes;
  final bool isAbsent;

  const ClassroomDailyRecord({
    required this.studentId,
    required this.firstName,
    required this.lastName,
    required this.hasRecordToday,
    this.dailyRecordId,
    this.sleepStatus,
    this.waterIntake,
    this.teacherNotes,
    required this.isAbsent,
  });

  factory ClassroomDailyRecord.fromJson(Map<String, dynamic> json) {
    return ClassroomDailyRecord(
      studentId: json['studentId']?.toString() ?? json['StudentId']?.toString() ?? '',
      firstName: json['firstName'] as String? ?? json['FirstName'] as String? ?? '',
      lastName: json['lastName'] as String? ?? json['LastName'] as String? ?? '',
      hasRecordToday: json['hasRecordToday'] as bool? ?? json['HasRecordToday'] as bool? ?? false,
        dailyRecordId: json['dailyRecordId']?.toString() ?? json['DailyRecordId']?.toString(),
      sleepStatus: json['sleepStatus'] as int? ?? json['SleepStatus'] as int?,
      waterIntake: json['waterIntake'] as int? ?? json['WaterIntake'] as int?,
      teacherNotes: json['teacherNotes'] as String? ?? json['TeacherNotes'] as String?,
      isAbsent: json['isAbsent'] as bool? ?? json['IsAbsent'] as bool? ?? false,
    );
  }

  String get sleepStatusLabel {
    switch (sleepStatus) {
      case 0:
        return 'Uyumadı';
      case 1:
        return 'Az uyudu';
      case 2:
        return 'İyi uyudu';
      default:
        return 'Bilinmiyor';
    }
  }
}
