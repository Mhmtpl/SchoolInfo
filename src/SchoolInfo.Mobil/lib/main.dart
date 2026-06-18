import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:google_fonts/google_fonts.dart';
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
        scaffoldBackgroundColor: const Color(0xFFF8FAFC), // Ultra-modern soft slate-white background
        colorScheme: ColorScheme.fromSeed(
          seedColor: const Color(0xFF4F46E5), // Ana Renk: Royal Indigo (Modern & Timeless)
          primary: const Color(0xFF4F46E5),
          secondary: const Color(0xFFF43F5E), // Yardımcı Renk: Rose (Soft & Child-centric)
          tertiary: const Color(0xFF0D9488), // Detay Rengi: Teal (Fresh & Clean)
          surface: Colors.white,
          onPrimary: Colors.white,
        ),
        textTheme: GoogleFonts.outfitTextTheme(Theme.of(context).textTheme),
        appBarTheme: const AppBarTheme(
          backgroundColor: Colors.transparent,
          elevation: 0,
          centerTitle: true,
          titleTextStyle: TextStyle(
            color: Color(0xFF0F172A),
            fontSize: 20,
            fontWeight: FontWeight.bold,
          ),
          iconTheme: IconThemeData(color: Color(0xFF0F172A)),
        ),
      ),

      // Uygulama açıldığında ilk gösterilecek ekran
      home: const LoginScreen(),
    );
  }
}
