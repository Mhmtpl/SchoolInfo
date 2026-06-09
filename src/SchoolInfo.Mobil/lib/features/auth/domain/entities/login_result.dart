import 'package:flutter/foundation.dart';
import '../../../../core/tenant/school_id.dart';
import 'student.dart';

// Giriş başarılı olduğunda dönen sonuç.
// Artık kullanıcı adı/okulun yanında ad/soyad, rol ve varsa çocuk(lar) bilgisi içerir.
@immutable
class LoginResult {
  final String userName;
  final SchoolId schoolId;
  final String firstName;
  final String lastName;
  final String role;
  final String token;
  final List<Student> students;

  const LoginResult({
    required this.userName,
    required this.schoolId,
    required this.firstName,
    required this.lastName,
    required this.role,
    required this.token,
    this.students = const [],
  });
}
