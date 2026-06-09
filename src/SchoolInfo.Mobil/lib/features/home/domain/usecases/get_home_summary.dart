import '../../../../core/tenant/school_id.dart';
import '../../../../core/usecases/usecase.dart';
import '../entities/home_summary.dart';
import '../repositories/home_repository.dart';

// Ana sayfa özetini getiren use case.
// Bu sınıf, mevcut okul kimliğine göre HomeRepository'den veri alır.
class GetHomeSummary implements UseCase<HomeSummary, SchoolId> {
  final HomeRepository repository;

  GetHomeSummary(this.repository);

  @override
  Future<HomeSummary> call(SchoolId schoolId) {
    return repository.getHomeSummary(schoolId: schoolId);
  }
}
