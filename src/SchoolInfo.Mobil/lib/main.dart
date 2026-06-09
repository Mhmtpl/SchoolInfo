import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'features/auth/presentation/pages/login_screen.dart';

// Uygulamanın başlangıç noktası.
// runApp ile Flutter uygulamasını başlatır ve ana widget'ı ekrana getirir.
void main() {
  runApp(const ProviderScope(child: MiniAdimlarApp()));
}

class MiniAdimlarApp extends StatelessWidget {
  const MiniAdimlarApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      debugShowCheckedModeBanner: false,
      title: 'Mini Adımlar Portal',

      // Uygulamanın genel renk, font ve tasarım şeması
      theme: ThemeData(
        useMaterial3: true,
        scaffoldBackgroundColor: const Color(
          0xFFF7F9FC,
        ), // Yumuşak gri-mavi arka plan
        colorScheme: ColorScheme.fromSeed(
          seedColor: const Color(0xFF6C5CE7), // Ana Renk: Tatlı Mor
          primary: const Color(0xFF6C5CE7),
          secondary: const Color(0xFFFF7675), // Yardımcı Renk: Pastel Mercan
          tertiary: const Color(0xFF00CEC9), // Detay Rengi: Mint Yeşili
        ),
      ),

      // Uygulama açıldığında ilk gösterilecek ekran
      home: const LoginScreen(),
    );
  }
}
