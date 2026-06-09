import '../../../../core/usecases/usecase.dart';
import '../entities/login_credentials.dart';
import '../entities/login_result.dart';
import '../repositories/auth_repository.dart';

// Giriş işlemini gerçekleştiren use case.
// UI katmanı bu sınıfı kullanır; iş mantığı repository üzerinden soyutlanır.
class LoginUseCase implements UseCase<LoginResult, LoginCredentials> {
  final AuthRepository repository;

  LoginUseCase(this.repository);

  @override
  Future<LoginResult> call(LoginCredentials credentials) {
    // Kullanıcının verdiği kimlik bilgileri ile repository'den login sonucu al.
    return repository.login(credentials);
  }
}
