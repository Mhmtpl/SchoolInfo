import 'package:flutter/foundation.dart';
import '../../../../core/tenant/school_id.dart';

// Ana sayfa ekranında gösterilecek özet verilerini tutar.
@immutable
class HomeSummary {
  final SchoolId schoolId;
  final String childName;
  final String classroom;
  final String mealStatus;
  final String sleepStatus;
  final String activityStatus;
  final DateTime updatedAt;

  const HomeSummary({
    required this.schoolId,
    required this.childName,
    required this.classroom,
    required this.mealStatus,
    required this.sleepStatus,
    required this.activityStatus,
    required this.updatedAt,
  });
}
