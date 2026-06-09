import '../../domain/entities/home_summary.dart';
import '../../../../core/tenant/school_id.dart';

class HomeSummaryModel extends HomeSummary {
  const HomeSummaryModel({
    required super.schoolId,
    required super.childName,
    required super.classroom,
    required super.mealStatus,
    required super.sleepStatus,
    required super.activityStatus,
    required super.updatedAt,
  });

  factory HomeSummaryModel.mock({required String schoolId}) {
    return HomeSummaryModel(
      schoolId: SchoolId(schoolId),
      childName: 'Mert',
      classroom: 'Neşeli Bulutlar',
      mealStatus: 'Yemek tamamlandı',
      sleepStatus: 'Uyku düzenli',
      activityStatus: 'Oyun zamanı',
      updatedAt: DateTime.now(),
    );
  }
}
