import 'school_id.dart';

abstract class CurrentTenant {
  SchoolId get schoolId;
}

class CurrentTenantImpl implements CurrentTenant {
  @override
  final SchoolId schoolId;

  const CurrentTenantImpl(this.schoolId);
}
