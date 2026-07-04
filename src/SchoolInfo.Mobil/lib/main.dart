import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'features/auth/presentation/pages/login_screen.dart';
import 'theme/app_theme.dart';
import 'theme/theme_provider.dart';

// Uygulamanın başlangıç noktası.
// runApp ile Flutter uygulamasını başlatır ve ana widget'ı ekrana getirir.
void main() {
  runApp(const ProviderScope(child: MiniAdimlarApp()));
}

class MiniAdimlarApp extends ConsumerWidget {
  const MiniAdimlarApp({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final themeMode = ref.watch(appThemeModeProvider);

    return MaterialApp(
      debugShowCheckedModeBanner: false,
      title: 'Mini Adımlar Portal',
      // Uygulamanın genel renk, font ve tasarım şeması
      theme: lightTheme,
      darkTheme: darkTheme,
      themeMode: themeMode,

      // Uygulama açıldığında ilk gösterilecek ekran
      home: const LoginScreen(),
    );
  }
}
