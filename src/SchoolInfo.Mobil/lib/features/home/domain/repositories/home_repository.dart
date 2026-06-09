import '../entities/home_summary.dart';
import '../../../../core/tenant/school_id.dart';

// Home ekranı için özet veriyi sağlayan repository arayüzü.
abstract class HomeRepository {
  Future<HomeSummary> getHomeSummary({required SchoolId schoolId});
}
