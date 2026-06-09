import '../../domain/entities/home_summary.dart';
import '../../domain/repositories/home_repository.dart';
import '../models/home_summary_model.dart';
import '../../../../core/tenant/school_id.dart';

// HomeRepository'nin gerçek uygulaması.
// Şu anda demo amaçlı, sabit/mock veri döndürüyor.
class HomeRepositoryImpl implements HomeRepository {
  @override
  Future<HomeSummary> getHomeSummary({required SchoolId schoolId}) async {
    // Burada ileride gerçek bir API / veri kaynağı bağlanacak.
    return HomeSummaryModel.mock(schoolId: schoolId.value);
  }
}
