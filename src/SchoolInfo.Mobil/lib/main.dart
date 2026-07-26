import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'core/services/auth_storage_service.dart';
import 'features/auth/presentation/pages/login_screen.dart';
import 'features/home/presentation/pages/home_screen.dart';
import 'features/teacher/presentation/pages/teacher_class_selection_screen.dart';
import 'features/teacher/presentation/providers/teacher_providers.dart';
import 'theme/app_theme.dart';
import 'theme/theme_provider.dart';

// Uygulamanın başlangıç noktası.
void main() async {
  WidgetsFlutterBinding.ensureInitialized();
  await AuthStorageService.init();

  Widget initialScreen = const LoginScreen();
  final loginResult = AuthStorageService.currentLoginResult;

  if (loginResult != null) {
    final isExpired = AuthStorageService.isTokenExpired(loginResult.token);
    bool refreshSuccess = false;

    if (isExpired) {
      refreshSuccess = await AuthStorageService.refreshSession();
    } else {
      refreshSuccess = true;
    }

    if (refreshSuccess) {
      final updatedResult = AuthStorageService.currentLoginResult!;
      if (updatedResult.role.toLowerCase() == 'teacher') {
        initialScreen = const TeacherClassSelectionScreen();
      } else {
        initialScreen = HomeScreen(
          schoolId: updatedResult.schoolId,
          token: updatedResult.token,
          userFirstName: updatedResult.firstName,
          userLastName: updatedResult.lastName,
          userEmail: updatedResult.userName,
          students: updatedResult.students,
        );
      }
    }
  }

  runApp(ProviderScope(
    child: MiniAdimlarApp(initialScreen: initialScreen),
  ));
}

class MiniAdimlarApp extends ConsumerWidget {
  final Widget? initialScreen;
  const MiniAdimlarApp({super.key, this.initialScreen});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final themeMode = ref.watch(appThemeModeProvider);

    // Eğer başlangıçta öğretmen olarak giriş yapılmışsa, sağlayıcı durumunu ayarla
    final loginResult = AuthStorageService.currentLoginResult;
    if (loginResult != null && loginResult.role.toLowerCase() == 'teacher') {
      Future.microtask(() {
        if (ref.read(currentTeacherProvider) == null) {
          ref.read(currentTeacherProvider.notifier).state = loginResult;
        }
      });
    }

    return MaterialApp(
      debugShowCheckedModeBanner: false,
      title: 'Veliportal',
      theme: lightTheme,
      darkTheme: darkTheme,
      themeMode: themeMode,
      home: initialScreen ?? const LoginScreen(),
    );
  }
}
