import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../auth/domain/entities/student.dart';
import '../../../auth/presentation/pages/login_screen.dart';
import '../providers/teacher_providers.dart';
import '../../domain/entities/classroom_activity.dart';
import '../../domain/entities/classroom_daily_record.dart';
import '../../domain/entities/classroom_summary.dart';
import '../../domain/entities/student_meal_record.dart';
import '../../domain/entities/weekly_meal_plan.dart';

class TeacherClassSelectionScreen extends ConsumerWidget {
  const TeacherClassSelectionScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final classroomAsync = ref.watch(teacherClassroomListProvider);

    return Scaffold(
      appBar: AppBar(
        title: const Text('Öğretmen Paneli'),
        actions: [
          IconButton(
            icon: const Icon(Icons.logout),
            tooltip: 'Çıkış Yap',
            onPressed: () => _logout(context),
          ),
        ],
      ),
      body: classroomAsync.when(
        data: (classrooms) => _buildClassroomList(context, classrooms),
        loading: () => const Center(child: CircularProgressIndicator()),
        error: (error, stack) => Center(child: Text('Hata: $error')),
      ),
    );
  }

  void _logout(BuildContext context) {
    Navigator.of(context).pushAndRemoveUntil(
      MaterialPageRoute(builder: (_) => const LoginScreen()),
      (route) => false,
    );
  }

  Widget _buildClassroomList(
    BuildContext context,
    List<ClassroomSummary> classrooms,
  ) {
    if (classrooms.isEmpty) {
      return const Center(child: Text('Atanmış sınıf bulunamadı.'));
    }

    final totalStudents = classrooms.fold<int>(0, (sum, item) => sum + item.studentCount);

    return ListView(
      padding: const EdgeInsets.all(16),
      children: [
        Card(
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(16),
          ),
          elevation: 2,
          child: Padding(
            padding: const EdgeInsets.all(20),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                const Text(
                  'Hoş geldiniz, öğretmen!',
                  style: TextStyle(fontSize: 20, fontWeight: FontWeight.bold),
                ),
                const SizedBox(height: 8),
                const Text(
                  'Bugün sınıflarınızı hızlıca yönetebilir, uyku ve haftalık yemek programlarını düzenleyebilirsiniz.',
                ),
                const SizedBox(height: 16),
                Wrap(
                  spacing: 12,
                  runSpacing: 12,
                  children: [
                    _dashboardMetric('Sınıf', classrooms.length.toString()),
                    _dashboardMetric('Öğrenci', totalStudents.toString()),
                  ],
                ),
              ],
            ),
          ),
        ),
        const SizedBox(height: 16),
        ...classrooms.map((classroom) {
          return Padding(
            padding: const EdgeInsets.only(bottom: 12),
            child: Card(
              shape: RoundedRectangleBorder(
                borderRadius: BorderRadius.circular(16),
              ),
              elevation: 1,
              child: ListTile(
                contentPadding: const EdgeInsets.symmetric(
                  horizontal: 16,
                  vertical: 14,
                ),
                title: Text(
                  classroom.name,
                  style: const TextStyle(fontWeight: FontWeight.bold),
                ),
                subtitle: Text('${classroom.studentCount} öğrenci'),
                trailing: const Icon(Icons.arrow_forward_ios, size: 18),
                onTap: () {
                  Navigator.of(context).push(
                    MaterialPageRoute(
                      builder: (_) =>
                          TeacherClassroomDetailScreen(classroom: classroom),
                    ),
                  );
                },
              ),
            ),
          );
        }).toList(),
      ],
    );
  }

  Widget _dashboardMetric(String label, String value) {
    return Container(
      width: 120,
      padding: const EdgeInsets.all(14),
      decoration: BoxDecoration(
        color: Colors.blue.shade50,
        borderRadius: BorderRadius.circular(14),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(
            value,
            style: const TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
          ),
          const SizedBox(height: 6),
          Text(label, style: const TextStyle(color: Colors.black87)),
        ],
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
  bool _showDailySaveButton = false;
  late final ScrollController _dailyScrollController;
  bool _weeklyPlanEditMode = false;
  List<WeeklyMealPlan> _weeklyPlanEdits = [];

  @override
  void initState() {
    super.initState();
    _tabController = TabController(length: 6, vsync: this);
    _tabController.addListener(() => setState(() {}));
    _dailyScrollController = ScrollController();
    _dailyScrollController.addListener(() {
      final shouldShow = _dailyScrollController.offset > 24;
      if (shouldShow != _showDailySaveButton) {
        setState(() {
          _showDailySaveButton = shouldShow;
        });
      }
    });
  }

  @override
  void dispose() {
    _dailyScrollController.dispose();
    _tabController.dispose();
    _titleController.dispose();
    _descriptionController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
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
        title: Text(widget.classroom.name),
        actions: [
          IconButton(
            icon: const Icon(Icons.logout),
            tooltip: 'Çıkış Yap',
            onPressed: _logout,
          ),
        ],
        bottom: TabBar(
          isScrollable: true,
          controller: _tabController,
          tabs: const [
            Tab(icon: Icon(Icons.people), text: 'Sınıf'),
            Tab(icon: Icon(Icons.schedule), text: 'Günlük'),
            Tab(icon: Icon(Icons.restaurant), text: 'Yemek'),
            Tab(icon: Icon(Icons.medication), text: 'İlaç'),
            Tab(icon: Icon(Icons.calendar_today), text: 'Plan'),
            Tab(icon: Icon(Icons.sports_handball), text: 'Aktiviteler'),
          ],
        ),
      ),
      body: Column(
        children: [
          _buildHeader(),
          Expanded(
            child: TabBarView(
              controller: _tabController,
              children: [
                _buildStudentsTab(studentsAsync),
                _buildDailyTab(dailyAsync),
                _buildMealTab(studentsAsync, mealsAsync),
                _buildMedicationTab(studentsAsync),
                _buildWeeklyMealPlanTab(weeklyPlansAsync),
                _buildActivitiesTab(activitiesAsync),
              ],
            ),
          ),
        ],
      ),
      floatingActionButton: _tabController.index == 5
          ? FloatingActionButton.extended(
              onPressed: _showCreateActivityDialog,
              icon: const Icon(Icons.add),
              label: const Text('Yeni Aktivite'),
            )
          : null,
    );
  }

  void _logout() {
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
      color: Colors.white,
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

  Widget _buildStudentsTab(AsyncValue<List<Student>> studentsAsync) {
    return studentsAsync.when(
      data: (students) {
        if (students.isEmpty) {
          return const Center(child: Text('Sınıfta öğrenci bulunamadı.'));
        }
        return RefreshIndicator(
          onRefresh: () async {
            ref.refresh(classroomStudentsProvider(widget.classroom.id));
          },
          child: ListView.separated(
            padding: const EdgeInsets.all(16),
            itemCount: students.length,
            separatorBuilder: (_, __) => const SizedBox(height: 10),
            itemBuilder: (context, index) {
              final student = students[index];
              return Card(
                child: ListTile(
                  title: Text('${student.firstName} ${student.lastName}'),
                  subtitle: Text(
                    'Doğum: ${student.dateOfBirth.toLocal().toIso8601String().split('T').first}',
                  ),
                ),
              );
            },
          ),
        );
      },
      loading: () => const Center(child: CircularProgressIndicator()),
      error: (error, stack) => Center(child: Text('Hata: $error')),
    );
  }

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
              child: AnimatedOpacity(
                duration: const Duration(milliseconds: 200),
                opacity: _showDailySaveButton ? 1 : 0,
                child: IgnorePointer(
                  ignoring: !_showDailySaveButton,
                  child: FilledButton(
                    onPressed: _isDailySaving ? null : _saveDailyEntries,
                    style: FilledButton.styleFrom(
                      padding: const EdgeInsets.symmetric(vertical: 16),
                      backgroundColor: const Color(0xFF6C5CE7),
                    ),
                    child: Text(_isDailySaving ? 'Kaydediliyor...' : 'Günlük Kaydet'),
                  ),
                ),
              ),
            ),
          ],
        );
      },
      loading: () => const Center(child: CircularProgressIndicator()),
      error: (error, stack) => Center(child: Text('Hata: $error')),
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
                    final meal = records.firstWhere(
                      (record) => record.studentId == student.id,
                      orElse: () => StudentMealRecord(
                        studentId: student.id,
                        firstName: student.firstName,
                        lastName: student.lastName,
                        mealRecordId: null,
                        status: 0,
                        notes: null,
                      ),
                    );
                    _mealEntries.add(
                      _TeacherMealEntry(
                        studentId: student.id,
                        firstName: student.firstName,
                        lastName: student.lastName,
                        mealRecordId: meal.mealRecordId,
                        status: meal.status ?? 0,
                        notes: meal.notes ?? '',
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
                        borderRadius: BorderRadius.circular(14),
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
                              label: Text(_mealStatusLabel(entry.status)),
                              backgroundColor: _mealStatusColor(entry.status).withOpacity(0.16),
                              labelStyle: TextStyle(
                                color: _mealStatusColor(entry.status),
                                fontWeight: FontWeight.w600,
                              ),
                            ),
                          ],
                        ),
                        childrenPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
                        children: [
                          DropdownButtonFormField<int>(
                            value: entry.status,
                            decoration: const InputDecoration(
                              labelText: 'Yemek Durumu',
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
                                entry.status = value;
                              });
                            },
                          ),
                          const SizedBox(height: 12),
                          TextFormField(
                            key: ValueKey('meal-note-${entry.studentId}'),
                            initialValue: entry.notes,
                            decoration: const InputDecoration(
                              labelText: 'Yemek Notu',
                            ),
                            maxLines: 2,
                            onChanged: (value) {
                              entry.notes = value;
                            },
                          ),
                          const SizedBox(height: 12),
                          if (entry.mealRecordId == null)
                            Container(
                              padding: const EdgeInsets.all(12),
                              decoration: BoxDecoration(
                                color: Colors.orange.shade50,
                                borderRadius: BorderRadius.circular(12),
                              ),
                              child: const Text(
                                'Bu öğrenci için yemek kaydı bulunamadı. Önce günlük kaydın oluşturulması gerekebilir.',
                              ),
                            ),
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
                    borderRadius: BorderRadius.circular(14),
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
        if (activities.isEmpty) {
          return const Center(child: Text('Sınıf için aktivite bulunamadı.'));
        }
        return RefreshIndicator(
          onRefresh: () async {
            ref.refresh(classroomActivitiesProvider(widget.classroom.id));
          },
          child: ListView.separated(
            padding: const EdgeInsets.all(16),
            itemCount: activities.length,
            separatorBuilder: (_, __) => const SizedBox(height: 10),
            itemBuilder: (context, index) {
              final activity = activities[index];
              return Card(
                child: ListTile(
                  title: Text(activity.title),
                  subtitle: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(activity.description),
                      const SizedBox(height: 6),
                      Text(
                        '${activity.activityDate.toLocal().toIso8601String().split('T').first} • ${activity.formattedTime}',
                      ),
                      const SizedBox(height: 2),
                      Text(activity.completed ? 'Tamamlandı' : 'Tamamlanmadı'),
                    ],
                  ),
                  trailing: activity.completed
                      ? const Icon(Icons.check_circle, color: Colors.green)
                      : TextButton(
                          onPressed: () => _completeActivity(activity.id),
                          child: const Text('Tamamla'),
                        ),
                ),
              );
            },
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
        entry.status = status;
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

  Future<void> _showCreateActivityDialog() async {
    _errorMessage = null;
    _titleController.clear();
    _descriptionController.clear();
    _selectedDate = DateTime.now();
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
        if (entry.mealRecordId == null) {
          continue;
        }

        await repository.updateMealRecord(
          studentId: entry.studentId,
          mealRecordId: entry.mealRecordId!,
          status: entry.status,
          notes: entry.notes.trim().isEmpty ? null : entry.notes.trim(),
        );
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

class _TeacherMealEntry {
  final String studentId;
  final String firstName;
  final String lastName;
  final String? mealRecordId;
  int status;
  String notes;

  _TeacherMealEntry({
    required this.studentId,
    required this.firstName,
    required this.lastName,
    this.mealRecordId,
    this.status = 0,
    this.notes = '',
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
