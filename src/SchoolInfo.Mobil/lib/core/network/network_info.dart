abstract class NetworkInfo {
  Future<bool> get isConnected;
}

class NetworkInfoImpl implements NetworkInfo {
  @override
  Future<bool> get isConnected async {
    // Burada bir kablosuz bağlantı kontrolü eklenebilir.
    return true;
  }
}
