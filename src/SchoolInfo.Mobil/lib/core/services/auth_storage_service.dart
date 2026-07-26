import 'dart:convert';
import 'package:http/http.dart' as http;
import 'package:shared_preferences/shared_preferences.dart';
import '../../features/auth/domain/entities/login_result.dart';
import '../tenant/school_id.dart';

class AuthStorageService {
  static SharedPreferences? _prefs;
  static LoginResult? _currentLoginResult;
  static const String _baseUrl = 'https://api.veliport.com.tr';

  static Future<void> init() async {
    _prefs = await SharedPreferences.getInstance();
    final jsonStr = _prefs?.getString('login_result');
    if (jsonStr != null) {
      try {
        _currentLoginResult = LoginResult.fromJson(jsonDecode(jsonStr));
      } catch (_) {
        // ignore
      }
    }
  }

  static String? get currentToken => _currentLoginResult?.token;
  static String? get currentRefreshToken => _currentLoginResult?.refreshToken;
  static LoginResult? get currentLoginResult => _currentLoginResult;

  static Future<void> saveLoginResult(LoginResult result) async {
    _currentLoginResult = result;
    final jsonStr = jsonEncode(result.toJson());
    await _prefs?.setString('login_result', jsonStr);
  }

  static Future<void> clear() async {
    _currentLoginResult = null;
    await _prefs?.remove('login_result');
  }

  // Token'ın geçerliliğini (exp claim'ine bakarak) kontrol eder.
  static bool isTokenExpired(String token) {
    try {
      final parts = token.split('.');
      if (parts.length != 3) return true;

      final normalizedPayload = base64Url.normalize(parts[1]);
      final payloadBytes = base64Url.decode(normalizedPayload);
      final payloadString = utf8.decode(payloadBytes);
      final claims = jsonDecode(payloadString) as Map<String, dynamic>;

      final exp = claims['exp'] as int?;
      if (exp == null) return true;

      final expiryDate = DateTime.fromMillisecondsSinceEpoch(exp * 1000, isUtc: true);
      // Süresi bitmişse veya bitmesine 15 dakikadan az kalmışsa expired kabul et
      return expiryDate.isBefore(DateTime.now().toUtc().add(const Duration(minutes: 15)));
    } catch (_) {
      return true;
    }
  }

  // Refresh token kullanarak oturumu günceller.
  static Future<bool> refreshSession() async {
    final refreshToken = currentRefreshToken;
    if (refreshToken == null || refreshToken.isEmpty) return false;

    try {
      final response = await http.post(
        Uri.parse('$_baseUrl/api/auth/refresh'),
        headers: {'Content-Type': 'application/json'},
        body: jsonEncode({'refreshToken': refreshToken}),
      );

      if (response.statusCode != 200) {
        // Refresh token geçersiz ise oturumu sıfırla
        await clear();
        return false;
      }

      final Map<String, dynamic> body = jsonDecode(response.body) as Map<String, dynamic>;
      final newToken = body['token'] as String?;
      final newRefreshToken = body['refreshToken'] as String?;

      if (newToken == null || newToken.isEmpty || _currentLoginResult == null) {
        return false;
      }

      // Mevcut login sonucunu yeni tokenlar ile güncelle ve kaydet
      final updatedResult = _currentLoginResult!.copyWith(
        token: newToken,
        refreshToken: newRefreshToken,
      );
      await saveLoginResult(updatedResult);
      return true;
    } catch (_) {
      return false;
    }
  }
}
