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
  final String classroomName;
  final String schoolName;

  const Student({
    required this.id,
    required this.firstName,
    required this.lastName,
    required this.dateOfBirth,
    required this.classroomId,
    this.dailyRecordSummary,
    this.aiSummary,
    this.classroomName = 'Bilinmeyen Sınıf',
    this.schoolName = 'Veliport Portal',
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
    classroomName: json['classroomName'] as String? ?? 'Bilinmeyen Sınıf',
    schoolName: json['schoolName'] as String? ?? 'Veliport Portal',
  );

  Student copyWith({
    String? id,
    String? firstName,
    String? lastName,
    DateTime? dateOfBirth,
    String? classroomId,
    String? dailyRecordSummary,
    String? aiSummary,
    String? classroomName,
    String? schoolName,
  }) {
    return Student(
      id: id ?? this.id,
      firstName: firstName ?? this.firstName,
      lastName: lastName ?? this.lastName,
      dateOfBirth: dateOfBirth ?? this.dateOfBirth,
      classroomId: classroomId ?? this.classroomId,
      dailyRecordSummary: dailyRecordSummary ?? this.dailyRecordSummary,
      aiSummary: aiSummary ?? this.aiSummary,
      classroomName: classroomName ?? this.classroomName,
      schoolName: schoolName ?? this.schoolName,
    );
  }

  Map<String, dynamic> toJson() => {
    'id': id,
    'firstName': firstName,
    'lastName': lastName,
    'dateOfBirth': dateOfBirth.toIso8601String(),
    'classroomId': classroomId,
    'dailyRecordSummary': dailyRecordSummary,
    'aiSummary': aiSummary,
    'classroomName': classroomName,
    'schoolName': schoolName,
  };
}
