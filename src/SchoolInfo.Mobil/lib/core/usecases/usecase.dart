// Tüm use case sınıflarının ortak arayüzü.
// Use case, uygulamanın iş mantığını kapsayan bağımsız bir işlem birimidir.
abstract class UseCase<Type, Params> {
  // Her use case bu metodu uygular ve dışarıdan gelen parametre ile çalışır.
  Future<Type> call(Params params);
}

// Parametre gerektirmeyen use case'ler için boş bir sınıf.
class NoParams {}
