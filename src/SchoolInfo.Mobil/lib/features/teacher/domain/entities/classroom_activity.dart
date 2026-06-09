import 'package:flutter/material.dart';
import 'package:flutter/foundation.dart';

@immutable
class ClassroomActivity {
  final String id;
  final String title;
  final String description;
  final DateTime activityDate;
  final TimeOfDay startTime;
  final TimeOfDay endTime;
  final int type;
  final bool completed;

  const ClassroomActivity({
    required this.id,
    required this.title,
    required this.description,
    required this.activityDate,
    required this.startTime,
    required this.endTime,
    required this.type,
    required this.completed,
  });

  factory ClassroomActivity.fromJson(Map<String, dynamic> json) {
    TimeOfDay _parseTime(dynamic value) {
      if (value == null) return const TimeOfDay(hour: 0, minute: 0);
      final string = value.toString();
      final parts = string.split(':');
      if (parts.length >= 2) {
        final hour = int.tryParse(parts[0]) ?? 0;
        final minute = int.tryParse(parts[1]) ?? 0;
        return TimeOfDay(hour: hour, minute: minute);
      }
      return const TimeOfDay(hour: 0, minute: 0);
    }

    int _parseType(dynamic value) {
      if (value == null) return 0;
      if (value is int) return value;
      return int.tryParse(value.toString()) ?? 0;
    }

    return ClassroomActivity(
      id: json['id']?.toString() ?? json['Id']?.toString() ?? '',
      title: json['title'] as String? ?? json['Title'] as String? ?? '',
      description: json['description'] as String? ?? json['Description'] as String? ?? '',
      activityDate: DateTime.tryParse(
            json['activityDate']?.toString() ?? json['ActivityDate']?.toString() ?? '',
          ) ??
          DateTime.fromMillisecondsSinceEpoch(0),
      startTime: _parseTime(json['startTime'] ?? json['StartTime']),
      endTime: _parseTime(json['endTime'] ?? json['EndTime']),
      type: _parseType(json['type'] ?? json['Type']),
      completed: json['completedAt'] != null || json['CompletedAt'] != null,
    );
  }

  String get formattedTime {
    String twoDigits(int value) => value.toString().padLeft(2, '0');
    final start = '${twoDigits(startTime.hour)}:${twoDigits(startTime.minute)}';
    final end = '${twoDigits(endTime.hour)}:${twoDigits(endTime.minute)}';
    return '$start - $end';
  }
}
