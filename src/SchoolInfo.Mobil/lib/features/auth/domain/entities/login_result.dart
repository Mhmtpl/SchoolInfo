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
  final String? refreshToken;
  final List<Student> students;

  const LoginResult({
    required this.userName,
    required this.schoolId,
    required this.firstName,
    required this.lastName,
    required this.role,
    required this.token,
    this.refreshToken,
    this.students = const [],
  });

  factory LoginResult.fromJson(Map<String, dynamic> json) => LoginResult(
    userName: json['userName'] as String? ?? '',
    schoolId: SchoolId(json['schoolId']?.toString() ?? ''),
    firstName: json['firstName'] as String? ?? '',
    lastName: json['lastName'] as String? ?? '',
    role: json['role'] as String? ?? '',
    token: json['token'] as String? ?? '',
    refreshToken: json['refreshToken'] as String?,
    students: (json['students'] as List<dynamic>?)
            ?.map((e) => Student.fromJson(e as Map<String, dynamic>))
            .toList() ??
        const [],
  );

  Map<String, dynamic> toJson() => {
    'userName': userName,
    'schoolId': schoolId.value,
    'firstName': firstName,
    'lastName': lastName,
    'role': role,
    'token': token,
    'refreshToken': refreshToken,
    'students': students.map((e) => e.toJson()).toList(),
  };

  LoginResult copyWith({
    String? userName,
    SchoolId? schoolId,
    String? firstName,
    String? lastName,
    String? role,
    String? token,
    String? refreshToken,
    List<Student>? students,
  }) {
    return LoginResult(
      userName: userName ?? this.userName,
      schoolId: schoolId ?? this.schoolId,
      firstName: firstName ?? this.firstName,
      lastName: lastName ?? this.lastName,
      role: role ?? this.role,
      token: token ?? this.token,
      refreshToken: refreshToken ?? this.refreshToken,
      students: students ?? this.students,
    );
  }
}
