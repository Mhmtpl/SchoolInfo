import 'package:flutter/foundation.dart';

// Okul / tenant kimliğini temsil eden küçük bir değer nesnesi.
// Bu sınıf uygulama içinde "hangi okula ait olduğu" bilgisini taşımak için kullanılır.
@immutable
class SchoolId {
  // Okul kimliği değeri. Örneğin "tenant-001" gibi bir string.
  final String value;

  // Tek alanlı const yapıcı. Okul kimliği oluşturmak için kullanılır.
  const SchoolId(this.value);

  // Kimlik değeri boş değilse true döner.
  // Bu, kimliğin geçerli olup olmadığını kontrol etmek için faydalıdır.
  bool get isNotEmpty => value.isNotEmpty;

  // SchoolId nesnesini yazdırırken içindeki string değerin gösterilmesini sağlar.
  @override
  String toString() => value;
}
