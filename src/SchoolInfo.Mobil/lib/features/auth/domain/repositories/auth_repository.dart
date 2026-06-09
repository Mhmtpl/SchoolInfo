import '../entities/login_credentials.dart';
import '../entities/login_result.dart';

// Auth veri kaynağı için soyut arayüz.
// Use case, bu arayüzü kullanarak gerçek veri kaynağından bağımsız kalır.
abstract class AuthRepository {
  // Kullanıcının verdiği kimlik bilgileri ile giriş yapar.
  Future<LoginResult> login(LoginCredentials credentials);
}
