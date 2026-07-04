import 'dart:convert';

import 'package:http/http.dart' as http;

import '../../../../core/tenant/school_id.dart';
import '../../domain/entities/login_credentials.dart';
import '../../domain/entities/login_result.dart';
import '../../domain/entities/student.dart';
import '../../domain/repositories/auth_repository.dart';

// AuthRepository'nin gerçek uygulaması.
// Şu anda SchoolInfo API'sine HTTP POST isteği gönderir.
class AuthRepositoryImpl implements AuthRepository {
  // Sunucu IP adresi üzerinden internet erişimi için kullanılır.
  static const String _baseUrl = 'http://85.235.74.24:53079';
  static const String _loginPath = '/api/auth/login';

  @override
  Future<LoginResult> login(LoginCredentials credentials) async {
    final uri = Uri.parse('$_baseUrl$_loginPath');
    final response = await http.post(
      uri,
      headers: {'Content-Type': 'application/json'},
      body: jsonEncode({
        'email': credentials.email,
        'password': credentials.password,
      }),
    );

    if (response.statusCode != 200) {
      throw Exception(
        'Giriş başarısız. Sunucu durumu: ${response.statusCode}.',
      );
    }

    final Map<String, dynamic> body =
        jsonDecode(response.body) as Map<String, dynamic>;
    // Backend dönen JSON'da 'token' veya 'Token' olabilir.
    final token = (body['token'] ?? body['Token']) as String?;
    if (token == null || token.isEmpty) {
      throw Exception('Sunucudan geçerli bir token alınamadı.');
    }

    final claims = _decodeJwtClaims(token);
    final schoolIdValue =
        claims['SchoolId'] as String? ??
        claims['schoolid'] as String? ??
        claims['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/schoolid']
            as String?;

    if (schoolIdValue == null || schoolIdValue.isEmpty) {
      throw Exception('Sunucudan okul kimliği bilgisi alınamadı.');
    }

    final email =
        claims['email'] as String? ??
        claims['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress']
            as String? ??
        credentials.email;

    // Kullanıcı profili almak için /api/users/me çağrısı
    final profileResp = await http.get(
      Uri.parse('$_baseUrl/api/users/me'),
      headers: {
        'Content-Type': 'application/json',
        'Authorization': 'Bearer $token',
      },
    );

    if (profileResp.statusCode != 200) {
      // Profil alınamazsa en azından token ve email ile dönelim
      return LoginResult(
        userName: email,
        schoolId: SchoolId(schoolIdValue),
        firstName: '',
        lastName: '',
        role: '',
        token: token,
      );
    }

    final Map<String, dynamic> profile =
        jsonDecode(profileResp.body) as Map<String, dynamic>;
    final firstName =
        profile['firstName'] as String? ?? profile['firstName'] ?? '';
    final lastName =
        profile['lastName'] as String? ?? profile['lastName'] ?? '';
    final role = profile['role'] as String? ?? profile['role'] ?? '';

    // Eğer kullanıcı veli ise çocuklarını da alalım
    List<Student> students = [];
    if (role.toLowerCase() == 'parent') {
      final childrenResp = await http.get(
        Uri.parse('$_baseUrl/api/students/my-children'),
        headers: {
          'Content-Type': 'application/json',
          'Authorization': 'Bearer $token',
        },
      );

      if (childrenResp.statusCode == 200) {
        final List<dynamic> arr =
            jsonDecode(childrenResp.body) as List<dynamic>;
        students = arr
            .map((e) => Student.fromJson(e as Map<String, dynamic>))
            .toList();
      }
    } else if (role.toLowerCase() == 'teacher') {
      // Öğretmense önce atandığı sınıfları alalım, sonra ilk sınıfın öğrencilerini çekelim
      final classResp = await http.get(
        Uri.parse('$_baseUrl/api/classrooms/teacher/my'),
        headers: {
          'Content-Type': 'application/json',
          'Authorization': 'Bearer $token',
        },
      );

      if (classResp.statusCode == 200) {
        final List<dynamic> classes =
            jsonDecode(classResp.body) as List<dynamic>;
        if (classes.isNotEmpty) {
          final firstClass = classes.first as Map<String, dynamic>;
          final classId =
              firstClass['id']?.toString() ?? firstClass['Id']?.toString();
          if (classId != null && classId.isNotEmpty) {
            final studentsResp = await http.get(
              Uri.parse('$_baseUrl/api/classrooms/$classId/students'),
              headers: {
                'Content-Type': 'application/json',
                'Authorization': 'Bearer $token',
              },
            );

            if (studentsResp.statusCode == 200) {
              final List<dynamic> arr =
                  jsonDecode(studentsResp.body) as List<dynamic>;
              students = arr
                  .map((e) => Student.fromJson(e as Map<String, dynamic>))
                  .toList();
            }
          }
        }
      }
    }

    // Her öğrenci için bugünkü günlük kayıt ve yapay zeka özetini çekelim (varsa)
    for (var i = 0; i < students.length; i++) {
      final s = students[i];

      try {
        final drResp = await http.get(
          Uri.parse('$_baseUrl/api/daily-records/student/${s.id}/today'),
          headers: {
            'Content-Type': 'application/json',
            'Authorization': 'Bearer $token',
          },
        );

        String? drSummary;
        if (drResp.statusCode == 200) {
          final Map<String, dynamic> dr = jsonDecode(drResp.body) as Map<String, dynamic>;
          final sleep = dr['sleepStatus'] as String? ?? dr['SleepStatus'] as String? ?? '';
          final water = dr['waterIntake']?.toString() ?? dr['WaterIntake']?.toString() ?? '';
          final note = dr['teacherNote'] as String? ?? dr['TeacherNote'] as String? ?? '';
          drSummary = 'Uyku: $sleep • Su: ${water}ml' + (note.isNotEmpty ? ' • Not: $note' : '');
        }

        String? aiSummary;
        try {
          final aiResp = await http.get(
            Uri.parse('$_baseUrl/api/summary/student/${s.id}/today'),
            headers: {
              'Content-Type': 'application/json',
              'Authorization': 'Bearer $token',
            },
          );
          if (aiResp.statusCode == 200) {
            final Map<String, dynamic> a = jsonDecode(aiResp.body) as Map<String, dynamic>;
            aiSummary = a['content'] as String? ?? a['Content'] as String?;
          }
        } catch (_) {}

        students[i] = s.copyWith(dailyRecordSummary: drSummary, aiSummary: aiSummary);
      } catch (_) {
        // ignore and continue
      }
    }

    return LoginResult(
      userName: email,
      schoolId: SchoolId(schoolIdValue),
      firstName: firstName,
      lastName: lastName,
      role: role,
      token: token,
      students: students,
    );
  }

  static Map<String, dynamic> _decodeJwtClaims(String token) {
    final parts = token.split('.');
    if (parts.length != 3) {
      throw Exception('Geçersiz JWT formatı.');
    }

    final normalizedPayload = base64Url.normalize(parts[1]);
    final payloadBytes = base64Url.decode(normalizedPayload);
    final payloadString = utf8.decode(payloadBytes);
    final payloadMap = jsonDecode(payloadString);
    if (payloadMap is! Map<String, dynamic>) {
      throw Exception('JWT yükü çözümlenirken beklenmeyen veri alındı.');
    }

    return payloadMap;
  }
}
