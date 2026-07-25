import 'dart:convert';
import 'package:web_socket_channel/web_socket_channel.dart';

/// SignalR Hub protokolünü saf WebSocket üzerinden uygulayan hafif ve kararlı servis.
/// Windows dosya yolu ve symlink hatalarına açık olan 3. parti paketlerin yerine kullanılır.
class BiometricSignalRService {
  final String hubUrl; // Örn: https://api.veliport.com.tr/hubs/biometrics
  final String token;
  final String studentId;
  final Function(Map<String, dynamic>) onUpdateReceived;
  final Function(String) onStatusChanged;

  WebSocketChannel? _channel;
  bool _isClosed = false;

  BiometricSignalRService({
    required this.hubUrl,
    required this.token,
    required this.studentId,
    required this.onUpdateReceived,
    required this.onStatusChanged,
  });

  Future<void> connect() async {
    _isClosed = false;
    onStatusChanged("Bağlanıyor...");

    // Http adresini WebSocket adresine dönüştür
    String wsUrl = hubUrl
        .replaceAll('https://', 'wss://')
        .replaceAll('http://', 'ws://');
    
    // Kimlik doğrulama token'ını query parametresi olarak ekle (SignalR Hub standardı)
    wsUrl = '$wsUrl?access_token=$token';

    try {
      _channel = WebSocketChannel.connect(Uri.parse(wsUrl));
      onStatusChanged("Bağlandı");

      // 1. Handshake (El Sıkışma) mesajı gönderilir (ASCII 30 ile sonlandırılır)
      final handshake = jsonEncode({"protocol": "json", "version": 1}) + "\x1e";
      _channel!.sink.add(handshake);

      // 2. Gruba katılım mesajı (JoinStudentGroup)
      final joinGroup = jsonEncode({
        "type": 1,
        "target": "JoinStudentGroup",
        "arguments": [studentId]
      }) + "\x1e";
      _channel!.sink.add(joinGroup);

      // Stream dinlemeye başla
      _channel!.stream.listen(
        (data) {
          _handleMessage(data.toString());
        },
        onError: (err) {
          if (!_isClosed) {
            onStatusChanged("Bağlantı Hatası");
            _reconnect();
          }
        },
        onDone: () {
          if (!_isClosed) {
            onStatusChanged("Bağlantı Kesildi");
            _reconnect();
          }
        },
      );
    } catch (e) {
      if (!_isClosed) {
        onStatusChanged("Bağlantı Hatası");
        _reconnect();
      }
    }
  }

  void _handleMessage(String rawMessage) {
    // SignalR mesajları ASCII 30 (Record Separator) karakteri ile ayrılır
    final parts = rawMessage.split('\x1e');
    for (var part in parts) {
      if (part.trim().isEmpty) continue;
      try {
        final message = jsonDecode(part) as Map<String, dynamic>;
        
        // type 1: Invocation (Sunucunun istemcide metot tetiklemesi)
        if (message['type'] == 1 && message['target'] == 'ReceiveBiometricUpdate') {
          final args = message['arguments'] as List<dynamic>?;
          if (args != null && args.isNotEmpty) {
            final update = args[0] as Map<String, dynamic>;
            onUpdateReceived(update);
          }
        }
      } catch (_) {
        // Hatalı veya el sıkışma dışı mesajları yoksay
      }
    }
  }

  Future<void> _reconnect() async {
    await Future.delayed(const Duration(seconds: 5));
    if (!_isClosed) {
      connect();
    }
  }

  Future<void> disconnect() async {
    _isClosed = true;
    if (_channel != null) {
      try {
        await _channel!.sink.close();
      } catch (_) {}
      _channel = null;
    }
    onStatusChanged("Bağlantı Kesildi");
  }
}
