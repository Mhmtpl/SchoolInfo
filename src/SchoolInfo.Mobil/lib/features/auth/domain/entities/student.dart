import 'package:flutter/foundation.dart';

@immutable
class Student {
  final String id;
  final String firstName;
  final String lastName;
  final DateTime dateOfBirth;
  final String classroomId;
  final String? dailyRecordSummary;
  final String? aiSummary;

  const Student({
    required this.id,
    required this.firstName,
    required this.lastName,
    required this.dateOfBirth,
    required this.classroomId,
    this.dailyRecordSummary,
    this.aiSummary,
  });

  factory Student.fromJson(Map<String, dynamic> json) => Student(
    id: json['id']?.toString() ?? '',
    firstName: json['firstName'] as String? ?? '',
    lastName: json['lastName'] as String? ?? '',
    dateOfBirth: json['dateOfBirth'] != null
        ? DateTime.parse(json['dateOfBirth'] as String)
        : DateTime.fromMillisecondsSinceEpoch(0),
    classroomId: json['classroomId']?.toString() ?? '',
    dailyRecordSummary: json['dailyRecordSummary'] as String?,
    aiSummary: json['aiSummary'] as String?,
  );

  Student copyWith({
    String? id,
    String? firstName,
    String? lastName,
    DateTime? dateOfBirth,
    String? classroomId,
    String? dailyRecordSummary,
    String? aiSummary,
  }) {
    return Student(
      id: id ?? this.id,
      firstName: firstName ?? this.firstName,
      lastName: lastName ?? this.lastName,
      dateOfBirth: dateOfBirth ?? this.dateOfBirth,
      classroomId: classroomId ?? this.classroomId,
      dailyRecordSummary: dailyRecordSummary ?? this.dailyRecordSummary,
      aiSummary: aiSummary ?? this.aiSummary,
    );
  }
}
