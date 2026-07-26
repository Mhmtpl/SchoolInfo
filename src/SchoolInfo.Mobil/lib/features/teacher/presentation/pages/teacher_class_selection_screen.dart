import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../auth/domain/entities/student.dart';
import '../../../../core/services/auth_storage_service.dart';
import '../../../auth/presentation/pages/login_screen.dart';
import '../providers/teacher_providers.dart';
import '../../domain/entities/classroom_activity.dart';
import '../../domain/entities/classroom_daily_record.dart';
import '../../domain/entities/classroom_summary.dart';
import '../../domain/entities/student_meal_record.dart';
import '../../domain/entities/weekly_meal_plan.dart';
import 'package:speech_to_text/speech_to_text.dart' as stt;
import '../../../../theme/app_theme.dart';
import '../../../../theme/theme_provider.dart';
import '../../../../theme/theme_provider.dart';

class TeacherClassSelectionScreen extends ConsumerWidget {
  const TeacherClassSelectionScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final classroomAsync = ref.watch(teacherClassroomListProvider);
    final theme = Theme.of(context);

    return Scaffold(
      backgroundColor: theme.colorScheme.surface,
      appBar: AppBar(
        backgroundColor: theme.colorScheme.surface,
        elevation: 0,
        centerTitle: true,
        title: const Text('Öğretmen Paneli'),
        foregroundColor: theme.colorScheme.onSurface,
        actions: [
          IconButton(
            icon: Icon(theme.brightness == Brightness.dark ? Icons.wb_sunny_outlined : Icons.nights_stay_outlined),
            tooltip: 'Tema Değiştir',
            onPressed: () {
              final current = ref.read(appThemeModeProvider);
              ref.read(appThemeModeProvider.notifier).state =
                  current == ThemeMode.dark ? ThemeMode.light : ThemeMode.dark;
            },
          ),
          IconButton(
            icon: const Icon(Icons.logout_rounded),
            tooltip: 'Çıkış Yap',
            onPressed: () => _logout(context),
          ),
        ],
      ),
      body: Container(
        color: theme.colorScheme.surface,
        child: classroomAsync.when(
          data: (classrooms) => _buildClassroomList(context, classrooms),
          loading: () => const Center(child: CircularProgressIndicator()),
          error: (error, stack) => Center(child: Text('Hata: $error')),
        ),
      ),
    );
  }

  void _logout(BuildContext context) async {
    await AuthStorageService.clear();
    Navigator.of(context).pushAndRemoveUntil(
      MaterialPageRoute(builder: (_) => const LoginScreen()),
      (route) => false,
    );
  }

  Widget _buildClassroomList(
    BuildContext context,
    List<ClassroomSummary> classrooms,
  ) {
    final theme = Theme.of(context);
    if (classrooms.isEmpty) {
      return Center(
        child: Text(
          'Atanmış sınıf bulunamadı.',
          style: TextStyle(color: theme.colorScheme.onSurface.withOpacity(0.75), fontSize: 16),
        ),
      );
    }

    final totalStudents = classrooms.fold<int>(0, (sum, item) => sum + item.studentCount);

    return ListView(
      physics: const BouncingScrollPhysics(),
      padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 18),
      children: [
        // Hoş Geldiniz Kartı
        Container(
          padding: const EdgeInsets.all(24),
          decoration: BoxDecoration(
            color: theme.colorScheme.surface,
            borderRadius: BorderRadius.circular(22),
            border: Border.all(color: theme.colorScheme.primary.withOpacity(0.14), width: 1),
            boxShadow: [
              BoxShadow(
                color: theme.brightness == Brightness.dark
                    ? Colors.black.withOpacity(0.30)
                    : Colors.black.withOpacity(0.07),
                blurRadius: 24,
                offset: const Offset(0, 12),
              ),
            ],
          ),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Row(
                children: [
                  Container(
                    padding: const EdgeInsets.all(12),
                    decoration: BoxDecoration(
                      color: theme.colorScheme.primary.withOpacity(0.08),
                      shape: BoxShape.circle,
                    ),
                    child: Icon(
                      Icons.school_rounded,
                      color: theme.colorScheme.primary,
                      size: 28,
                    ),
                  ),
                  const SizedBox(width: 14),
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          'Hoş geldiniz, Öğretmenim! 👋',
                          style: TextStyle(
                            fontSize: 18,
                            fontWeight: FontWeight.w900,
                            color: theme.colorScheme.onSurface,
                            letterSpacing: -0.4,
                          ),
                        ),
                        Text(
                          'Günlük Sınıf Yönetim Paneli',
                          style: TextStyle(
                            fontSize: 11,
                            color: theme.colorScheme.onSurface.withOpacity(0.55),
                            fontWeight: FontWeight.w600,
                          ),
                        ),
                      ],
                    ),
                  ),
                ],
              ),
              const SizedBox(height: 16),
              const SizedBox(height: 8),
              Text(
                'Bugün sınıflarınızı hızlıca yönetebilir, uyku ve haftalık yemek programlarını düzenleyebilirsiniz.',
                style: theme.textTheme.bodyMedium?.copyWith(
                  color: theme.colorScheme.onSurface.withOpacity(0.72),
                  height: 1.55,
                  fontWeight: FontWeight.w500,
                ),
              ),
              const SizedBox(height: 20),
              Row(
                children: [
                  _dashboardMetric(context, 'Sınıf', classrooms.length.toString(), theme.colorScheme.primary),
                  const SizedBox(width: 12),
                  _dashboardMetric(context, 'Öğrenci', totalStudents.toString(), theme.colorScheme.secondary),
                ],
              ),
            ],
          ),
        ),
        const SizedBox(height: 24),
        
        Text(
          'Sınıflarınız',
          style: TextStyle(
            fontSize: 18,
            fontWeight: FontWeight.w800,
            color: theme.colorScheme.onBackground,
          ),
        ),
        const SizedBox(height: 12),
        
        ...classrooms.map((classroom) {
          return Padding(
            padding: const EdgeInsets.only(bottom: 12),
            child: Container(
              decoration: BoxDecoration(
                color: theme.colorScheme.surface,
                borderRadius: BorderRadius.circular(20),
                border: Border.all(color: theme.colorScheme.primary.withOpacity(0.08), width: 1),
                boxShadow: [
                  BoxShadow(
                    color: theme.brightness == Brightness.dark
                        ? Colors.black.withOpacity(0.18)
                        : Colors.black.withOpacity(0.04),
                    blurRadius: 14,
                  ),
                ],
              ),
              child: Material(
                color: Colors.transparent,
                child: InkWell(
                  borderRadius: BorderRadius.circular(20),
                  onTap: () {
                    Navigator.of(context).push(
                      MaterialPageRoute(
                        builder: (_) =>
                            TeacherClassroomDetailScreen(classroom: classroom),
                      ),
                    );
                  },
                  child: Padding(
                    padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 18),
                    child: Row(
                      children: [
                        Container(
                          width: 50,
                          height: 50,
                          decoration: BoxDecoration(
                            color: theme.colorScheme.primary.withOpacity(0.18),
                            shape: BoxShape.circle,
                            boxShadow: [
                              BoxShadow(
                                color: theme.brightness == Brightness.dark
                                    ? theme.colorScheme.primary.withOpacity(0.18)
                                    : theme.colorScheme.primary.withOpacity(0.16),
                                blurRadius: 14,
                                offset: const Offset(0, 8),
                              ),
                            ],
                          ),
                          child: Icon(Icons.school_rounded, color: theme.colorScheme.primary, size: 22),
                        ),
                        const SizedBox(width: 16),
                        Expanded(
                          child: Column(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              Text(
                                classroom.name,
                                style: theme.textTheme.titleMedium?.copyWith(
                                  fontWeight: FontWeight.w800,
                                  color: theme.colorScheme.onBackground,
                                ),
                              ),
                              const SizedBox(height: 4),
                              Text(
                                '${classroom.studentCount} kayıtlı öğrenci',
                                style: theme.textTheme.bodySmall?.copyWith(
                                  color: theme.colorScheme.onSurface.withOpacity(0.72),
                                  fontWeight: FontWeight.w500,
                                ),
                              ),
                            ],
                          ),
                        ),
                        Icon(Icons.chevron_right_rounded, color: Theme.of(context).colorScheme.onSurface.withOpacity(0.6), size: 24),
                      ],
                    ),
                  ),
                ),
              ),
            ),
          );
        }).toList(),
      ],
    );
  }

  Widget _dashboardMetric(BuildContext context, String label, String value, Color color) {
    final theme = Theme.of(context);
    return Expanded(
      child: Container(
        padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 14),
        decoration: BoxDecoration(
          color: color.withOpacity(0.08),
          borderRadius: BorderRadius.circular(16),
        ),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              value,
              style: TextStyle(
                fontSize: 22,
                fontWeight: FontWeight.w800,
                color: color,
                letterSpacing: -0.5,
              ),
            ),
            const SizedBox(height: 4),
            Text(
              label,
              style: theme.textTheme.bodySmall?.copyWith(
                color: theme.colorScheme.onSurface.withOpacity(0.62),
                fontWeight: FontWeight.w600,
              ),
            ),
          ],
        ),
      ),
    );
  }
}

class TeacherClassroomDetailScreen extends ConsumerStatefulWidget {
  final ClassroomSummary classroom;

  const TeacherClassroomDetailScreen({super.key, required this.classroom});

  @override
  ConsumerState<TeacherClassroomDetailScreen> createState() =>
      _TeacherClassroomDetailScreenState();
}

class _TeacherClassroomDetailScreenState
    extends ConsumerState<TeacherClassroomDetailScreen>
    with SingleTickerProviderStateMixin {
  late final TabController _tabController;
  final _createFormKey = GlobalKey<FormState>();
  final _titleController = TextEditingController();
  final _descriptionController = TextEditingController();
  DateTime _selectedDate = DateTime.now();
  TimeOfDay _selectedStartTime = const TimeOfDay(hour: 9, minute: 0);
  TimeOfDay _selectedEndTime = const TimeOfDay(hour: 10, minute: 0);
  int _selectedType = 0;
  String? _errorMessage;
  bool _isSubmitting = false;
  bool _isDailySaving = false;
  bool _isWeeklySaving = false;
  bool _mealEntriesInitialized = false;
  final List<_TeacherMealEntry> _mealEntries = [];
  bool _medicationEntriesInitialized = false;
  final List<_TeacherMedicationEntry> _medicationEntries = [];
  bool _dailyEntriesInitialized = false;
  final List<_TeacherDailyEntry> _dailyEntries = [];
  bool _showDailySaveButton = true;
  late final ScrollController _dailyScrollController;
  bool _weeklyPlanEditMode = false;
  List<WeeklyMealPlan> _weeklyPlanEdits = [];
  int _mealTabSubIndex = 0;

  // Yapay Zeka Güncelleme Değişkenleri
  late final stt.SpeechToText _speech;
  bool _speechInitialized = false;
  bool _isListening = false;
  String _speechError = '';
  final _aiCommandController = TextEditingController();
  DateTime _aiSelectedDate = DateTime.now();
  bool _aiIsSubmitting = false;
  String? _aiSuccessMessage;
  List<String>? _aiUpdatedStudents;

  @override
  void initState() {
    super.initState();
    _tabController = TabController(length: 5, vsync: this);
    _tabController.addListener(() => setState(() {}));
    _dailyScrollController = ScrollController();

    _speech = stt.SpeechToText();
    _initSpeech();
  }

  Future<void> _initSpeech() async {
    try {
      final hasSpeech = await _speech.initialize(
        onError: (val) {
          setState(() {
            _speechError = 'Hata: ${val.errorMsg}';
            _isListening = false;
          });
        },
        onStatus: (val) {
          if (val == 'done' || val == 'notListening') {
            setState(() {
              _isListening = false;
            });
          }
        },
      );
      if (mounted) {
        setState(() {
          _speechInitialized = hasSpeech;
        });
      }
    } catch (e) {
      if (mounted) {
        setState(() {
          _speechError = 'Konuşma tanıma başlatılamadı: $e';
        });
      }
    }
  }

  Future<void> _toggleListening() async {
    if (!_speechInitialized) {
      await _initSpeech();
    }
    if (_isListening) {
      await _speech.stop();
      setState(() {
        _isListening = false;
      });
    } else {
      if (_speechInitialized) {
        setState(() {
          _isListening = true;
          _speechError = '';
        });
        await _speech.listen(
          onResult: (val) {
            setState(() {
              if (val.recognizedWords.isNotEmpty) {
                final currentText = _aiCommandController.text.trim();
                _aiCommandController.text = currentText.isEmpty
                    ? val.recognizedWords
                    : '$currentText ${val.recognizedWords}';
              }
            });
          },
          localeId: 'tr_TR',
        );
      } else {
        setState(() {
          _speechError = 'Mikrofon veya ses tanıma sistemi hazır değil.';
        });
      }
    }
  }

  Future<void> _submitAICommand() async {
    final command = _aiCommandController.text.trim();
    if (command.isEmpty) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Lütfen önce bir komut yazın veya ses kaydı alın.')),
      );
      return;
    }

    setState(() {
      _aiIsSubmitting = true;
      _aiSuccessMessage = null;
      _aiUpdatedStudents = null;
    });

    try {
      final repository = ref.read(teacherRepositoryProvider);
      final dateStr = '${_aiSelectedDate.year}-${_aiSelectedDate.month.toString().padLeft(2, '0')}-${_aiSelectedDate.day.toString().padLeft(2, '0')}';
      
      final result = await repository.aiUpdateClassroom(
        classroomId: widget.classroom.id,
        command: command,
        dateStr: dateStr,
      );

      if (mounted) {
        setState(() {
          _aiSuccessMessage = result.message;
          _aiUpdatedStudents = result.updatedStudents;
          _aiCommandController.clear();
          _dailyEntriesInitialized = false;
        });

        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text(result.message)),
        );

        // Refresh classroom daily and meal records to reflect the AI updates
        ref.refresh(classroomDailyRecordsProvider(widget.classroom.id));
        ref.refresh(classroomMealRecordsProvider(widget.classroom.id));
      }
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('Hata oluştu: $e')),
        );
      }
    } finally {
      if (mounted) {
        setState(() {
          _aiIsSubmitting = false;
        });
      }
    }
  }

  @override
  void dispose() {
    _dailyScrollController.dispose();
    _tabController.dispose();
    _titleController.dispose();
    _descriptionController.dispose();
    _aiCommandController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final studentsAsync = ref.watch(
      classroomStudentsProvider(widget.classroom.id),
    );
    final dailyAsync = ref.watch(
      classroomDailyRecordsProvider(widget.classroom.id),
    );
    final mealsAsync = ref.watch(
      classroomMealRecordsProvider(widget.classroom.id),
    );
    final weeklyPlansAsync = ref.watch(
      classroomWeeklyMealPlansProvider(widget.classroom.id),
    );
    final activitiesAsync = ref.watch(
      classroomActivitiesProvider(widget.classroom.id),
    );

    return Scaffold(
      appBar: AppBar(
        backgroundColor: theme.colorScheme.surface,
        elevation: 0,
        centerTitle: true,
        title: Text(widget.classroom.name),
        foregroundColor: theme.colorScheme.onSurface,
        actions: [
          IconButton(
            icon: Icon(theme.brightness == Brightness.dark ? Icons.wb_sunny_outlined : Icons.nights_stay_outlined),
            tooltip: 'Tema Değiştir',
            onPressed: () {
              final current = ref.read(appThemeModeProvider);
              ref.read(appThemeModeProvider.notifier).state =
                  current == ThemeMode.dark ? ThemeMode.light : ThemeMode.dark;
            },
          ),
          IconButton(
            icon: const Icon(Icons.logout),
            tooltip: 'Çıkış Yap',
            onPressed: _logout,
          ),
        ],
      ),
      backgroundColor: theme.colorScheme.background,
      body: Column(
        children: [
          _buildHeader(),
          Expanded(
            child: TabBarView(
              controller: _tabController,
              children: [
                _buildDailyTab(dailyAsync),
                _buildMealTabCombined(studentsAsync, mealsAsync, weeklyPlansAsync),
                _buildMedicationTab(studentsAsync),
                _buildActivitiesTab(activitiesAsync),
                _buildAIUpdateTab(),
              ],
            ),
          ),
        ],
      ),
      bottomNavigationBar: _buildBottomNavigationBar(context),
      floatingActionButton: _tabController.index == 3
          ? FloatingActionButton.extended(
              onPressed: () => _showCreateActivityDialog(),
              icon: const Icon(Icons.add),
              label: const Text('Yeni Aktivite'),
            )
          : null,
    );
  }

  Widget _buildBottomNavigationBar(BuildContext context) {
    final theme = Theme.of(context);
    final activeColor = theme.colorScheme.primary;
    final inactiveColor = theme.brightness == Brightness.dark
        ? Colors.white70
        : theme.colorScheme.onSurface.withOpacity(0.65);

    return SafeArea(
      top: false,
      child: Padding(
        padding: const EdgeInsets.fromLTRB(16, 8, 16, 18),
        child: Container(
          height: 88,
          decoration: BoxDecoration(
            color: theme.colorScheme.surface,
            borderRadius: BorderRadius.circular(28),
            boxShadow: [
              BoxShadow(
                color: theme.brightness == Brightness.dark
                    ? Colors.black.withOpacity(0.35)
                    : Colors.black.withOpacity(0.10),
                blurRadius: 22,
                offset: const Offset(0, 12),
              ),
            ],
          ),
          child: Stack(
            clipBehavior: Clip.none,
            alignment: Alignment.center,
            children: [
              Positioned.fill(
                child: Row(
                  children: [
                    Expanded(
                      child: Row(
                        mainAxisAlignment: MainAxisAlignment.spaceEvenly,
                        children: [
                          _buildNavItem(context, icon: Icons.schedule_rounded, index: 0, label: 'Günlük', activeColor: activeColor, inactiveColor: inactiveColor),
                          _buildNavItem(context, icon: Icons.restaurant_rounded, index: 1, label: 'Yemek', activeColor: activeColor, inactiveColor: inactiveColor),
                        ],
                      ),
                    ),
                    const SizedBox(width: 80),
                    Expanded(
                      child: Row(
                        mainAxisAlignment: MainAxisAlignment.spaceEvenly,
                        children: [
                          _buildNavItem(context, icon: Icons.medication_rounded, index: 2, label: 'İlaç', activeColor: activeColor, inactiveColor: inactiveColor),
                          _buildNavItem(context, icon: Icons.sports_handball_rounded, index: 3, label: 'Aktivite', activeColor: activeColor, inactiveColor: inactiveColor),
                        ],
                      ),
                    ),
                  ],
                ),
              ),
              Positioned(
                top: -18,
                left: 0,
                right: 0,
                child: Center(
                  child: _buildCenterAiButton(context),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }

  Widget _buildNavItem(
    BuildContext context, {
    required IconData icon,
    required int index,
    required String label,
    required Color activeColor,
    required Color inactiveColor,
  }) {
    final theme = Theme.of(context);
    final isSelected = _tabController.index == index;
    return GestureDetector(
      onTap: () => _tabController.animateTo(index),
      child: Container(
        padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 6),
        decoration: BoxDecoration(
          color: isSelected ? activeColor.withOpacity(0.12) : Colors.transparent,
          borderRadius: BorderRadius.circular(16),
        ),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Icon(icon, color: isSelected ? activeColor : inactiveColor, size: 24),
            const SizedBox(height: 4),
            Text(
              label,
              style: theme.textTheme.labelSmall?.copyWith(
                color: isSelected ? activeColor : inactiveColor,
                fontWeight: FontWeight.bold,
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildCenterAiButton(BuildContext context) {
    final theme = Theme.of(context);
    return GestureDetector(
      onTap: () => _tabController.animateTo(4),
      child: Container(
        width: 72,
        height: 72,
        decoration: BoxDecoration(
          gradient: LinearGradient(
            colors: [theme.colorScheme.primary, theme.colorScheme.secondary],
            begin: Alignment.topLeft,
            end: Alignment.bottomRight,
          ),
          borderRadius: BorderRadius.circular(24),
          boxShadow: [
            BoxShadow(
              color: theme.colorScheme.primary.withOpacity(0.28),
              blurRadius: 22,
              offset: const Offset(0, 12),
            ),
          ],
        ),
        child: const Icon(Icons.auto_awesome_rounded, color: Colors.white, size: 32),
      ),
    );
  }

  void _logout() async {
    await AuthStorageService.clear();
    if (!mounted) return;
    Navigator.of(context).pushAndRemoveUntil(
      MaterialPageRoute(builder: (_) => const LoginScreen()),
      (route) => false,
    );
  }

  Future<void> _saveDailyEntries() async {
    setState(() {
      _isDailySaving = true;
    });

    try {
      final repository = ref.read(teacherRepositoryProvider);
      for (final entry in _dailyEntries) {
        final recordId = entry.dailyRecordId?.isNotEmpty == true
            ? entry.dailyRecordId!
            : await repository.createDailyRecord(entry.studentId);

        await repository.updateDailyRecord(
          dailyRecordId: recordId,
          sleepStatus: entry.sleepStatus,
          waterAmountInMilliliters: entry.waterAmount,
          teacherNote: entry.teacherNote.trim().isEmpty
              ? null
              : entry.teacherNote.trim(),
        );
        entry.dailyRecordId = recordId;
      }

      ref.refresh(classroomDailyRecordsProvider(widget.classroom.id));
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Uyku ve günlük bilgiler kaydedildi.')),
        );
      }
    } catch (error) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('Uyku kaydı sırasında hata oluştu: $error')),
        );
      }
    } finally {
      if (mounted) {
        setState(() {
          _isDailySaving = false;
        });
      }
    }
  }

  void _startWeeklyPlanEdit(List<WeeklyMealPlan> plans) {
    setState(() {
      _weeklyPlanEditMode = true;
      _weeklyPlanEdits = plans
          .map((plan) => WeeklyMealPlan(
                id: plan.id,
                dayOfWeek: plan.dayOfWeek,
                mealName: plan.mealName,
                plannedCalories: plan.plannedCalories,
                foodContent: plan.foodContent,
                proteinGrams: plan.proteinGrams,
                carbsGrams: plan.carbsGrams,
              ))
          .toList();
    });
  }

  void _cancelWeeklyPlanEdit() {
    setState(() {
      _weeklyPlanEditMode = false;
      _weeklyPlanEdits = [];
    });
  }

  Future<void> _saveWeeklyMealPlanUpdates() async {
    setState(() {
      _isWeeklySaving = true;
    });

    try {
      final repository = ref.read(teacherRepositoryProvider);
      await repository.updateClassroomWeeklyMealPlans(
        widget.classroom.id,
        _weeklyPlanEdits,
      );
      ref.refresh(classroomWeeklyMealPlansProvider(widget.classroom.id));
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Haftalık yemek planı güncellendi.')),
        );
      }
      setState(() {
        _weeklyPlanEditMode = false;
      });
    } catch (error) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('Haftalık plan kaydında hata oluştu: $error')),
        );
      }
    } finally {
      if (mounted) {
        setState(() {
          _isWeeklySaving = false;
        });
      }
    }
  }

  void _updateWeeklyPlanField(
    int index, {
    String? mealName,
    String? foodContent,
  }) {
    setState(() {
      final plan = _weeklyPlanEdits[index];
      _weeklyPlanEdits[index] = WeeklyMealPlan(
        id: plan.id,
        dayOfWeek: plan.dayOfWeek,
        mealName: mealName ?? plan.mealName,
        plannedCalories: plan.plannedCalories,
        foodContent: foodContent ?? plan.foodContent,
        proteinGrams: plan.proteinGrams,
        carbsGrams: plan.carbsGrams,
      );
    });
  }

  void _setAllSleepStatus(int status) {
    setState(() {
      for (final entry in _dailyEntries) {
        entry.sleepStatus = status;
      }
    });
  }

  void _setAllWaterInput(String value) {
    setState(() {
      for (final entry in _dailyEntries) {
        entry.waterInput = value;
      }
    });
  }

  Widget _buildHeader() {
    return Container(
      width: double.infinity,
      color: Theme.of(context).colorScheme.surface,
      padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 14),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceBetween,
        children: [
          Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                widget.classroom.name,
                style: const TextStyle(
                  fontSize: 18,
                  fontWeight: FontWeight.bold,
                ),
              ),
              const SizedBox(height: 4),
              Text(
                '${widget.classroom.studentCount} öğrenci',
                style: const TextStyle(color: Colors.grey),
              ),
            ],
          ),
        ],
      ),
    );
  }

  // 'Sınıf' tab removed — student list rendered elsewhere if needed.

  Widget _buildDailyTab(AsyncValue<List<ClassroomDailyRecord>> dailyAsync) {
    return dailyAsync.when(
      data: (records) {
        if (!_dailyEntriesInitialized) {
          WidgetsBinding.instance.addPostFrameCallback((_) {
            if (!mounted) return;
            setState(() {
              _dailyEntriesInitialized = true;
              _dailyEntries.clear();
              for (final record in records) {
                _dailyEntries.add(
                  _TeacherDailyEntry(
                    studentId: record.studentId,
                    firstName: record.firstName,
                    lastName: record.lastName,
                    dailyRecordId: record.dailyRecordId,
                    sleepStatus: record.sleepStatus ?? 0,
                    waterInput:
                        record.waterIntake?.toString() ?? '',
                    teacherNote: record.teacherNotes ?? '',
                    isAbsent: record.isAbsent,
                  ),
                );
              }
            });
          });
        }

        if (records.isEmpty) {
          return const Center(
            child: Text('Bugün için günlük kayıt bulunamadı.'),
          );
        }

        return Stack(
          children: [
            RefreshIndicator(
              onRefresh: () async {
                ref.refresh(classroomDailyRecordsProvider(widget.classroom.id));
                setState(() {
                  _dailyEntriesInitialized = false;
                });
              },
              child: ListView(
                controller: _dailyScrollController,
                padding: const EdgeInsets.fromLTRB(16, 16, 16, 92),
                children: [
              Card(
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(16),
                  ),
                elevation: 2,
                child: Padding(
                  padding: const EdgeInsets.all(16.0),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.stretch,
                    children: [
                      const Text(
                        'Uyku & Günlük Takip',
                        style: TextStyle(
                          fontSize: 18,
                          fontWeight: FontWeight.bold,
                        ),
                      ),
                      const SizedBox(height: 8),
                      const Text(
                        'Toplu uyku durumu ve su miktarı ayarlayarak tüm öğrenciler için hızlı işlem yapın.',
                      ),
                      const SizedBox(height: 16),
                      Row(
                        children: [
                          Expanded(
                            child: FilledButton.tonal(
                              onPressed: () => _setAllSleepStatus(2),
                              child: const Text('İyi uyudu'),
                            ),
                          ),
                          const SizedBox(width: 8),
                          Expanded(
                            child: FilledButton.tonal(
                              onPressed: () => _setAllSleepStatus(1),
                              child: const Text('Az uyudu'),
                            ),
                          ),
                          const SizedBox(width: 8),
                          Expanded(
                            child: FilledButton.tonal(
                              onPressed: () => _setAllSleepStatus(0),
                              child: const Text('Uyumadı'),
                            ),
                          ),
                        ],
                      ),
                      const SizedBox(height: 12),
                      Row(
                        children: [
                          Expanded(
                            child: TextFormField(
                              key: const ValueKey('bulk-water-input'),
                              keyboardType: TextInputType.number,
                              decoration: const InputDecoration(
                                labelText: 'Tümüne su miktarı (ml)',
                                border: OutlineInputBorder(),
                              ),
                              onChanged: (value) {},
                              onFieldSubmitted: (value) {
                                _setAllWaterInput(value);
                              },
                            ),
                          ),
                          const SizedBox(width: 8),
                          FilledButton(
                            onPressed: () => _setAllWaterInput('200'),
                            child: const Text('200 ml'),
                          ),
                        ],
                      ),
                      const SizedBox(height: 16),
                    ],
                  ),
                ),
              ),
              const SizedBox(height: 16),
              ..._dailyEntries.map((entry) {
                return Card(
                  key: ValueKey('daily-${entry.studentId}'),
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(14),
                  ),
                  child: Padding(
                    padding: const EdgeInsets.all(16.0),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.stretch,
                      children: [
                        Row(
                          children: [
                            Expanded(
                              child: Text(
                                '${entry.firstName} ${entry.lastName}',
                                style: const TextStyle(
                                  fontWeight: FontWeight.bold,
                                ),
                              ),
                            ),
                            if (entry.isAbsent)
                              Chip(
                                label: const Text('Devamsız'),
                                backgroundColor: Colors.red.shade50,
                                labelStyle: const TextStyle(color: Colors.red),
                              ),
                          ],
                        ),
                        const SizedBox(height: 12),
                        DropdownButtonFormField<int>(
                          value: entry.sleepStatus,
                          decoration: const InputDecoration(
                            labelText: 'Uyku Durumu',
                          ),
                          items: const [
                            DropdownMenuItem(value: 0, child: Text('Uyumadı')),
                            DropdownMenuItem(value: 1, child: Text('Az uyudu')),
                            DropdownMenuItem(value: 2, child: Text('İyi uyudu')),
                          ],
                          onChanged: (value) {
                            if (value == null) return;
                            setState(() {
                              entry.sleepStatus = value;
                            });
                          },
                        ),
                        const SizedBox(height: 12),
                        TextFormField(
                          key: ValueKey('water-${entry.studentId}'),
                          initialValue: entry.waterInput,
                          keyboardType: TextInputType.number,
                          decoration: const InputDecoration(
                            labelText: 'Su Miktarı (ml)',
                          ),
                          onChanged: (value) {
                            entry.waterInput = value;
                          },
                        ),
                        const SizedBox(height: 12),
                        TextFormField(
                          key: ValueKey('note-${entry.studentId}'),
                          initialValue: entry.teacherNote,
                          decoration: const InputDecoration(
                            labelText: 'Öğretmen Notu',
                          ),
                          maxLines: 2,
                          onChanged: (value) {
                            entry.teacherNote = value;
                          },
                        ),
                      ],
                    ),
                  ),
                );
              }).toList(),
            ],
          ),
            ),
            Positioned(
              left: 16,
              right: 16,
              bottom: 16,
              child: FilledButton(
                onPressed: _isDailySaving ? null : _saveDailyEntries,
                style: FilledButton.styleFrom(
                  padding: const EdgeInsets.symmetric(vertical: 16),
                  backgroundColor: AppColors.primary,
                ),
                child: Text(_isDailySaving ? 'Kaydediliyor...' : 'Günlük Kaydet'),
              ),
            ),
          ],
        );
      },
      loading: () => const Center(child: CircularProgressIndicator()),
      error: (error, stack) => Center(child: Text('Hata: $error')),
    );
  }

  Widget _buildMealTabCombined(
    AsyncValue<List<Student>> studentsAsync,
    AsyncValue<List<StudentMealRecord>> mealAsync,
    AsyncValue<List<WeeklyMealPlan>> weeklyPlansAsync,
  ) {
    final theme = Theme.of(context);
    return Column(
      children: [
        Padding(
          padding: const EdgeInsets.fromLTRB(16, 12, 16, 4),
          child: Container(
            height: 46,
            padding: const EdgeInsets.all(4),
            decoration: BoxDecoration(
              color: theme.brightness == Brightness.dark
                  ? const Color(0xFF1E293B)
                  : const Color(0xFFF1F5F9),
              borderRadius: BorderRadius.circular(16),
            ),
            child: Row(
              children: [
                Expanded(
                  child: GestureDetector(
                    onTap: () => setState(() => _mealTabSubIndex = 0),
                    child: Container(
                      decoration: BoxDecoration(
                        color: _mealTabSubIndex == 0
                            ? theme.colorScheme.surface
                            : Colors.transparent,
                        borderRadius: BorderRadius.circular(12),
                        boxShadow: _mealTabSubIndex == 0
                            ? [
                                BoxShadow(
                                  color: Colors.black.withOpacity(0.05),
                                  blurRadius: 4,
                                  offset: const Offset(0, 2),
                                )
                              ]
                            : null,
                      ),
                      child: Center(
                        child: Text(
                          'Günlük Tüketim',
                          style: TextStyle(
                            fontSize: 12,
                            fontWeight: FontWeight.bold,
                            color: _mealTabSubIndex == 0
                                ? theme.colorScheme.primary
                                : theme.colorScheme.onSurface.withOpacity(0.6),
                          ),
                        ),
                      ),
                    ),
                  ),
                ),
                Expanded(
                  child: GestureDetector(
                    onTap: () => setState(() => _mealTabSubIndex = 1),
                    child: Container(
                      decoration: BoxDecoration(
                        color: _mealTabSubIndex == 1
                            ? theme.colorScheme.surface
                            : Colors.transparent,
                        borderRadius: BorderRadius.circular(12),
                        boxShadow: _mealTabSubIndex == 1
                            ? [
                                BoxShadow(
                                  color: Colors.black.withOpacity(0.05),
                                  blurRadius: 4,
                                  offset: const Offset(0, 2),
                                )
                              ]
                            : null,
                      ),
                      child: Center(
                        child: Text(
                          'Haftalık Menü',
                          style: TextStyle(
                            fontSize: 12,
                            fontWeight: FontWeight.bold,
                            color: _mealTabSubIndex == 1
                                ? theme.colorScheme.primary
                                : theme.colorScheme.onSurface.withOpacity(0.6),
                          ),
                        ),
                      ),
                    ),
                  ),
                ),
              ],
            ),
          ),
        ),
        Expanded(
          child: _mealTabSubIndex == 0
              ? _buildMealTab(studentsAsync, mealAsync)
              : _buildWeeklyMealPlanTab(weeklyPlansAsync),
        ),
      ],
    );
  }

  Widget _buildMealTab(
    AsyncValue<List<Student>> studentsAsync,
    AsyncValue<List<StudentMealRecord>> mealAsync,
  ) {
    return studentsAsync.when(
      data: (students) {
        return mealAsync.when(
          data: (records) {
            if (!_mealEntriesInitialized) {
              WidgetsBinding.instance.addPostFrameCallback((_) {
                if (!mounted) return;
                setState(() {
                  _mealEntriesInitialized = true;
                  _mealEntries.clear();
                  for (final student in students) {
                    final mealRecord = records.firstWhere(
                      (record) => record.studentId == student.id,
                      orElse: () => StudentMealRecord(
                        studentId: student.id,
                        firstName: student.firstName,
                        lastName: student.lastName,
                        meals: const [],
                      ),
                    );

                    final mealNames = ['Kahvaltı', 'Öğle Yemeği', 'İkindi Kahvaltısı'];
                    final subEntries = mealNames.map((mealName) {
                      final existing = mealRecord.meals.firstWhere(
                        (m) => m.mealName.toLowerCase().contains(mealName.split(' ')[0].toLowerCase()),
                        orElse: () => MealDetail(
                          mealRecordId: '',
                          mealName: mealName,
                          statusType: 0,
                          statusDescription: '',
                        ),
                      );
                      return _TeacherMealSubEntry(
                        mealName: existing.mealName.isNotEmpty ? existing.mealName : mealName,
                        mealRecordId: existing.mealRecordId,
                        status: existing.statusType,
                        notes: existing.statusDescription,
                      );
                    }).toList();

                    _mealEntries.add(
                      _TeacherMealEntry(
                        studentId: student.id,
                        firstName: student.firstName,
                        lastName: student.lastName,
                        meals: subEntries,
                      ),
                    );
                  }
                });
              });
            }

            if (students.isEmpty) {
              return const Center(child: Text('Sınıfta öğrenci bulunamadı.'));
            }

            final theme = Theme.of(context);

            return RefreshIndicator(
              onRefresh: () async {
                ref.refresh(classroomStudentsProvider(widget.classroom.id));
                ref.refresh(classroomMealRecordsProvider(widget.classroom.id));
                setState(() {
                  _mealEntriesInitialized = false;
                });
              },
              child: ListView(
                padding: const EdgeInsets.all(16),
                children: [
                  Card(
                    shape: RoundedRectangleBorder(
                      borderRadius: BorderRadius.circular(16),
                    ),
                    elevation: 2,
                    child: Padding(
                      padding: const EdgeInsets.all(16.0),
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.stretch,
                        children: [
                          const Text(
                            'Yemek Girişi',
                            style: TextStyle(
                              fontSize: 18,
                              fontWeight: FontWeight.bold,
                            ),
                          ),
                          const SizedBox(height: 8),
                          const Text(
                            'Öğrencilerin yemek durumlarını hızlıca kaydedin. Toplu doldurma ve not ekleme seçenekleriyle daha hızlı ilerleyin.',
                          ),
                          const SizedBox(height: 16),
                          Wrap(
                            spacing: 8,
                            runSpacing: 8,
                            children: [
                              FilledButton.tonal(
                                onPressed: () => _setAllMealStatus(3),
                                child: const Text('Tümü yedi'),
                              ),
                              FilledButton.tonal(
                                onPressed: () => _setAllMealStatus(2),
                                child: const Text('Yarı yedi'),
                              ),
                              FilledButton.tonal(
                                onPressed: () => _setAllMealStatus(1),
                                child: const Text('Az yedi'),
                              ),
                              FilledButton.tonal(
                                onPressed: () => _setAllMealStatus(0),
                                child: const Text('Durum yok'),
                              ),
                            ],
                          ),
                        ],
                      ),
                    ),
                  ),
                  const SizedBox(height: 16),
                  ..._mealEntries.map((entry) {
                    return Card(
                      key: ValueKey(entry.studentId),
                      shape: RoundedRectangleBorder(
                        borderRadius: BorderRadius.circular(16),
                      ),
                      child: ExpansionTile(
                        title: Row(
                          children: [
                            Expanded(
                              child: Text(
                                '${entry.firstName} ${entry.lastName}',
                                style: const TextStyle(
                                  fontWeight: FontWeight.bold,
                                ),
                              ),
                            ),
                            Builder(
                              builder: (context) {
                                final enteredCount = entry.meals.where((m) => m.mealRecordId.isNotEmpty && m.mealRecordId != '00000000-0000-0000-0000-000000000000').length;
                                return Chip(
                                  label: Text(
                                    enteredCount == 0 ? 'Girilmedi' : 'Giriş ($enteredCount/3)',
                                  ),
                                  backgroundColor: enteredCount == 3
                                      ? Colors.green.shade50
                                      : (enteredCount > 0 ? Colors.blue.shade50 : Colors.grey.shade100),
                                  labelStyle: TextStyle(
                                    color: enteredCount == 3
                                        ? Colors.green.shade700
                                        : (enteredCount > 0 ? Colors.blue.shade700 : Colors.grey.shade700),
                                    fontWeight: FontWeight.bold,
                                    fontSize: 11,
                                  ),
                                );
                              },
                            ),
                          ],
                        ),
                        childrenPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
                        children: [
                          ...entry.meals.map((meal) {
                            Color containerBg;
                            Color borderCol;
                            Color headerColor;
                            
                            if (meal.mealName.toLowerCase().contains('kahvaltı') && !meal.mealName.toLowerCase().contains('ikindi')) {
                              containerBg = theme.brightness == Brightness.dark ? const Color(0xFF2D2510) : const Color(0xFFFEF3C7).withOpacity(0.45);
                              borderCol = const Color(0xFFFBBF24).withOpacity(0.2);
                              headerColor = const Color(0xFFD97706);
                            } else if (meal.mealName.toLowerCase().contains('öğle')) {
                              containerBg = theme.brightness == Brightness.dark ? const Color(0xFF1E283C) : const Color(0xFFDBEAFE).withOpacity(0.45);
                              borderCol = const Color(0xFF3B82F6).withOpacity(0.2);
                              headerColor = const Color(0xFF2563EB);
                            } else {
                              containerBg = theme.brightness == Brightness.dark ? const Color(0xFF142D23) : const Color(0xFFD1FAE5).withOpacity(0.45);
                              borderCol = const Color(0xFF10B981).withOpacity(0.2);
                              headerColor = const Color(0xFF059669);
                            }

                            return Container(
                              margin: const EdgeInsets.only(bottom: 12),
                              padding: const EdgeInsets.all(12),
                              decoration: BoxDecoration(
                                color: containerBg,
                                borderRadius: BorderRadius.circular(12),
                                border: Border.all(color: borderCol, width: 1),
                              ),
                              child: Column(
                                crossAxisAlignment: CrossAxisAlignment.stretch,
                                children: [
                                  Row(
                                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                                    children: [
                                      Text(
                                        meal.mealName,
                                        style: TextStyle(
                                          fontWeight: FontWeight.w900,
                                          fontSize: 13,
                                          color: headerColor,
                                        ),
                                      ),
                                      if (meal.mealRecordId.isNotEmpty && meal.mealRecordId != '00000000-0000-0000-0000-000000000000')
                                        const Text(
                                          'Kayıt Var',
                                          style: TextStyle(
                                            fontSize: 10,
                                            color: Colors.green,
                                            fontWeight: FontWeight.bold,
                                          ),
                                        ),
                                    ],
                                  ),
                                  const SizedBox(height: 10),
                                  DropdownButtonFormField<int>(
                                    value: meal.status,
                                    decoration: const InputDecoration(
                                      labelText: 'Yemek Durumu',
                                      contentPadding: EdgeInsets.symmetric(horizontal: 12, vertical: 8),
                                    ),
                                    items: const [
                                      DropdownMenuItem(value: 0, child: Text('Durum yok')),
                                      DropdownMenuItem(value: 1, child: Text('Az yedi')),
                                      DropdownMenuItem(value: 2, child: Text('Yarı yedi')),
                                      DropdownMenuItem(value: 3, child: Text('Tamamını yedi')),
                                    ],
                                    onChanged: (value) {
                                      if (value == null) return;
                                      setState(() {
                                        meal.status = value;
                                      });
                                    },
                                  ),
                                  const SizedBox(height: 10),
                                  TextFormField(
                                    key: ValueKey('meal-note-${entry.studentId}-${meal.mealName}'),
                                    initialValue: meal.notes,
                                    decoration: const InputDecoration(
                                      labelText: 'Yemek Notu / Açıklama',
                                      contentPadding: EdgeInsets.symmetric(horizontal: 12, vertical: 8),
                                    ),
                                    maxLines: 1,
                                    onChanged: (value) {
                                      meal.notes = value;
                                    },
                                  ),
                                ],
                              ),
                            );
                          }).toList(),
                        ],
                      ),
                    );
                  }).toList(),
                  const SizedBox(height: 16),
                  FilledButton(
                    onPressed: _isSubmitting ? null : _saveMealEntries,
                    child: Text(_isSubmitting ? 'Kaydediliyor...' : 'Yemekleri Kaydet'),
                  ),
                ],
              ),
            );
          },
          loading: () => const Center(child: CircularProgressIndicator()),
          error: (error, stack) => Center(child: Text('Hata: $error')),
        );
      },
      loading: () => const Center(child: CircularProgressIndicator()),
      error: (error, stack) => Center(child: Text('Hata: $error')),
    );
  }

  Widget _buildMedicationTab(AsyncValue<List<Student>> studentsAsync) {
    return studentsAsync.when(
      data: (students) {
        if (!_medicationEntriesInitialized) {
          WidgetsBinding.instance.addPostFrameCallback((_) {
            if (!mounted) return;
            setState(() {
              _medicationEntriesInitialized = true;
              _medicationEntries.clear();
              for (final student in students) {
                _medicationEntries.add(
                  _TeacherMedicationEntry(
                    studentId: student.id,
                    firstName: student.firstName,
                    lastName: student.lastName,
                    medicineName: '',
                    dosage: '',
                    time: '',
                    note: '',
                    taken: false,
                  ),
                );
              }
            });
          });
        }

        if (students.isEmpty) {
          return const Center(child: Text('Sınıfta öğrenci bulunamadı.'));
        }

        return RefreshIndicator(
          onRefresh: () async {
            ref.refresh(classroomStudentsProvider(widget.classroom.id));
            setState(() {
              _medicationEntriesInitialized = false;
            });
          },
          child: ListView(
            padding: const EdgeInsets.all(16),
            children: [
              Card(
                shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(16),
                ),
                elevation: 2,
                child: Padding(
                  padding: const EdgeInsets.all(16.0),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.stretch,
                    children: const [
                      Text(
                        'İlaç Girişi',
                        style: TextStyle(
                          fontSize: 18,
                          fontWeight: FontWeight.bold,
                        ),
                      ),
                      SizedBox(height: 8),
                      Text(
                        'Sadece ilaç verilecek öğrenciler için bilgileri girin. İsterseniz alındı durumunu işaretleyin ve not ekleyin.',
                      ),
                    ],
                  ),
                ),
              ),
              const SizedBox(height: 16),
              ..._medicationEntries.map((entry) {
                return Card(
                    key: ValueKey('med-${entry.studentId}'),
                    shape: RoundedRectangleBorder(
                      borderRadius: BorderRadius.circular(16),
                    ),
                  child: ExpansionTile(
                    title: Row(
                      children: [
                        Expanded(
                          child: Text(
                            '${entry.firstName} ${entry.lastName}',
                            style: const TextStyle(
                              fontWeight: FontWeight.bold,
                            ),
                          ),
                        ),
                        Chip(
                          label: Text(entry.taken ? 'İlaç alındı' : 'Beklemede'),
                          backgroundColor: entry.taken
                              ? Colors.green.withOpacity(0.16)
                              : Colors.grey.withOpacity(0.16),
                          labelStyle: TextStyle(
                            color: entry.taken ? Colors.green : Colors.grey[700],
                            fontWeight: FontWeight.w600,
                          ),
                        ),
                      ],
                    ),
                    childrenPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
                    children: [
                      TextFormField(
                        key: ValueKey('med-name-${entry.studentId}'),
                        initialValue: entry.medicineName,
                        decoration: const InputDecoration(
                          labelText: 'İlaç Adı',
                        ),
                        onChanged: (value) {
                          entry.medicineName = value;
                        },
                      ),
                      const SizedBox(height: 10),
                      Row(
                        children: [
                          Expanded(
                            child: TextFormField(
                              key: ValueKey('med-dosage-${entry.studentId}'),
                              initialValue: entry.dosage,
                              decoration: const InputDecoration(
                                labelText: 'Dozaj',
                              ),
                              onChanged: (value) {
                                entry.dosage = value;
                              },
                            ),
                          ),
                          const SizedBox(width: 12),
                          Expanded(
                            child: TextFormField(
                              key: ValueKey('med-time-${entry.studentId}'),
                              initialValue: entry.time,
                              decoration: const InputDecoration(
                                labelText: 'Veriliş Saati',
                              ),
                              onChanged: (value) {
                                entry.time = value;
                              },
                            ),
                          ),
                        ],
                      ),
                      const SizedBox(height: 10),
                      TextFormField(
                        key: ValueKey('med-note-${entry.studentId}'),
                        initialValue: entry.note,
                        decoration: const InputDecoration(
                          labelText: 'Not',
                        ),
                        maxLines: 2,
                        onChanged: (value) {
                          entry.note = value;
                        },
                      ),
                      const SizedBox(height: 10),
                      Row(
                        children: [
                          Checkbox(
                            value: entry.taken,
                            onChanged: (value) {
                              if (value == null) return;
                              setState(() {
                                entry.taken = value;
                              });
                            },
                          ),
                          const Expanded(
                            child: Text('İlaç alındı olarak işaretle'),
                          ),
                        ],
                      ),
                    ],
                  ),
                );
              }).toList(),
              const SizedBox(height: 16),
              FilledButton(
                onPressed: _isSubmitting ? null : _saveMedicationEntries,
                child: Text(_isSubmitting ? 'Kaydediliyor...' : 'İlaçları Kaydet'),
              ),
            ],
          ),
        );
      },
      loading: () => const Center(child: CircularProgressIndicator()),
      error: (error, stack) => Center(child: Text('Hata: $error')),
    );
  }

  Widget _buildWeeklyMealPlanTab(
    AsyncValue<List<WeeklyMealPlan>> weeklyPlansAsync,
  ) {
    return weeklyPlansAsync.when(
      data: (plans) {
        if (plans.isEmpty) {
          return const Center(child: Text('Haftalık yemek planı bulunamadı.'));
        }

        final displayPlans = _weeklyPlanEditMode ? _weeklyPlanEdits : plans;
        final sortedPlans = [...displayPlans]
          ..sort((a, b) {
            final order = a.dayOfWeek.compareTo(b.dayOfWeek);
            return order != 0 ? order : a.mealName.compareTo(b.mealName);
          });

        return RefreshIndicator(
          onRefresh: () async {
            ref.refresh(classroomWeeklyMealPlansProvider(widget.classroom.id));
            setState(() {
              _weeklyPlanEditMode = false;
              _weeklyPlanEdits = [];
            });
          },
          child: ListView(
            padding: const EdgeInsets.all(16),
            children: [
              Card(
                shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(16),
                ),
                elevation: 2,
                child: Padding(
                  padding: const EdgeInsets.all(16.0),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.stretch,
                    children: [
                      const Text(
                        'Haftalık Yemek Programı',
                        style: TextStyle(
                          fontSize: 18,
                          fontWeight: FontWeight.bold,
                        ),
                      ),
                      const SizedBox(height: 8),
                      const Text(
                        'Haftalık menüye not ekleyebilir veya yemek adlarını düzenleyebilirsiniz.',
                      ),
                      const SizedBox(height: 16),
                      Row(
                        children: [
                          FilledButton(
                            onPressed: _weeklyPlanEditMode
                                ? _saveWeeklyMealPlanUpdates
                                : () => _startWeeklyPlanEdit(plans),
                            child: Text(_weeklyPlanEditMode ? 'Değişiklikleri Kaydet' : 'Düzenle'),
                          ),
                          const SizedBox(width: 12),
                          if (_weeklyPlanEditMode)
                            FilledButton.tonal(
                              onPressed: _cancelWeeklyPlanEdit,
                              child: const Text('İptal'),
                            ),
                        ],
                      ),
                    ],
                  ),
                ),
              ),
              const SizedBox(height: 16),
              ...sortedPlans.map((plan) {
                return Card(
                  key: ValueKey('plan-${plan.id}'),
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(14),
                  ),
                  child: Padding(
                    padding: const EdgeInsets.all(16.0),
                    child: _weeklyPlanEditMode
                        ? Column(
                            crossAxisAlignment: CrossAxisAlignment.stretch,
                            children: [
                              Text(
                                plan.dayLabel,
                                style: const TextStyle(
                                  fontSize: 16,
                                  fontWeight: FontWeight.bold,
                                ),
                              ),
                              const SizedBox(height: 12),
                              TextFormField(
                                initialValue: plan.mealName,
                                decoration: const InputDecoration(
                                  labelText: 'Yemek Adı',
                                ),
                                onChanged: (value) {
                                  final index = _weeklyPlanEdits.indexWhere(
                                      (item) => item.id == plan.id);
                                  if (index == -1) return;
                                  _updateWeeklyPlanField(index, mealName: value);
                                },
                              ),
                              const SizedBox(height: 12),
                              TextFormField(
                                initialValue: plan.foodContent ?? '',
                                decoration: const InputDecoration(
                                  labelText: 'İçerik',
                                ),
                                maxLines: 2,
                                onChanged: (value) {
                                  final index = _weeklyPlanEdits.indexWhere(
                                      (item) => item.id == plan.id);
                                  if (index == -1) return;
                                  _updateWeeklyPlanField(index, foodContent: value);
                                },
                              ),
                            ],
                          )
                        : ListTile(
                            title: Text('${plan.dayLabel} • ${plan.mealName}'),
                            subtitle: Text(
                              plan.nutritionSummary.isEmpty
                                  ? 'Besin bilgisi yok'
                                  : plan.nutritionSummary,
                            ),
                          ),
                  ),
                );
              }).toList(),
              if (_weeklyPlanEditMode) ...[
                const SizedBox(height: 16),
                FilledButton(
                  onPressed: _isWeeklySaving ? null : _saveWeeklyMealPlanUpdates,
                  child: Text(_isWeeklySaving ? 'Kaydediliyor...' : 'Değişiklikleri Kaydet'),
                ),
              ],
            ],
          ),
        );
      },
      loading: () => const Center(child: CircularProgressIndicator()),
      error: (error, stack) => Center(child: Text('Hata: $error')),
    );
  }

  Widget _buildActivitiesTab(
    AsyncValue<List<ClassroomActivity>> activitiesAsync,
  ) {
    return activitiesAsync.when(
      data: (activities) {
        final theme = Theme.of(context);
        final today = DateTime.now();
        final currentDayOfWeek = today.weekday;
        final monday = today.subtract(Duration(days: currentDayOfWeek - 1));

        final weekDays = List.generate(5, (index) {
          final date = monday.add(Duration(days: index));
          return {
            'name': ['Pazartesi', 'Salı', 'Çarşamba', 'Perşembe', 'Cuma'][index],
            'date': date,
          };
        });

        return RefreshIndicator(
          onRefresh: () async {
            ref.refresh(classroomActivitiesProvider(widget.classroom.id));
          },
          child: ListView(
            padding: const EdgeInsets.all(16),
            children: [
              Card(
                shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(16),
                ),
                elevation: 2,
                child: Padding(
                  padding: const EdgeInsets.all(16.0),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.stretch,
                    children: [
                      const Text(
                        'Haftalık Ders Akışı',
                        style: TextStyle(
                          fontSize: 18,
                          fontWeight: FontWeight.bold,
                        ),
                      ),
                      const SizedBox(height: 8),
                      const Text(
                        'Pazartesi - Cuma arası haftalık ders ve etkinlik planını buradan görebilir, tamamlayabilir veya yeni ekleme yapabilirsiniz.',
                      ),
                    ],
                  ),
                ),
              ),
              const SizedBox(height: 16),
              ...weekDays.map((day) {
                final date = day['date'] as DateTime;
                final dayName = day['name'] as String;

                final dayActs = activities.where((act) {
                  return act.activityDate.year == date.year &&
                      act.activityDate.month == date.month &&
                      act.activityDate.day == date.day;
                }).toList()..sort((a, b) {
                  final aMinutes = a.startTime.hour * 60 + a.startTime.minute;
                  final bMinutes = b.startTime.hour * 60 + b.startTime.minute;
                  return aMinutes.compareTo(bMinutes);
                });

                return Card(
                  key: ValueKey('day-${date.day}'),
                  margin: const EdgeInsets.only(bottom: 16),
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(18),
                    side: BorderSide(
                      color: date.day == today.day && date.month == today.month
                          ? theme.colorScheme.primary.withOpacity(0.3)
                          : Colors.transparent,
                      width: 1.5,
                    ),
                  ),
                  child: Padding(
                    padding: const EdgeInsets.all(16.0),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.stretch,
                      children: [
                        Row(
                          mainAxisAlignment: MainAxisAlignment.spaceBetween,
                          children: [
                            Column(
                              crossAxisAlignment: CrossAxisAlignment.start,
                              children: [
                                Text(
                                  dayName,
                                  style: TextStyle(
                                    fontSize: 16,
                                    fontWeight: FontWeight.bold,
                                    color: date.day == today.day && date.month == today.month
                                        ? theme.colorScheme.primary
                                        : theme.colorScheme.onSurface,
                                  ),
                                ),
                                const SizedBox(height: 2),
                                Text(
                                  '${date.day} ${_getTurkishMonthName(date.month)}',
                                  style: TextStyle(
                                    fontSize: 11,
                                    color: theme.colorScheme.onSurface.withOpacity(0.5),
                                    fontWeight: FontWeight.w600,
                                  ),
                                ),
                              ],
                            ),
                            IconButton.filledTonal(
                              onPressed: () => _showCreateActivityDialog(targetDate: date),
                              icon: const Icon(Icons.add_rounded, size: 20),
                              tooltip: 'Yeni Ekle',
                            ),
                          ],
                        ),
                        const SizedBox(height: 12),
                        if (dayActs.isNotEmpty) ...[
                          ...dayActs.map((act) {
                            String icon = "⭐";
                            Color bgColor;
                            Color borderCol;
                            Color textColor;

                            switch (act.type) {
                              case 0:
                                icon = "📚";
                                bgColor = theme.brightness == Brightness.dark ? const Color(0xFF1E283C) : const Color(0xFFDBEAFE).withOpacity(0.45);
                                borderCol = const Color(0xFF3B82F6).withOpacity(0.2);
                                textColor = theme.brightness == Brightness.dark ? const Color(0xFF93C5FD) : const Color(0xFF1E3A8A);
                                break;
                              case 1:
                                icon = "🎨";
                                bgColor = theme.brightness == Brightness.dark ? const Color(0xFF2D2510) : const Color(0xFFFEF3C7).withOpacity(0.45);
                                borderCol = const Color(0xFFFBBF24).withOpacity(0.2);
                                textColor = theme.brightness == Brightness.dark ? const Color(0xFFFDE047) : const Color(0xFF78350F);
                                break;
                              case 2:
                                icon = "🍲";
                                bgColor = theme.brightness == Brightness.dark ? const Color(0xFF142D23) : const Color(0xFFD1FAE5).withOpacity(0.45);
                                borderCol = const Color(0xFF10B981).withOpacity(0.2);
                                textColor = theme.brightness == Brightness.dark ? const Color(0xFF6EE7B7) : const Color(0xFF065F46);
                                break;
                              case 3:
                                icon = "😴";
                                bgColor = theme.brightness == Brightness.dark ? const Color(0xFF281E3C) : const Color(0xFFF3E8FF).withOpacity(0.45);
                                borderCol = const Color(0xFFA855F7).withOpacity(0.2);
                                textColor = theme.brightness == Brightness.dark ? const Color(0xFFD8B4FE) : const Color(0xFF581C87);
                                break;
                              case 4:
                              default:
                                icon = "🖌️";
                                bgColor = theme.brightness == Brightness.dark ? const Color(0xFF2D1424) : const Color(0xFFFCE7F3).withOpacity(0.45);
                                borderCol = const Color(0xFFEC4899).withOpacity(0.2);
                                textColor = theme.brightness == Brightness.dark ? const Color(0xFFF9A8D4) : const Color(0xFF831843);
                                break;
                            }

                            return Container(
                              margin: const EdgeInsets.only(bottom: 8),
                              padding: const EdgeInsets.all(12),
                              decoration: BoxDecoration(
                                color: bgColor,
                                border: Border.all(color: borderCol, width: 1),
                                borderRadius: BorderRadius.circular(14),
                              ),
                              child: Row(
                                children: [
                                  Text(
                                    icon,
                                    style: const TextStyle(fontSize: 20),
                                  ),
                                  const SizedBox(width: 12),
                                  Expanded(
                                    child: Column(
                                      crossAxisAlignment: CrossAxisAlignment.start,
                                      children: [
                                        Text(
                                          act.title,
                                          style: TextStyle(
                                            fontWeight: FontWeight.bold,
                                            fontSize: 13.5,
                                            color: textColor,
                                          ),
                                        ),
                                        const SizedBox(height: 4),
                                        Text(
                                          '🕒 ${act.formattedTime}',
                                          style: TextStyle(
                                            fontSize: 11,
                                            color: textColor.withOpacity(0.8),
                                            fontWeight: FontWeight.w600,
                                          ),
                                        ),
                                        if (act.description.isNotEmpty) ...[
                                          const SizedBox(height: 2),
                                          Text(
                                            act.description,
                                            style: TextStyle(
                                              fontSize: 11,
                                              color: theme.colorScheme.onSurface.withOpacity(0.6),
                                            ),
                                            maxLines: 2,
                                            overflow: TextOverflow.ellipsis,
                                          ),
                                        ],
                                      ],
                                    ),
                                  ),
                                  const SizedBox(width: 8),
                                  act.completed
                                      ? Container(
                                          padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
                                          decoration: BoxDecoration(
                                            color: Colors.green.withOpacity(0.12),
                                            borderRadius: BorderRadius.circular(8),
                                          ),
                                          child: const Row(
                                            mainAxisSize: MainAxisSize.min,
                                            children: [
                                              Icon(Icons.check, color: Colors.green, size: 14),
                                              SizedBox(width: 4),
                                              Text(
                                                'Bitti',
                                                style: TextStyle(
                                                  color: Colors.green,
                                                  fontWeight: FontWeight.bold,
                                                  fontSize: 10,
                                                ),
                                              ),
                                            ],
                                          ),
                                        )
                                      : TextButton(
                                          onPressed: () => _completeActivity(act.id),
                                          style: TextButton.styleFrom(
                                            padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
                                            minimumSize: Size.zero,
                                            tapTargetSize: MaterialTapTargetSize.shrinkWrap,
                                          ),
                                          child: const Text(
                                            'Tamamla',
                                            style: TextStyle(
                                              fontWeight: FontWeight.bold,
                                              fontSize: 11,
                                            ),
                                          ),
                                        ),
                                ],
                              ),
                            );
                          }).toList(),
                        ] else ...[
                          GestureDetector(
                            onTap: () => _showCreateActivityDialog(targetDate: date),
                            child: Container(
                              padding: const EdgeInsets.symmetric(vertical: 20),
                              decoration: BoxDecoration(
                                border: Border.all(
                                  color: theme.colorScheme.onSurface.withOpacity(0.1),
                                  style: BorderStyle.solid,
                                  width: 1,
                                ),
                                borderRadius: BorderRadius.circular(14),
                                color: theme.colorScheme.onSurface.withOpacity(0.02),
                              ),
                              child: Center(
                                child: Row(
                                  mainAxisAlignment: MainAxisAlignment.center,
                                  children: [
                                    Icon(
                                      Icons.add_circle_outline_rounded,
                                      size: 16,
                                      color: theme.colorScheme.onSurface.withOpacity(0.4),
                                    ),
                                    const SizedBox(width: 6),
                                    Text(
                                      'Aktivite planlanmadı. Ekle +',
                                      style: TextStyle(
                                        fontSize: 12,
                                        color: theme.colorScheme.onSurface.withOpacity(0.4),
                                        fontWeight: FontWeight.w600,
                                      ),
                                    ),
                                  ],
                                ),
                              ),
                            ),
                          ),
                        ],
                      ],
                    ),
                  ),
                );
              }).toList(),
              const SizedBox(height: 60),
            ],
          ),
        );
      },
      loading: () => const Center(child: CircularProgressIndicator()),
      error: (error, stack) => Center(child: Text('Hata: $error')),
    );
  }

  Future<void> _completeActivity(String activityId) async {
    try {
      final repository = ref.read(teacherRepositoryProvider);
      await repository.completeActivity(activityId);
      ref.refresh(classroomActivitiesProvider(widget.classroom.id));
      ScaffoldMessenger.of(
        context,
      ).showSnackBar(const SnackBar(content: Text('Aktivite tamamlandı.')));
    } catch (error) {
      ScaffoldMessenger.of(
        context,
      ).showSnackBar(SnackBar(content: Text('Hata: $error')));
    }
  }

  void _setAllMealStatus(int status) {
    setState(() {
      for (final entry in _mealEntries) {
        for (final meal in entry.meals) {
          meal.status = status;
        }
      }
    });
  }

  String _mealStatusLabel(int status) {
    switch (status) {
      case 1:
        return 'Az yedi';
      case 2:
        return 'Yarı yedi';
      case 3:
        return 'Tamamını yedi';
      default:
        return 'Durum yok';
    }
  }

  Color _mealStatusColor(int status) {
    switch (status) {
      case 1:
        return Colors.orange;
      case 2:
        return Colors.amber;
      case 3:
        return Colors.green;
      default:
        return Colors.grey;
    }
  }

  Future<void> _showCreateActivityDialog({DateTime? targetDate}) async {
    _errorMessage = null;
    _titleController.clear();
    _descriptionController.clear();
    _selectedDate = targetDate ?? DateTime.now();
    _selectedStartTime = const TimeOfDay(hour: 9, minute: 0);
    _selectedEndTime = const TimeOfDay(hour: 10, minute: 0);
    _selectedType = 0;

    await showDialog<void>(
      context: context,
      builder: (dialogContext) {
        return AlertDialog(
          title: const Text('Yeni Aktivite Oluştur'),
          content: StatefulBuilder(
            builder: (context, setState) {
              return SizedBox(
                width: 360,
                child: SingleChildScrollView(
                  child: Form(
                    key: _createFormKey,
                    child: Column(
                      mainAxisSize: MainAxisSize.min,
                      children: [
                        TextFormField(
                          controller: _titleController,
                          decoration: const InputDecoration(
                            labelText: 'Başlık',
                          ),
                          validator: (value) {
                            if (value == null || value.isEmpty) {
                              return 'Başlık gerekli.';
                            }
                            return null;
                          },
                        ),
                        const SizedBox(height: 12),
                        TextFormField(
                          controller: _descriptionController,
                          maxLines: 3,
                          decoration: const InputDecoration(
                            labelText: 'Açıklama',
                          ),
                          validator: (value) {
                            if (value == null || value.isEmpty) {
                              return 'Açıklama gerekli.';
                            }
                            return null;
                          },
                        ),
                        const SizedBox(height: 12),
                        DropdownButtonFormField<int>(
                          value: _selectedType,
                          decoration: const InputDecoration(
                            labelText: 'Aktivite Türü',
                          ),
                          items: const [
                            DropdownMenuItem(value: 0, child: Text('Genel')),
                            DropdownMenuItem(value: 1, child: Text('Kahvaltı')),
                            DropdownMenuItem(value: 2, child: Text('Öğle')),
                            DropdownMenuItem(
                              value: 3,
                              child: Text('Atıştırma'),
                            ),
                            DropdownMenuItem(value: 4, child: Text('Uyku')),
                            DropdownMenuItem(
                              value: 5,
                              child: Text('Serbest Oyun'),
                            ),
                            DropdownMenuItem(value: 6, child: Text('Sanat')),
                            DropdownMenuItem(value: 7, child: Text('Fen')),
                            DropdownMenuItem(value: 8, child: Text('Müzik')),
                            DropdownMenuItem(
                              value: 9,
                              child: Text('Açık Hava'),
                            ),
                            DropdownMenuItem(value: 99, child: Text('Diğer')),
                          ],
                          onChanged: (value) {
                            if (value != null) {
                              setState(() {
                                _selectedType = value;
                              });
                            }
                          },
                        ),
                        const SizedBox(height: 12),
                        Row(
                          children: [
                            Expanded(
                              child: TextButton(
                                onPressed: () async {
                                  final selected = await showDatePicker(
                                    context: context,
                                    initialDate: _selectedDate,
                                    firstDate: DateTime.now().subtract(
                                      const Duration(days: 30),
                                    ),
                                    lastDate: DateTime.now().add(
                                      const Duration(days: 365),
                                    ),
                                  );
                                  if (selected != null) {
                                    setState(() {
                                      _selectedDate = selected;
                                    });
                                  }
                                },
                                child: Text(
                                  'Tarih: ${_selectedDate.toLocal().toIso8601String().split('T').first}',
                                ),
                              ),
                            ),
                          ],
                        ),
                        const SizedBox(height: 8),
                        Row(
                          children: [
                            Expanded(
                              child: TextButton(
                                onPressed: () async {
                                  final selected = await showTimePicker(
                                    context: context,
                                    initialTime: _selectedStartTime,
                                  );
                                  if (selected != null) {
                                    setState(() {
                                      _selectedStartTime = selected;
                                    });
                                  }
                                },
                                child: Text(
                                  'Başlangıç: ${_selectedStartTime.format(context)}',
                                ),
                              ),
                            ),
                            Expanded(
                              child: TextButton(
                                onPressed: () async {
                                  final selected = await showTimePicker(
                                    context: context,
                                    initialTime: _selectedEndTime,
                                  );
                                  if (selected != null) {
                                    setState(() {
                                      _selectedEndTime = selected;
                                    });
                                  }
                                },
                                child: Text(
                                  'Bitiş: ${_selectedEndTime.format(context)}',
                                ),
                              ),
                            ),
                          ],
                        ),
                        if (_errorMessage != null) ...[
                          const SizedBox(height: 8),
                          Text(
                            _errorMessage!,
                            style: const TextStyle(color: Colors.red),
                          ),
                        ],
                      ],
                    ),
                  ),
                ),
              );
            },
          ),
          actions: [
            TextButton(
              onPressed: () => Navigator.of(dialogContext).pop(),
              child: const Text('İptal'),
            ),
            FilledButton(
              onPressed: _isSubmitting
                  ? null
                  : () async {
                      if (!_createFormKey.currentState!.validate()) return;
                      if (_selectedEndTime.hour < _selectedStartTime.hour ||
                          (_selectedEndTime.hour == _selectedStartTime.hour &&
                              _selectedEndTime.minute <=
                                  _selectedStartTime.minute)) {
                        setState(() {
                          _errorMessage =
                              'Bitiş saati başlangıç saatinden sonra olmalıdır.';
                        });
                        return;
                      }

                      setState(() {
                        _isSubmitting = true;
                        _errorMessage = null;
                      });

                      try {
                        final repository = ref.read(teacherRepositoryProvider);
                        await repository.createClassroomActivity(
                          classroomId: widget.classroom.id,
                          title: _titleController.text.trim(),
                          description: _descriptionController.text.trim(),
                          activityDate: _selectedDate,
                          startTime: _formatTimeOfDay(_selectedStartTime),
                          endTime: _formatTimeOfDay(_selectedEndTime),
                          type: _selectedType,
                        );
                        ref.refresh(
                          classroomActivitiesProvider(widget.classroom.id),
                        );
                        Navigator.of(dialogContext).pop();
                        ScaffoldMessenger.of(context).showSnackBar(
                          const SnackBar(
                            content: Text('Aktivite başarıyla eklendi.'),
                          ),
                        );
                      } catch (error) {
                        setState(() {
                          _errorMessage = error.toString();
                        });
                      } finally {
                        setState(() {
                          _isSubmitting = false;
                        });
                      }
                    },
              child: const Text('Kaydet'),
            ),
          ],
        );
      },
    );
  }

  String _formatTimeOfDay(TimeOfDay time) {
    final hour = time.hour.toString().padLeft(2, '0');
    final minute = time.minute.toString().padLeft(2, '0');
    return '$hour:$minute:00';
  }

  Future<void> _saveMealEntries() async {
    setState(() {
      _isSubmitting = true;
    });

    try {
      final repository = ref.read(teacherRepositoryProvider);
      for (final entry in _mealEntries) {
        for (final meal in entry.meals) {
          final recordId = meal.mealRecordId.isEmpty
              ? '00000000-0000-0000-0000-000000000000'
              : meal.mealRecordId;

          await repository.updateMealRecord(
            studentId: entry.studentId,
            mealRecordId: recordId,
            mealName: meal.mealName,
            status: meal.status,
            notes: meal.notes.trim().isEmpty ? null : meal.notes.trim(),
          );
        }
      }

      ref.refresh(classroomMealRecordsProvider(widget.classroom.id));
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Yemek bilgileri kaydedildi.')),
        );
      }
    } catch (error) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('Yemek kaydı sırasında hata oluştu: $error')),
        );
      }
    } finally {
      if (mounted) {
        setState(() {
          _isSubmitting = false;
        });
      }
    }
  }

  Future<void> _saveMedicationEntries() async {
    setState(() {
      _isSubmitting = true;
    });

    try {
      final repository = ref.read(teacherRepositoryProvider);
      final entriesToSave = _medicationEntries.where(
        (entry) => entry.medicineName.trim().isNotEmpty || entry.dosage.trim().isNotEmpty || entry.time.trim().isNotEmpty || entry.taken,
      );

      if (entriesToSave.isEmpty) {
        if (mounted) {
          ScaffoldMessenger.of(context).showSnackBar(
            const SnackBar(content: Text('Kaydedilecek ilaç bilgisi yok.')),
          );
        }
        return;
      }

      var savedCount = 0;
      for (final entry in entriesToSave) {
        await repository.saveMedicationRecord(
          studentId: entry.studentId,
          medicineName: entry.medicineName.trim(),
          dosage: entry.dosage.trim(),
          time: entry.time.trim(),
          taken: entry.taken,
          note: entry.note.trim().isEmpty ? null : entry.note.trim(),
        );
        savedCount++;
      }

      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('$savedCount ilaç kaydı kaydedildi.')),
        );
      }
    } catch (error) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('İlaç kaydı sırasında hata oluştu: $error')),
        );
      }
    } finally {
      if (mounted) {
        setState(() {
          _isSubmitting = false;
        });
      }
    }
  }

  Widget _buildAIUpdateTab() {
    final theme = Theme.of(context);
    final formattedDate = '${_aiSelectedDate.day} ${_getTurkishMonthName(_aiSelectedDate.month)} ${_aiSelectedDate.year}';

    return SingleChildScrollView(
      physics: const BouncingScrollPhysics(),
      padding: const EdgeInsets.all(16),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          // Bilgi Kartı
          Card(
            shape: RoundedRectangleBorder(
              borderRadius: BorderRadius.circular(16),
            ),
            elevation: 1,
            color: theme.colorScheme.primary.withOpacity(0.04),
            child: Padding(
              padding: const EdgeInsets.all(16.0),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Row(
                    children: [
                      Icon(Icons.auto_awesome_rounded, color: theme.colorScheme.primary, size: 20),
                      const SizedBox(width: 8),
                      Text(
                        'Yapay Zeka Sınıf Güncellemesi',
                        style: TextStyle(
                          fontSize: 16,
                          fontWeight: FontWeight.bold,
                          color: theme.colorScheme.primary,
                        ),
                      ),
                    ],
                  ),
                  const SizedBox(height: 8),
                  const Text(
                    'Öğrencilerinizin uyku, yemek, su tüketimi ve devamsızlık durumlarını tek bir sesli veya yazılı komutla güncelleyebilirsiniz.',
                    style: TextStyle(fontSize: 13, height: 1.4, color: Color(0xFF475569)),
                  ),
                  const SizedBox(height: 8),
                  const Text(
                    'Örnek: "Ali yemeğini bitirdi, Mehmet uyumadı ve Ayşe bugün gelmedi."',
                    style: TextStyle(fontSize: 12, fontStyle: FontStyle.italic, color: Color(0xFF64748B)),
                  ),
                ],
              ),
            ),
          ),
          const SizedBox(height: 16),

          // Tarih Seçici
          Container(
            padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
            decoration: BoxDecoration(
              color: Colors.white,
              borderRadius: BorderRadius.circular(16),
              border: Border.all(color: const Color(0xFFE2E8F0), width: 1),
            ),
            child: Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    const Text(
                      'Hedef Tarih',
                      style: TextStyle(fontSize: 11, color: Color(0xFF64748B), fontWeight: FontWeight.bold),
                    ),
                    const SizedBox(height: 4),
                    Text(
                      formattedDate,
                      style: const TextStyle(fontSize: 15, fontWeight: FontWeight.bold, color: Color(0xFF0F172A)),
                    ),
                  ],
                ),
                TextButton.icon(
                  onPressed: () async {
                    final selected = await showDatePicker(
                      context: context,
                      initialDate: _aiSelectedDate,
                      firstDate: DateTime.now().subtract(const Duration(days: 7)),
                      lastDate: DateTime.now().add(const Duration(days: 1)),
                    );
                    if (selected != null) {
                      setState(() {
                        _aiSelectedDate = selected;
                      });
                    }
                  },
                  icon: const Icon(Icons.calendar_month_rounded, size: 18),
                  label: const Text('Değiştir'),
                ),
              ],
            ),
          ),
          const SizedBox(height: 20),

          // Mikrofon / Ses Tanıma Alanı
          Center(
            child: Column(
              children: [
                GestureDetector(
                  onTap: _toggleListening,
                  child: AnimatedContainer(
                    duration: const Duration(milliseconds: 300),
                    width: 72,
                    height: 72,
                    decoration: BoxDecoration(
                      shape: BoxShape.circle,
                      color: _isListening
                          ? const Color(0xFF10B981)
                          : theme.colorScheme.secondary.withOpacity(0.08),
                      border: Border.all(
                        color: _isListening
                            ? const Color(0xFF34D399)
                            : theme.colorScheme.secondary.withOpacity(0.2),
                        width: _isListening ? 6 : 2,
                      ),
                      boxShadow: _isListening
                          ? [
                              BoxShadow(
                                color: const Color(0xFF10B981).withOpacity(0.4),
                                blurRadius: 20,
                                spreadRadius: 4,
                              )
                            ]
                          : [],
                    ),
                    child: Icon(
                      _isListening ? Icons.mic_rounded : Icons.mic_none_rounded,
                      color: _isListening ? Colors.white : theme.colorScheme.secondary,
                      size: 32,
                    ),
                  ),
                ),
                const SizedBox(height: 12),
                Text(
                  _isListening
                      ? '🎙️ Dinleniyor... Lütfen konuşun'
                      : 'Ses kaydı yapmak için mikrofona dokunun',
                  style: TextStyle(
                    fontSize: 13,
                    fontWeight: FontWeight.w600,
                    color: _isListening ? const Color(0xFF10B981) : const Color(0xFF64748B),
                  ),
                ),
                if (_speechError.isNotEmpty) ...[
                  const SizedBox(height: 8),
                  Text(
                    _speechError,
                    style: const TextStyle(color: Colors.red, fontSize: 12, fontWeight: FontWeight.w500),
                    textAlign: TextAlign.center,
                  ),
                ],
              ],
            ),
          ),
          const SizedBox(height: 24),

          // Metin Alanı
          TextFormField(
            controller: _aiCommandController,
            maxLines: 4,
            decoration: InputDecoration(
              hintText: 'Komutunuzu buraya yazın veya seslendirin...',
              labelText: 'Komut İçeriği',
              border: OutlineInputBorder(
                borderRadius: BorderRadius.circular(16),
              ),
              suffixIcon: IconButton(
                onPressed: () => setState(() => _aiCommandController.clear()),
                icon: const Icon(Icons.clear_rounded),
              ),
            ),
          ),
          const SizedBox(height: 16),

          // Uygula Butonu
          FilledButton.icon(
            onPressed: _aiIsSubmitting ? null : _submitAICommand,
            icon: _aiIsSubmitting
                ? const SizedBox(
                    width: 18,
                    height: 18,
                    child: CircularProgressIndicator(strokeWidth: 2, color: Colors.white),
                  )
                : const Icon(Icons.auto_awesome_rounded),
            label: Text(_aiIsSubmitting ? 'Analiz ediliyor...' : 'Yapay Zeka ile Uygula'),
            style: FilledButton.styleFrom(
              padding: const EdgeInsets.symmetric(vertical: 16),
              shape: RoundedRectangleBorder(
                borderRadius: BorderRadius.circular(16),
              ),
            ),
          ),

          // Güncelleme Raporu Box
          if (_aiSuccessMessage != null || (_aiUpdatedStudents != null && _aiUpdatedStudents!.isNotEmpty)) ...[
            const SizedBox(height: 24),
            Container(
              padding: const EdgeInsets.all(16),
              decoration: BoxDecoration(
                color: Colors.green.shade50,
                borderRadius: BorderRadius.circular(16),
                border: Border.all(color: Colors.green.shade200, width: 1),
              ),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Row(
                    children: [
                      const Icon(Icons.check_circle_rounded, color: Colors.green, size: 20),
                      const SizedBox(width: 8),
                      Expanded(
                        child: Text(
                          _aiSuccessMessage ?? 'Başarıyla uygulandı.',
                          style: const TextStyle(fontWeight: FontWeight.bold, color: Color(0xFF0F5132), fontSize: 14),
                        ),
                      ),
                    ],
                  ),
                  if (_aiUpdatedStudents != null && _aiUpdatedStudents!.isNotEmpty) ...[
                    const SizedBox(height: 12),
                    const Text(
                      'Güncellenen Öğrenciler:',
                      style: TextStyle(fontWeight: FontWeight.bold, fontSize: 13, color: Color(0xFF155724)),
                    ),
                    const SizedBox(height: 6),
                    ..._aiUpdatedStudents!.map(
                      (report) => Padding(
                        padding: const EdgeInsets.only(bottom: 4),
                        child: Row(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            const Text('• ', style: TextStyle(fontWeight: FontWeight.bold, color: Color(0xFF155724))),
                            Expanded(
                              child: Text(
                                report,
                                style: const TextStyle(fontSize: 12, color: Color(0xFF1C7430), height: 1.4),
                              ),
                            ),
                          ],
                        ),
                      ),
                    ),
                  ],
                ],
              ),
            ),
          ],
        ],
      ),
    );
  }

  String _getTurkishMonthName(int month) => const [
        '',
        'Ocak',
        'Şubat',
        'Mart',
        'Nisan',
        'Mayıs',
        'Haziran',
        'Temmuz',
        'Ağustos',
        'Eylül',
        'Ekim',
        'Kasım',
        'Aralık'
      ][month];
}


class _TeacherDailyEntry {
  final String studentId;
  final String firstName;
  final String lastName;
  String? dailyRecordId;
  int sleepStatus;
  String waterInput;
  String teacherNote;
  final bool isAbsent;

  _TeacherDailyEntry({
    required this.studentId,
    required this.firstName,
    required this.lastName,
    this.dailyRecordId,
    this.sleepStatus = 0,
    this.waterInput = '',
    this.teacherNote = '',
    this.isAbsent = false,
  });

  int get waterAmount => int.tryParse(waterInput) ?? 0;
}

class _TeacherMealSubEntry {
  final String mealName;
  String mealRecordId;
  int status;
  String notes;

  _TeacherMealSubEntry({
    required this.mealName,
    this.mealRecordId = '',
    this.status = 0,
    this.notes = '',
  });
}

class _TeacherMealEntry {
  final String studentId;
  final String firstName;
  final String lastName;
  final List<_TeacherMealSubEntry> meals;

  _TeacherMealEntry({
    required this.studentId,
    required this.firstName,
    required this.lastName,
    required this.meals,
  });
}

class _TeacherMedicationEntry {
  final String studentId;
  final String firstName;
  final String lastName;
  String medicineName;
  String dosage;
  String time;
  String note;
  bool taken;

  _TeacherMedicationEntry({
    required this.studentId,
    required this.firstName,
    required this.lastName,
    this.medicineName = '',
    this.dosage = '',
    this.time = '',
    this.note = '',
    this.taken = false,
  });
}
