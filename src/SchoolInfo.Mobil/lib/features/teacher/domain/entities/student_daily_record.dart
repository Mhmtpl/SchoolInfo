import 'package:flutter/foundation.dart';

@immutable
class StudentDailyRecord {
  final String id;
  final String studentId;
  final int sleepStatus;
  final int waterIntake;
  final String? teacherNotes;

  const StudentDailyRecord({
    required this.id,
    required this.studentId,
    required this.sleepStatus,
    required this.waterIntake,
    this.teacherNotes,
  });

  factory StudentDailyRecord.fromJson(Map<String, dynamic> json) {
    return StudentDailyRecord(
      id: json['id']?.toString() ?? json['Id']?.toString() ?? '',
      studentId: json['studentId']?.toString() ?? json['StudentId']?.toString() ?? '',
      sleepStatus: _parseSleepStatus(
        json['sleepStatus'] as String? ?? json['SleepStatus'] as String?,
      ),
      waterIntake: json['waterIntake'] as int? ?? json['WaterIntake'] as int? ?? 0,
      teacherNotes: json['teacherNotes'] as String? ?? json['TeacherNotes'] as String?,
    );
  }

  static int _parseSleepStatus(String? value) {
    final normalized = value?.toLowerCase();
    switch (normalized) {
      case 'didnotsleep':
      case 'did not sleep':
      case '0':
        return 0;
      case 'sleptlittle':
      case 'slept little':
      case '1':
        return 1;
      case 'sleptwell':
      case 'slept well':
      case '2':
        return 2;
      default:
        return 0;
    }
  }
}
