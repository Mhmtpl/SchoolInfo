import 'package:flutter/foundation.dart';

@immutable
class ClassroomSummary {
  final String id;
  final String name;
  final int studentCount;
  final int reportSent;

  const ClassroomSummary({
    required this.id,
    required this.name,
    required this.studentCount,
    required this.reportSent,
  });
}
