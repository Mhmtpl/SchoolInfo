import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';

class AppColors {
  static const primary = Color(0xFF06B6D4); // neon blue for light mode
  static const secondary = Color(0xFF8B5CF6); // neon purple for light mode

  static const lightBackground = Color(0xFFF1F5F9);
  static const lightSurface = Color(0xFFFFFFFF);
  static const lightText = Color(0xFF0F172A);
  static const lightMuted = Color(0xFF64748B);

  static const darkBackground = Color(0xFF0F172A);
  static const darkSurface = Color(0xFF1E293B);
  static const darkDetail = Color(0xFF22D3EE);
  static const darkMuted = Color(0xFF94A3B8);

  static const success = Color(0xFF16A34A);
  static const error = Color(0xFFEF4444);
}

final ThemeData lightTheme = ThemeData(
  useMaterial3: true,
  brightness: Brightness.light,
  scaffoldBackgroundColor: AppColors.lightBackground,
  colorScheme: const ColorScheme(
    brightness: Brightness.light,
    primary: AppColors.primary,
    onPrimary: Colors.white,
    secondary: AppColors.secondary,
    onSecondary: Colors.white,
    error: AppColors.error,
    onError: Colors.white,
    background: AppColors.lightBackground,
    onBackground: AppColors.lightText,
    surface: AppColors.lightSurface,
    onSurface: AppColors.lightText,
  ),
  textTheme: GoogleFonts.outfitTextTheme().copyWith(
    titleLarge: const TextStyle(color: AppColors.lightText, fontWeight: FontWeight.w700),
    bodyMedium: const TextStyle(color: AppColors.lightText),
    bodySmall: const TextStyle(color: AppColors.lightMuted),
  ),
  appBarTheme: AppBarTheme(
    backgroundColor: AppColors.lightSurface,
    elevation: 0,
    centerTitle: true,
    titleTextStyle: const TextStyle(
      color: AppColors.lightText,
      fontSize: 20,
      fontWeight: FontWeight.bold,
    ),
    iconTheme: const IconThemeData(color: AppColors.lightText),
  ),
  cardTheme: CardThemeData(
    color: AppColors.lightSurface,
    elevation: 12,
    shadowColor: Colors.black.withOpacity(0.08),
    shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
    margin: const EdgeInsets.symmetric(vertical: 12, horizontal: 14),
  ),
  elevatedButtonTheme: ElevatedButtonThemeData(
    style: ElevatedButton.styleFrom(
      backgroundColor: AppColors.primary,
      foregroundColor: Colors.white,
      elevation: 8,
      padding: const EdgeInsets.symmetric(vertical: 14, horizontal: 20),
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
      textStyle: const TextStyle(fontWeight: FontWeight.w700),
    ),
  ),
  iconTheme: const IconThemeData(color: AppColors.lightMuted),
  bottomNavigationBarTheme: BottomNavigationBarThemeData(
    backgroundColor: AppColors.lightSurface,
    elevation: 12,
    selectedItemColor: AppColors.primary,
    unselectedItemColor: AppColors.lightMuted,
  ),
);

final ThemeData darkTheme = ThemeData(
  useMaterial3: true,
  brightness: Brightness.dark,
  scaffoldBackgroundColor: AppColors.darkBackground,
  colorScheme: const ColorScheme(
    brightness: Brightness.dark,
    primary: AppColors.darkDetail,
    onPrimary: Colors.black,
    secondary: AppColors.darkDetail,
    onSecondary: Colors.black,
    error: AppColors.error,
    onError: Colors.white,
    background: AppColors.darkBackground,
    onBackground: Colors.white,
    surface: AppColors.darkSurface,
    onSurface: Colors.white,
  ),
  textTheme: GoogleFonts.outfitTextTheme(ThemeData(brightness: Brightness.dark).textTheme).copyWith(
    titleLarge: const TextStyle(color: Colors.white, fontWeight: FontWeight.w700),
    bodyMedium: const TextStyle(color: Colors.white),
    bodySmall: const TextStyle(color: AppColors.darkMuted),
  ),
  appBarTheme: const AppBarTheme(
    backgroundColor: AppColors.darkSurface,
    elevation: 0,
    centerTitle: true,
    titleTextStyle: TextStyle(
      color: Colors.white,
      fontSize: 20,
      fontWeight: FontWeight.bold,
    ),
    iconTheme: IconThemeData(color: Colors.white),
  ),
  cardTheme: CardThemeData(
    color: AppColors.darkSurface,
    elevation: 12,
    shadowColor: Colors.black.withOpacity(0.6),
    shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
    margin: const EdgeInsets.symmetric(vertical: 12, horizontal: 14),
  ),
  elevatedButtonTheme: ElevatedButtonThemeData(
    style: ElevatedButton.styleFrom(
      backgroundColor: AppColors.primary,
      foregroundColor: Colors.white,
      elevation: 8,
      padding: const EdgeInsets.symmetric(vertical: 14, horizontal: 20),
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
      textStyle: const TextStyle(fontWeight: FontWeight.w700),
    ),
  ),
  iconTheme: const IconThemeData(color: Colors.white70),
  bottomNavigationBarTheme: BottomNavigationBarThemeData(
    backgroundColor: AppColors.darkSurface,
    elevation: 12,
    selectedItemColor: AppColors.primary,
    unselectedItemColor: AppColors.darkMuted,
  ),
);

// Convenience getter: call `themeMode` with system preference in main.dart
