import 'package:flutter/foundation.dart';

// Kullanıcının giriş yaparken verdiği e-posta ve parola bilgilerini taşır.
@immutable
class LoginCredentials {
  final String email;
  final String password;

  const LoginCredentials({
    required this.email,
    required this.password,
  });
}
