import 'dart:convert';
import 'package:flutter/material.dart';
import 'package:http/http.dart' as http;
import '../../../../core/tenant/school_id.dart';
import '../../../auth/domain/entities/student.dart';
import '../../../../core/services/auth_storage_service.dart';
import '../../../auth/presentation/pages/login_screen.dart';
import '../../data/repositories/home_repository_impl.dart';
import '../../domain/usecases/get_home_summary.dart';
import '../../domain/entities/home_summary.dart';
import 'activity_screen.dart';
import 'medication_screen.dart';
import 'meal_screen.dart';
import 'profile_screen.dart';
import 'sleep_screen.dart';
import '../widgets/biometric_dashboard_card.dart';
import 'biometrics_history_screen.dart';
import 'dart:async';
import '../../../../core/services/biometric_signalr_service.dart';


// Ana sayfa ekranı. Login sonrası gösterilir ve okul kimliğine göre özet verilerini yükler.
class HomeScreen extends StatefulWidget {
  final SchoolId schoolId;
  final String? userFirstName;
  final String? userLastName;
  final String? userEmail;
  final List<Student>? students;
  final String token;

  const HomeScreen({
    super.key,
    required this.schoolId,
    required this.token,
    this.userFirstName,
    this.userLastName,
    this.userEmail,
    this.students,
  });

  @override
  State<HomeScreen> createState() => _HomeScreenState();
}

class _HomeScreenState extends State<HomeScreen> {
  String get _token => AuthStorageService.currentToken ?? widget.token;
  late final GetHomeSummary _getHomeSummary;
  int _selectedChildIndex = 0;
  int _currentTabIndex = 0;
  String _classroomName = 'Yükleniyor...';
  List<dynamic> _newsletters = [];
  bool _isClassroomLoading = true;
  bool _isNewslettersLoading = true;
  int _waterIntake = 0;
  String _sleepStatusText = 'Belirtilmemiş';
  String _teacherNote = '';
  List<dynamic> _meals = [];
  bool _isLoadingDailyDetails = true;
  late String _currentFirstName;
  late String _currentLastName;
  late String _currentEmail;
  late String _currentClassroom;
  late String _currentPhone;
  late Future<HomeSummary> _homeSummaryFuture;

  Student? get _selectedChild {
    if (widget.students == null || widget.students!.isEmpty) return null;
    if (_selectedChildIndex < 0 || _selectedChildIndex >= widget.students!.length) {
      return widget.students!.first;
    }
    return widget.students![_selectedChildIndex];
  }

  @override
  void initState() {
    super.initState();
    // Home ekranı açıldığında veri almak için use case oluşturulur.
    _getHomeSummary = GetHomeSummary(HomeRepositoryImpl());
    _homeSummaryFuture = _getHomeSummary(widget.schoolId);
    _currentFirstName = widget.userFirstName ?? '';
    _currentLastName = widget.userLastName ?? '';
    _currentEmail = widget.userEmail ?? '';
    _currentClassroom = '';
    _currentPhone = '';

    // İlk seçili çocuğun verilerini çekelim (Animasyonun bitmesini bekleyerek)
    WidgetsBinding.instance.addPostFrameCallback((_) {
      Future.delayed(const Duration(milliseconds: 400), () {
        if (mounted) {
          final initialChild = _selectedChild;
          if (initialChild != null) {
            _loadAllChildData(initialChild.id, initialChild.classroomId);
          } else {
            setState(() {
              _isClassroomLoading = false;
              _isNewslettersLoading = false;
              _isLoadingDailyDetails = false;
            });
          }
        }
      });
    });
  }

  @override
  void dispose() {
    super.dispose();
  }

  int _calculateAge(DateTime? dob) {
    if (dob == null) return 4;
    final today = DateTime.now();
    int age = today.year - dob.year;
    if (today.month < dob.month || (today.month == dob.month && today.day < dob.day)) {
      age--;
    }
    return age;
  }

  Future<void> _loadAllChildData(String studentId, String classroomId) async {
    _loadDynamicClassroomAndNewsletters(classroomId);
    _loadChildDailyDetails(studentId, classroomId);
    _loadParentProfile();
  }

  Future<void> _loadParentProfile() async {
    try {
      final response = await http.get(
        Uri.parse('https://api.veliport.com.tr/api/users/me'),
        headers: {
          'Content-Type': 'application/json',
          'Authorization': 'Bearer $_token',
        },
      );
      if (response.statusCode == 200) {
        final Map<String, dynamic> data = jsonDecode(response.body) as Map<String, dynamic>;
        final email = (data['email'] ?? data['Email']) as String? ?? '';
        final phone = (data['phoneNumber'] ?? data['PhoneNumber'] ?? data['phone'] ?? data['Phone']) as String? ?? '';
        final classroom = (data['classroom'] ?? data['Classroom']) as String? ?? '';
        if (mounted) {
          setState(() {
            _currentEmail = email.isNotEmpty ? email : _currentEmail;
            _currentPhone = phone;
            _currentClassroom = classroom;
          });
        }
      }
    } catch (_) {}
  }

  Future<void> _loadChildDailyDetails(String studentId, String classroomId) async {
    if (studentId.isEmpty) return;
    
    setState(() {
      _waterIntake = 0;
      _sleepStatusText = 'Belirtilmemiş';
      _teacherNote = '';
      _meals = [];
      _isLoadingDailyDetails = true;
    });

    final String baseUrl = 'https://api.veliport.com.tr';
    final dateStr = DateTime.now().toIso8601String().split('T').first;

    // 1. Günlük Özbakım Kaydını (Su, Uyku, Not) Çek
    try {
      final response = await http.get(
        Uri.parse('$baseUrl/api/daily-records/student/$studentId/today?date=$dateStr'),
        headers: {
          'Content-Type': 'application/json',
          'Authorization': 'Bearer $_token',
        },
      );
      if (response.statusCode == 200) {
        final Map<String, dynamic> data = jsonDecode(response.body) as Map<String, dynamic>;
        
        final water = (data['waterIntake'] ?? data['WaterIntake']) as num?;
        final sleep = (data['sleepStatus'] ?? data['SleepStatus'])?.toString() ?? '';
        final note = (data['teacherNote'] ?? data['TeacherNote']) as String?;

        String sleepText = 'Belirtilmemiş';
        if (sleep.toLowerCase().contains('well') || sleep == '2') {
          sleepText = 'Düzenli Uyudu';
        } else if (sleep.toLowerCase().contains('little') || sleep == '1') {
          sleepText = 'Az Uyudu';
        } else if (sleep.toLowerCase().contains('not') || sleep == '0') {
          sleepText = 'Uymadı';
        }

        if (mounted) {
          setState(() {
            _waterIntake = water?.toInt() ?? 0;
            _sleepStatusText = sleepText;
            _teacherNote = note ?? '';
          });
        }
      }
    } catch (_) {}

    // 2. Yemek Kayıtlarını Çek
    if (classroomId.isNotEmpty) {
      try {
        final response = await http.get(
          Uri.parse('$baseUrl/api/classrooms/$classroomId/meal-records/detailed?date=$dateStr'),
          headers: {
            'Content-Type': 'application/json',
            'Authorization': 'Bearer $_token',
          },
        );
        if (response.statusCode == 200) {
          final List<dynamic> allClassroomMeals = jsonDecode(response.body) as List<dynamic>;
          // Selected student meals bul
          final studentMealObj = allClassroomMeals.firstWhere(
            (m) => (m['studentId'] ?? m['StudentId'])?.toString() == studentId,
            orElse: () => null,
          );

          if (studentMealObj != null && studentMealObj['meals'] != null) {
            if (mounted) {
              setState(() {
                _meals = studentMealObj['meals'] as List<dynamic>;
              });
            }
          }
        }
      } catch (_) {}
    }

    if (mounted) {
      setState(() {
        _isLoadingDailyDetails = false;
      });
    }
  }

  Future<void> _loadDynamicClassroomAndNewsletters(String classroomId) async {
    if (classroomId.isEmpty) return;
    
    setState(() {
      _classroomName = 'Yükleniyor...';
      _newsletters = [];
      _isClassroomLoading = true;
      _isNewslettersLoading = true;
    });

    final String baseUrl = 'https://api.veliport.com.tr';

    // 1. Sınıf Detayını Çek
    try {
      final response = await http.get(
        Uri.parse('$baseUrl/api/classrooms/$classroomId'),
        headers: {
          'Content-Type': 'application/json',
          'Authorization': 'Bearer $_token',
        },
      );
      if (response.statusCode == 200) {
        final Map<String, dynamic> data = jsonDecode(response.body) as Map<String, dynamic>;
        final String? name = data['name'] as String? ?? data['Name'] as String?;
        if (mounted) {
          setState(() {
            _classroomName = name ?? 'Bilinmeyen Sınıf';
            _isClassroomLoading = false;
          });
        }
      } else {
        if (mounted) {
          setState(() {
            _classroomName = 'Sınıf Bilgisi Alınamadı';
            _isClassroomLoading = false;
          });
        }
      }
    } catch (_) {
      if (mounted) {
        setState(() {
          _classroomName = 'Bağlantı Hatası';
          _isClassroomLoading = false;
        });
      }
    }

    // 2. Sınıf Bültenlerini/Duyurularını Çek
    try {
      final response = await http.get(
        Uri.parse('$baseUrl/api/newsletters/classroom/$classroomId'),
        headers: {
          'Content-Type': 'application/json',
          'Authorization': 'Bearer $_token',
        },
      );
      if (response.statusCode == 200) {
        final List<dynamic> data = jsonDecode(response.body) as List<dynamic>;
        if (mounted) {
          setState(() {
            _newsletters = data;
            _isNewslettersLoading = false;
          });
        }
      } else {
        if (mounted) {
          setState(() {
            _isNewslettersLoading = false;
          });
        }
      }
    } catch (_) {
      if (mounted) {
        setState(() {
          _isNewslettersLoading = false;
        });
      }
    }
  }

  Future<void> _openProfileScreen() async {
    final updatedData = await Navigator.push<ProfileData>(
      context,
      MaterialPageRoute(
        builder: (_) => ProfileScreen(
          initialData: ProfileData(
            fullName: '$_currentFirstName $_currentLastName'.trim(),
            email: _currentEmail,
            classroom: _currentClassroom,
            phone: _currentPhone,
          ),
        ),
      ),
    );

    if (updatedData == null) return;

    setState(() {
      final fullName = updatedData.fullName.trim();
      if (fullName.isNotEmpty) {
        final parts = fullName.split(' ');
        _currentFirstName = parts.first;
        _currentLastName = parts.length > 1 ? parts.sublist(1).join(' ') : _currentLastName;
      }
      _currentEmail = updatedData.email;
      _currentClassroom = updatedData.classroom;
      _currentPhone = updatedData.phone;
    });
  }

  Widget _buildBodyForTab(HomeSummary summary, Student? selectedChild) {
    switch (_currentTabIndex) {
      case 0:
        return _buildHomeTabContent(summary, selectedChild);
      case 1:
        if (selectedChild == null) return const Center(child: Text('Çocuk seçilmedi'));
        return MealScreen(
          classroomId: selectedChild.classroomId,
          token: _token,
          isEmbedded: true,
        );
      case 2:
        if (selectedChild == null) return const Center(child: Text('Çocuk seçilmedi'));
        return ActivityScreen(
          classroomId: selectedChild.classroomId,
          token: _token,
          isEmbedded: true,
        );
      case 3:
        return const MedicationScreen(
          isEmbedded: true,
        );
      case 4:
        return _buildAnnouncementsTabContent();
      default:
        return const SizedBox.shrink();
    }
  }

  Widget _buildHomeTabContent(HomeSummary summary, Student? selectedChild) {
    return SafeArea(
      bottom: false,
      child: RefreshIndicator(
        onRefresh: () async {
          setState(() {});
          final child = _selectedChild;
          if (child != null) {
            await _loadAllChildData(child.id, child.classroomId);
          }
        },
        child: SingleChildScrollView(
          physics: const AlwaysScrollableScrollPhysics(),
          padding: const EdgeInsets.only(bottom: 100),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              _buildHeader(summary, selectedChild),
              Padding(
                padding: const EdgeInsets.symmetric(horizontal: 20),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    if (widget.students != null && widget.students!.isNotEmpty) ...[
                      _buildChildrenSection(widget.students!, _classroomName),
                      const SizedBox(height: 20),
                    ],
                    
                    if (selectedChild != null) ...[
                      _buildChildHeader(selectedChild, _classroomName),
                      const SizedBox(height: 16),

                      // Canlı Biyometrik Takip Modülü (EKG)
                      BiometricDashboardCard(
                        studentId: selectedChild.id,
                        studentName: '${selectedChild.firstName} ${selectedChild.lastName}',
                        studentAge: _calculateAge(selectedChild.dateOfBirth),
                        token: _token,
                      ),
                      const SizedBox(height: 22),

                      // Günlük Özbakım, Beslenme ve AI Özet Raporu
                      _buildDailyDevelopmentSection(selectedChild),
                      const SizedBox(height: 24),
                    ],
                  ],
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }

  Widget _buildAnnouncementsTabContent() {
    final theme = Theme.of(context);
    
    String formatNewsletterDate(String? dateStr) {
      if (dateStr == null) return '';
      try {
        final dt = DateTime.parse(dateStr).toLocal();
        final today = DateTime.now();
        if (dt.year == today.year && dt.month == today.month && dt.day == today.day) {
          return 'Bugün, ${dt.hour.toString().padLeft(2, '0')}:${dt.minute.toString().padLeft(2, '0')}';
        }
        final diff = today.difference(dt).inDays;
        if (diff == 1) {
          return 'Dün, ${dt.hour.toString().padLeft(2, '0')}:${dt.minute.toString().padLeft(2, '0')}';
        }
        return '${dt.day}/${dt.month}/${dt.year}';
      } catch (_) {
        return '';
      }
    }

    bool isNewsletterNew(String? dateStr) {
      if (dateStr == null) return false;
      try {
        final dt = DateTime.parse(dateStr).toLocal();
        return DateTime.now().difference(dt).inHours < 24;
      } catch (_) {
        return false;
      }
    }

    return Scaffold(
      backgroundColor: const Color(0xFFF8FAFC),
      appBar: AppBar(
        automaticallyImplyLeading: false,
        backgroundColor: Colors.white,
        foregroundColor: const Color(0xFF1E293B),
        elevation: 0,
        title: const Text(
          'Duyurular & Haberler',
          style: TextStyle(fontWeight: FontWeight.w800, fontSize: 18),
        ),
      ),
      body: _isNewslettersLoading
          ? const Center(
              child: Padding(
                padding: EdgeInsets.all(20.0),
                child: CircularProgressIndicator(color: Color(0xFF6366F1)),
              ),
            )
          : _newsletters.isEmpty
              ? Center(
                  child: Padding(
                    padding: const EdgeInsets.all(24.0),
                    child: Column(
                      mainAxisAlignment: MainAxisAlignment.center,
                      children: [
                        Icon(Icons.notifications_none_rounded, size: 48, color: Colors.grey[400]),
                        const SizedBox(height: 12),
                        const Text(
                          'Sınıfa ait güncel bülten veya duyuru bulunmamaktadır.',
                          style: TextStyle(fontSize: 14, color: Color(0xFF64748B), fontWeight: FontWeight.w500),
                          textAlign: TextAlign.center,
                        ),
                      ],
                    ),
                  ),
                )
              : RefreshIndicator(
                  onRefresh: () async {
                    final child = _selectedChild;
                    if (child != null) {
                      await _loadDynamicClassroomAndNewsletters(child.classroomId);
                    }
                  },
                  child: ListView.separated(
                    padding: const EdgeInsets.fromLTRB(20, 20, 20, 100),
                    itemCount: _newsletters.length,
                    separatorBuilder: (_, __) => const SizedBox(height: 12),
                    itemBuilder: (context, index) {
                      final item = _newsletters[index] as Map<String, dynamic>;
                      
                      final String title = item['title'] as String? ?? item['Title'] as String? ?? 'Duyuru';
                      final String body = item['content'] as String? ?? item['Content'] as String? ?? '';
                      final String publishedAtStr = item['publishedAt'] as String? ?? item['PublishedAt'] as String? ?? item['createdAt'] as String? ?? item['CreatedAt'] as String? ?? '';
                      final String category = item['weekName'] as String? ?? item['WeekName'] as String? ?? 'Duyuru';
                      
                      final bool isNew = isNewsletterNew(publishedAtStr);
                      final String formattedDate = formatNewsletterDate(publishedAtStr);
                      
                      final IconData icon = Icons.newspaper_rounded;
                      final Color iconColor = theme.colorScheme.primary;

                      return Container(
                        decoration: BoxDecoration(
                          color: Colors.white,
                          borderRadius: BorderRadius.circular(20),
                          border: Border.all(
                            color: isNew ? theme.colorScheme.secondary.withOpacity(0.2) : const Color(0xFFE2E8F0),
                            width: isNew ? 1.2 : 1,
                          ),
                          boxShadow: const [
                            BoxShadow(
                              color: Color.fromRGBO(15, 23, 42, 0.02),
                              blurRadius: 16,
                              offset: Offset(0, 6),
                            ),
                          ],
                        ),
                        padding: const EdgeInsets.all(16),
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Row(
                              crossAxisAlignment: CrossAxisAlignment.center,
                              children: [
                                Container(
                                  padding: const EdgeInsets.all(6),
                                  decoration: BoxDecoration(
                                    color: iconColor.withOpacity(0.06),
                                    borderRadius: BorderRadius.circular(10),
                                  ),
                                  child: Icon(icon, color: iconColor, size: 18),
                                ),
                                const SizedBox(width: 10),
                                Expanded(
                                  child: Column(
                                    crossAxisAlignment: CrossAxisAlignment.start,
                                    children: [
                                      Row(
                                        children: [
                                          Container(
                                            padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 3),
                                            decoration: BoxDecoration(
                                              color: isNew 
                                                  ? theme.colorScheme.secondary.withOpacity(0.1)
                                                  : const Color(0xFFF1F5F9),
                                              borderRadius: BorderRadius.circular(8),
                                            ),
                                            child: Text(
                                              category,
                                              style: TextStyle(
                                                color: isNew ? theme.colorScheme.secondary : const Color(0xFF64748B),
                                                fontSize: 10,
                                                fontWeight: FontWeight.bold,
                                              ),
                                            ),
                                          ),
                                          if (isNew) ...[
                                            const SizedBox(width: 6),
                                            Container(
                                              padding: const EdgeInsets.symmetric(horizontal: 5, vertical: 2),
                                              decoration: BoxDecoration(
                                                color: const Color(0xFFEF4444),
                                                borderRadius: BorderRadius.circular(6),
                                              ),
                                              child: const Text(
                                                'YENİ',
                                                style: TextStyle(
                                                  color: Colors.white,
                                                  fontSize: 8,
                                                  fontWeight: FontWeight.w800,
                                                  letterSpacing: 0.5,
                                                ),
                                              ),
                                            ),
                                          ],
                                        ],
                                      ),
                                      const SizedBox(height: 2),
                                      Text(
                                        formattedDate,
                                        style: const TextStyle(
                                          color: Color(0xFF94A3B8),
                                          fontSize: 10,
                                          fontWeight: FontWeight.w600,
                                        ),
                                      ),
                                    ],
                                  ),
                                ),
                              ],
                            ),
                            const SizedBox(height: 12),
                            Text(
                              title,
                              style: const TextStyle(
                                fontSize: 15,
                                fontWeight: FontWeight.w800,
                                color: Color(0xFF0F172A),
                                letterSpacing: -0.3,
                              ),
                            ),
                            const SizedBox(height: 4),
                            Text(
                              body,
                              style: const TextStyle(
                                fontSize: 12,
                                color: Color(0xFF475569),
                                height: 1.5,
                                fontWeight: FontWeight.w500,
                              ),
                            ),
                          ],
                        ),
                      );
                    },
                  ),
                ),
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
          height: 68,
          decoration: BoxDecoration(
            color: theme.colorScheme.surface,
            borderRadius: BorderRadius.circular(24),
            boxShadow: [
              BoxShadow(
                color: theme.brightness == Brightness.dark
                    ? Colors.black.withOpacity(0.35)
                    : Colors.black.withOpacity(0.10),
                blurRadius: 22,
                offset: const Offset(0, 8),
              ),
            ],
          ),
          child: Row(
            mainAxisAlignment: MainAxisAlignment.spaceEvenly,
            children: [
              _buildNavItem(context, icon: Icons.home_rounded, index: 0, label: 'Ana Sayfa', activeColor: activeColor, inactiveColor: inactiveColor),
              _buildNavItem(context, icon: Icons.restaurant_rounded, index: 1, label: 'Yemek', activeColor: activeColor, inactiveColor: inactiveColor),
              _buildNavItem(context, icon: Icons.sports_basketball_rounded, index: 2, label: 'Aktivite', activeColor: activeColor, inactiveColor: inactiveColor),
              _buildNavItem(context, icon: Icons.medication_rounded, index: 3, label: 'İlaç', activeColor: activeColor, inactiveColor: inactiveColor),
              _buildNavItem(context, icon: Icons.notifications_rounded, index: 4, label: 'Duyurular', activeColor: activeColor, inactiveColor: inactiveColor),
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
    final isSelected = _currentTabIndex == index;
    return GestureDetector(
      onTap: () {
        setState(() {
          _currentTabIndex = index;
        });
      },
      child: AnimatedContainer(
        duration: const Duration(milliseconds: 200),
        padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
        decoration: BoxDecoration(
          color: isSelected ? activeColor.withOpacity(0.12) : Colors.transparent,
          borderRadius: BorderRadius.circular(16),
        ),
        child: Row(
          mainAxisSize: MainAxisSize.min,
          children: [
            Icon(
              icon,
              color: isSelected ? activeColor : inactiveColor,
              size: 22,
            ),
            if (isSelected) ...[
              const SizedBox(width: 6),
              Text(
                label,
                style: TextStyle(
                  color: activeColor,
                  fontWeight: FontWeight.bold,
                  fontSize: 12,
                ),
              ),
            ],
          ],
        ),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return FutureBuilder<HomeSummary>(
      future: _homeSummaryFuture,
      builder: (context, snapshot) {
        if (snapshot.connectionState == ConnectionState.waiting) {
          return const Scaffold(
            backgroundColor: Color(0xFFF8FAFC),
            body: Center(child: CircularProgressIndicator()),
          );
        }

        if (snapshot.hasError) {
          return Scaffold(
            backgroundColor: const Color(0xFFF8FAFC),
            body: Center(
              child: Padding(
                padding: const EdgeInsets.all(24.0),
                child: Text(
                  'Veri yüklenemedi: ${snapshot.error}',
                  style: const TextStyle(color: Color(0xFFEF4444), fontWeight: FontWeight.bold),
                ),
              ),
            ),
          );
        }

        final summary = snapshot.data!;
        final selectedChild = _selectedChild;

        return Scaffold(
          backgroundColor: const Color(0xFFF8FAFC),
          body: _buildBodyForTab(summary, selectedChild),
          bottomNavigationBar: _buildBottomNavigationBar(context),
        );
      },
    );
  }

  void _logout() async {
    await AuthStorageService.clear();
    if (!mounted) return;
    Navigator.pushAndRemoveUntil(
      context,
      MaterialPageRoute(builder: (_) => const LoginScreen()),
      (route) => false,
    );
  }

  String get _parentFirstName {
    final value = _currentFirstName.trim();
    if (value.isEmpty) return 'Veli';

    final lowerValue = value.toLowerCase();
    final children = widget.students ?? [];
    if (children.any((child) => child.firstName.toLowerCase() == lowerValue)) {
      return 'Veli';
    }

    return value.split(' ').first;
  }

  Widget _buildHeader(HomeSummary summary, Student? selectedChild) {
    final theme = Theme.of(context);
    
    return Container(
      width: double.infinity,
      color: Colors.white,
      padding: const EdgeInsets.fromLTRB(24, 20, 24, 16),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            crossAxisAlignment: CrossAxisAlignment.center,
            children: [
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      selectedChild != null ? selectedChild.schoolName.toUpperCase() : 'VELİPORTAL',
                      style: TextStyle(
                        color: theme.colorScheme.primary,
                        fontSize: 10,
                        fontWeight: FontWeight.w800,
                        letterSpacing: 1.5,
                      ),
                    ),
                    const SizedBox(height: 4),
                    Text(
                      'Merhaba, $_parentFirstName 👋',
                      style: const TextStyle(
                        color: Color(0xFF0F172A),
                        fontSize: 24,
                        fontWeight: FontWeight.w800,
                        letterSpacing: -0.5,
                      ),
                    ),
                  ],
                ),
              ),
              const SizedBox(width: 12),
              
              // Profile Action Group
              Container(
                padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 6),
                decoration: BoxDecoration(
                  color: const Color(0xFFF1F5F9),
                  borderRadius: BorderRadius.circular(20),
                ),
                child: Row(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    InkWell(
                      borderRadius: BorderRadius.circular(32),
                      onTap: _openProfileScreen,
                      child: Container(
                        width: 32,
                        height: 32,
                        decoration: BoxDecoration(
                          shape: BoxShape.circle,
                          border: Border.all(color: Colors.white, width: 1.5),
                        ),
                        child: CircleAvatar(
                          radius: 16,
                          backgroundColor: Colors.white,
                          child: Icon(Icons.face_rounded, size: 20, color: theme.colorScheme.primary),
                        ),
                      ),
                    ),
                    const SizedBox(width: 6),
                    Container(
                      width: 28,
                      height: 28,
                      decoration: BoxDecoration(
                        color: Colors.white,
                        borderRadius: BorderRadius.circular(10),
                      ),
                      child: IconButton(
                        onPressed: _logout,
                        icon: const Icon(Icons.logout_rounded, size: 14, color: Color(0xFF64748B)),
                        tooltip: 'Çıkış',
                        padding: EdgeInsets.zero,
                        splashRadius: 18,
                      ),
                    ),
                  ],
                ),
              ),
            ],
          ),
        ],
      ),
    );
  }

  Widget _buildChildrenSection(List<Student> students, String className) {
    final theme = Theme.of(context);
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        const Text(
          'Çocuklarınız',
          style: TextStyle(
            fontSize: 15,
            fontWeight: FontWeight.w800,
            color: Color(0xFF0F172A),
          ),
        ),
        const SizedBox(height: 10),
        SizedBox(
          height: 82,
          child: ListView.separated(
            physics: const BouncingScrollPhysics(),
            scrollDirection: Axis.horizontal,
            itemCount: students.length,
            separatorBuilder: (context, index) => const SizedBox(width: 14),
            itemBuilder: (context, index) {
              final s = students[index];
              final isSelected = index == _selectedChildIndex;
              final initials = s.firstName.isNotEmpty ? s.firstName[0] : '';
              
              return GestureDetector(
                onTap: () {
                  if (_selectedChildIndex == index) return;
                  setState(() {
                    _selectedChildIndex = index;
                  });
                  _loadAllChildData(s.id, s.classroomId);
                },
                child: Column(
                  children: [
                    AnimatedContainer(
                      duration: const Duration(milliseconds: 200),
                      width: 52,
                      height: 52,
                      decoration: BoxDecoration(
                        shape: BoxShape.circle,
                        color: isSelected ? theme.colorScheme.primary : Colors.white,
                        border: Border.all(
                          color: isSelected ? theme.colorScheme.primary : const Color(0xFFE2E8F0),
                          width: isSelected ? 2 : 1.2,
                        ),
                        boxShadow: [
                          BoxShadow(
                            color: isSelected
                                ? theme.colorScheme.primary.withOpacity(0.12)
                                : const Color.fromRGBO(15, 23, 42, 0.02),
                            blurRadius: 8,
                            offset: const Offset(0, 3),
                          ),
                        ],
                      ),
                      child: Center(
                        child: Text(
                          initials,
                          style: TextStyle(
                            color: isSelected ? Colors.white : theme.colorScheme.primary,
                            fontWeight: FontWeight.bold,
                            fontSize: 16,
                          ),
                        ),
                      ),
                    ),
                    const SizedBox(height: 6),
                    Text(
                      s.firstName,
                      style: TextStyle(
                        fontSize: 11,
                        fontWeight: isSelected ? FontWeight.bold : FontWeight.w500,
                        color: isSelected ? theme.colorScheme.primary : const Color(0xFF475569),
                      ),
                    ),
                  ],
                ),
              );
            },
          ),
        ),
      ],
    );
  }

  // Visualizers for At a Glance section
  double _parseSleepProgress(String status) {
    final s = status.toLowerCase();
    if (s.contains('tamam') || s.contains('düzenli') || s.contains('iyi')) return 1.0;
    if (s.contains('orta') || s.contains('az')) return 0.4;
    return 0.8; 
  }

  String _getSleepLabel(String status) {
    final s = status.toLowerCase();
    if (s.contains('düzenli') || s.contains('iyi')) return '2.5 Sa / 2.5 Sa';
    if (s.contains('az')) return '1.0 Sa / 2.5 Sa';
    return '2.0 Sa / 2.5 Sa';
  }

  int _parseMealSegments(String status) {
    final s = status.toLowerCase();
    if (s.contains('tamam') || s.contains('hepsi')) return 3;
    if (s.contains('orta') || s.contains('yarım') || s.contains('kısmen')) return 2;
    if (s.contains('az') || s.contains('yemedi')) return 1;
    return 3;
  }

  String _getMealLabel(String status) {
    final s = status.toLowerCase();
    if (s.contains('tamam')) return '3 / 3 Öğün';
    if (s.contains('yarım') || s.contains('orta')) return '2 / 3 Öğün';
    return '3 / 3 Öğün';
  }

  double _parseActivityProgress(String status) {
    final s = status.toLowerCase();
    if (s.contains('zamanı') || s.contains('aktif') || s.contains('iyi')) return 0.75;
    if (s.contains('sakin') || s.contains('az')) return 0.35;
    return 0.6;
  }

  String _getActivityLabel(String status) {
    final s = status.toLowerCase();
    if (s.contains('zamanı') || s.contains('aktif')) return '45 / 60 dk';
    if (s.contains('sakin')) return '20 / 60 dk';
    return '35 / 60 dk';
  }

  Widget _buildAtAGlanceSection(HomeSummary summary) {
    final sleepVal = _parseSleepProgress(summary.sleepStatus);
    final sleepLbl = _getSleepLabel(summary.sleepStatus);
    final mealVal = _parseMealSegments(summary.mealStatus);
    final mealLbl = _getMealLabel(summary.mealStatus);
    final actVal = _parseActivityProgress(summary.activityStatus);
    final actLbl = _getActivityLabel(summary.activityStatus);

    return LayoutBuilder(
      builder: (context, constraints) {
        final cardWidth = (constraints.maxWidth - 20) / 3;
        final selectedChild = _selectedChild;
        return Row(
          mainAxisAlignment: MainAxisAlignment.spaceBetween,
          children: [
            SizedBox(
              width: cardWidth,
              child: _buildVisualStatCard(
                title: 'Uyku',
                icon: Icons.bedtime_rounded,
                accentColor: const Color(0xFFF59E0B),
                bgColor: const Color(0xFFFEF3C7),
                statusLabel: summary.sleepStatus,
                detailLabel: sleepLbl,
                onTap: () {
                  Navigator.push(
                    context,
                    MaterialPageRoute(builder: (_) => const SleepScreen()),
                  );
                },
                visual: SizedBox(
                  width: 38,
                  height: 38,
                  child: CircularProgressIndicator(
                    value: sleepVal,
                    strokeWidth: 4,
                    color: const Color(0xFFF59E0B),
                    backgroundColor: const Color(0xFFFEF3C7),
                    strokeCap: StrokeCap.round,
                  ),
                ),
              ),
            ),
            SizedBox(
              width: cardWidth,
              child: _buildVisualStatCard(
                title: 'Yemek',
                icon: Icons.restaurant_rounded,
                accentColor: const Color(0xFFF97316),
                bgColor: const Color(0xFFFFE4E6),
                statusLabel: summary.mealStatus,
                detailLabel: mealLbl,
                onTap: () {
                  if (selectedChild != null) {
                    Navigator.push(
                      context,
                      MaterialPageRoute(
                        builder: (_) => MealScreen(
                          classroomId: selectedChild.classroomId,
                          token: _token,
                        ),
                      ),
                    );
                  }
                },
                visual: Column(
                  mainAxisAlignment: MainAxisAlignment.center,
                  children: [
                    Row(
                      children: List.generate(3, (i) {
                        final isFilled = i < mealVal;
                        return Expanded(
                          child: Container(
                            height: 6,
                            margin: EdgeInsets.only(right: i < 2 ? 3 : 0),
                            decoration: BoxDecoration(
                              color: isFilled ? const Color(0xFFF97316) : const Color(0xFFFFE4E6),
                              borderRadius: BorderRadius.circular(3),
                            ),
                          ),
                        );
                      }),
                    ),
                    const SizedBox(height: 6),
                    Row(
                      mainAxisAlignment: MainAxisAlignment.spaceBetween,
                      children: const [
                        Text('K', style: TextStyle(fontSize: 8, color: Color(0xFF94A3B8), fontWeight: FontWeight.bold)),
                        Text('Ö', style: TextStyle(fontSize: 8, color: Color(0xFF94A3B8), fontWeight: FontWeight.bold)),
                        Text('İ', style: TextStyle(fontSize: 8, color: Color(0xFF94A3B8), fontWeight: FontWeight.bold)),
                      ],
                    ),
                  ],
                ),
              ),
            ),
            SizedBox(
              width: cardWidth,
              child: _buildVisualStatCard(
                title: 'Aktivite',
                icon: Icons.sports_tennis_rounded,
                accentColor: const Color(0xFF10B981),
                bgColor: const Color(0xFFD1FAE5),
                statusLabel: summary.activityStatus,
                detailLabel: actLbl,
                onTap: () {
                  if (selectedChild != null) {
                    Navigator.push(
                      context,
                      MaterialPageRoute(
                        builder: (_) => ActivityScreen(
                          classroomId: selectedChild.classroomId,
                          token: _token,
                        ),
                      ),
                    );
                  }
                },
                visual: SizedBox(
                  width: 38,
                  height: 38,
                  child: CircularProgressIndicator(
                    value: actVal,
                    strokeWidth: 4,
                    color: const Color(0xFF10B981),
                    backgroundColor: const Color(0xFFD1FAE5),
                    strokeCap: StrokeCap.round,
                  ),
                ),
              ),
            ),
          ],
        );
      },
    );
  }

  Widget _buildVisualStatCard({
    required String title,
    required IconData icon,
    required Color accentColor,
    required Color bgColor,
    required Widget visual,
    required String statusLabel,
    required String detailLabel,
    VoidCallback? onTap,
  }) {
    final cardContent = Container(
      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 14),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(20),
        border: Border.all(
          color: const Color(0xFFE2E8F0),
          width: 1,
        ),
        boxShadow: const [
          BoxShadow(
            color: Color.fromRGBO(15, 23, 42, 0.02),
            blurRadius: 16,
            offset: Offset(0, 8),
          ),
        ],
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              Container(
                padding: const EdgeInsets.all(5),
                decoration: BoxDecoration(
                  color: bgColor,
                  borderRadius: BorderRadius.circular(8),
                ),
                child: Icon(icon, color: accentColor, size: 16),
              ),
              Text(
                title,
                style: const TextStyle(
                  color: Color(0xFF64748B),
                  fontSize: 11,
                  fontWeight: FontWeight.w800,
                ),
              ),
            ],
          ),
          const SizedBox(height: 14),
          Center(
            child: SizedBox(
              height: 40,
              child: visual,
            ),
          ),
          const SizedBox(height: 12),
          Text(
            statusLabel,
            style: const TextStyle(
              color: Color(0xFF0F172A),
              fontSize: 12,
              fontWeight: FontWeight.bold,
            ),
            maxLines: 1,
            overflow: TextOverflow.ellipsis,
          ),
          const SizedBox(height: 2),
          Text(
            detailLabel,
            style: const TextStyle(
              color: Color(0xFF94A3B8),
              fontSize: 10,
              fontWeight: FontWeight.w600,
            ),
            maxLines: 1,
            overflow: TextOverflow.ellipsis,
          ),
        ],
      ),
    );

    if (onTap != null) {
      return GestureDetector(
        onTap: onTap,
        child: cardContent,
      );
    }
    return cardContent;
  }

  Widget _buildSectionTitle(String title, String subtitle, {VoidCallback? onViewAll}) {
    return Row(
      mainAxisAlignment: MainAxisAlignment.spaceBetween,
      children: [
        Expanded(
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                title,
                style: const TextStyle(
                  fontSize: 16,
                  fontWeight: FontWeight.w800,
                  color: Color(0xFF0F172A),
                  letterSpacing: -0.4,
                ),
              ),
              const SizedBox(height: 2),
              Text(
                subtitle,
                style: const TextStyle(
                  fontSize: 12,
                  color: Color(0xFF64748B),
                  fontWeight: FontWeight.w500,
                ),
              ),
            ],
          ),
        ),
        if (onViewAll != null)
          TextButton(
            onPressed: onViewAll,
            style: TextButton.styleFrom(
              padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
              minimumSize: Size.zero,
              tapTargetSize: MaterialTapTargetSize.shrinkWrap,
            ),
            child: const Text('Tümünü Gör', style: TextStyle(fontSize: 12, fontWeight: FontWeight.bold)),
          ),
      ],
    );
  }

  Widget _buildQuickActionsRow(BuildContext context, Student? selectedChild) {
    final theme = Theme.of(context);
    return SizedBox(
      height: 40,
      child: Row(
        children: [
          if (selectedChild != null) ...[
            Expanded(
              child: _buildQuickActionChip(
                context,
                icon: Icons.analytics_rounded,
                label: 'Sağlık Geçmişi',
                color: const Color(0xFF6366F1),
                onTap: () => Navigator.push(
                  context,
                  MaterialPageRoute(
                    builder: (_) => BiometricsHistoryScreen(
                      studentId: selectedChild.id,
                      studentName: '${selectedChild.firstName} ${selectedChild.lastName}',
                      token: _token,
                      studentAge: _calculateAge(selectedChild.dateOfBirth),
                    ),
                  ),
                ),
              ),
            ),
            const SizedBox(width: 10),
          ],
          Expanded(
            child: _buildQuickActionChip(
              context,
              icon: Icons.person_rounded,
              label: 'Veli Profili',
              color: theme.colorScheme.primary,
              onTap: _openProfileScreen,
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildQuickActionChip(
    BuildContext context, {
    required IconData icon,
    required String label,
    required Color color,
    required VoidCallback onTap,
  }) {
    return Container(
      decoration: BoxDecoration(
        color: color.withOpacity(0.06),
        borderRadius: BorderRadius.circular(12),
        border: Border.all(
          color: color.withOpacity(0.12),
          width: 1,
        ),
      ),
      child: Material(
        color: Colors.transparent,
        child: InkWell(
          onTap: onTap,
          borderRadius: BorderRadius.circular(12),
          child: Padding(
            padding: const EdgeInsets.symmetric(horizontal: 14),
            child: Row(
              mainAxisSize: MainAxisSize.min,
              children: [
                Icon(icon, color: color, size: 16),
                const SizedBox(width: 8),
                Text(
                  label,
                  style: TextStyle(
                    color: color,
                    fontWeight: FontWeight.bold,
                    fontSize: 12,
                  ),
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }

  Widget _buildChildHeader(Student child, String className) {
    final theme = Theme.of(context);
    final dob = child.dateOfBirth == DateTime.fromMillisecondsSinceEpoch(0)
        ? '—'
        : '${child.dateOfBirth.day}.${child.dateOfBirth.month}.${child.dateOfBirth.year}';
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 4),
      child: Row(
        children: [
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Row(
                  children: [
                    Text(
                      '${child.firstName} ${child.lastName}',
                      style: const TextStyle(
                        fontSize: 20,
                        fontWeight: FontWeight.w900,
                        color: Color(0xFF0F172A),
                        letterSpacing: -0.5,
                      ),
                    ),
                    const SizedBox(width: 8),
                    Container(
                      padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 3),
                      decoration: const BoxDecoration(
                        color: Color(0xFFD1FAE5),
                        borderRadius: BorderRadius.all(Radius.circular(6)),
                      ),
                      child: const Row(
                        mainAxisSize: MainAxisSize.min,
                        children: [
                          Badge(
                            backgroundColor: Color(0xFF10B981),
                            smallSize: 6,
                          ),
                          SizedBox(width: 5),
                          Text(
                            'Okulda',
                            style: TextStyle(
                              fontSize: 9,
                              fontWeight: FontWeight.bold,
                              color: Color(0xFF065F46),
                            ),
                          ),
                        ],
                      ),
                    ),
                  ],
                ),
                const SizedBox(height: 4),
                Text(
                  'Sınıf: ${child.classroomName}  •  Doğum Tarihi: $dob',
                  style: const TextStyle(
                    color: Color(0xFF64748B),
                    fontWeight: FontWeight.w600,
                    fontSize: 11.5,
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }


  Widget _buildDailyDevelopmentSection(Student child) {
    final theme = Theme.of(context);

    if (_isLoadingDailyDetails) {
      return const Center(
        child: Padding(
          padding: EdgeInsets.symmetric(vertical: 20.0),
          child: CircularProgressIndicator(color: Color(0xFF6366F1)),
        ),
      );
    }

    final waterPercent = (_waterIntake / 2000.0).clamp(0.0, 1.0);

    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        _buildSectionTitle(
          'Günlük Gelişim Raporu',
          'Çocuğunuzun okul içi durumunu takip edin',
        ),
        const SizedBox(height: 12),

        // 1. Su ve Uyku Kartları (Row)
        Row(
          children: [
            // Su Tüketimi
            Expanded(
              child: Container(
                padding: const EdgeInsets.all(14),
                decoration: BoxDecoration(
                  color: Colors.white,
                  borderRadius: BorderRadius.circular(20),
                  border: Border.all(color: const Color(0xFFE2E8F0)),
                ),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Row(
                      children: [
                        Container(
                          padding: const EdgeInsets.all(5),
                          decoration: const BoxDecoration(
                            color: Color(0xFFE0F2FE),
                            shape: BoxShape.circle,
                          ),
                          child: const Icon(Icons.water_drop, color: Color(0xFF0284C7), size: 14),
                        ),
                        const SizedBox(width: 6),
                        const Text(
                          'Su Tüketimi',
                          style: TextStyle(fontSize: 11, fontWeight: FontWeight.bold, color: Color(0xFF475569)),
                        ),
                      ],
                    ),
                    const SizedBox(height: 10),
                    Text(
                      '$_waterIntake ml',
                      style: const TextStyle(fontSize: 15, fontWeight: FontWeight.w800, color: Color(0xFF0284C7)),
                    ),
                    const SizedBox(height: 8),
                    ClipRRect(
                      borderRadius: BorderRadius.circular(4),
                      child: LinearProgressIndicator(
                        value: waterPercent,
                        minHeight: 5,
                        color: const Color(0xFF0284C7),
                        backgroundColor: const Color(0xFFE0F2FE),
                      ),
                    ),
                  ],
                ),
              ),
            ),
            const SizedBox(width: 10),
            
            // Uyku Durumu
            Expanded(
              child: Container(
                padding: const EdgeInsets.all(14),
                decoration: BoxDecoration(
                  color: Colors.white,
                  borderRadius: BorderRadius.circular(20),
                  border: Border.all(color: const Color(0xFFE2E8F0)),
                ),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Row(
                      children: [
                        Container(
                          padding: const EdgeInsets.all(5),
                          decoration: const BoxDecoration(
                            color: Color(0xFFFEF3C7),
                            shape: BoxShape.circle,
                          ),
                          child: const Icon(Icons.bedtime_rounded, color: Color(0xFFD97706), size: 14),
                        ),
                        const SizedBox(width: 6),
                        const Text(
                          'Öğle Uykusu',
                          style: TextStyle(fontSize: 11, fontWeight: FontWeight.bold, color: Color(0xFF475569)),
                        ),
                      ],
                    ),
                    const SizedBox(height: 10),
                    Text(
                      _sleepStatusText,
                      style: const TextStyle(fontSize: 15, fontWeight: FontWeight.w800, color: Color(0xFFD97706)),
                    ),
                    const SizedBox(height: 13),
                    const Text(
                      'Dinlenme Karnesi',
                      style: TextStyle(fontSize: 9, color: Color(0xFF94A3B8), fontWeight: FontWeight.bold),
                    ),
                  ],
                ),
              ),
            ),
          ],
        ),
        const SizedBox(height: 10),

        // 2. Yemek Öğünleri Kartı
        Container(
          width: double.infinity,
          padding: const EdgeInsets.all(16),
          decoration: BoxDecoration(
            color: Colors.white,
            borderRadius: BorderRadius.circular(24),
            border: Border.all(color: const Color(0xFFE2E8F0)),
          ),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Row(
                children: [
                  Icon(Icons.restaurant_rounded, color: theme.colorScheme.primary, size: 16),
                  const SizedBox(width: 6),
                  const Text(
                    'Beslenme Karnesi',
                    style: TextStyle(fontSize: 12.5, fontWeight: FontWeight.bold, color: Color(0xFF1E293B)),
                  ),
                ],
              ),
              const SizedBox(height: 12),
              _meals.isEmpty
                  ? const Center(
                      child: Padding(
                        padding: EdgeInsets.symmetric(vertical: 6.0),
                        child: Text(
                          'Öğün bilgisi henüz girilmedi.',
                          style: TextStyle(fontSize: 11, color: Color(0xFF94A3B8), fontWeight: FontWeight.w500),
                        ),
                      ),
                    )
                  : ListView.separated(
                      shrinkWrap: true,
                      physics: const NeverScrollableScrollPhysics(),
                      itemCount: _meals.length,
                      separatorBuilder: (_, __) => const Divider(color: Color(0xFFF1F5F9), height: 12),
                      itemBuilder: (context, idx) {
                        final m = _meals[idx] as Map<String, dynamic>;
                        final name = m['mealName'] as String? ?? m['MealName'] as String? ?? 'Öğün';
                        final details = m['foodContent'] as String? ?? m['FoodContent'] as String? ?? 'Menü bilgisi yok';
                        final int status = (m['statusType'] ?? m['StatusType'] ?? 0) as int;
                        
                        String statusText = 'Girilmedi';
                        IconData statusIcon = Icons.help_outline;
                        Color statusColor = const Color(0xFF64748B);
                        if (status == 3) {
                          statusText = 'Hepsini Yedi';
                          statusIcon = Icons.check_circle_rounded;
                          statusColor = const Color(0xFF16A34A);
                        } else if (status == 2) {
                          statusText = 'Yarısını Yedi';
                          statusIcon = Icons.pause_circle_filled_rounded;
                          statusColor = const Color(0xFFD97706);
                        } else if (status == 1) {
                          statusText = 'Yemedi';
                          statusIcon = Icons.cancel_rounded;
                          statusColor = const Color(0xFFDC2626);
                        }

                        return Row(
                          children: [
                            Expanded(
                              child: Column(
                                crossAxisAlignment: CrossAxisAlignment.start,
                                children: [
                                  Text(
                                    name,
                                    style: const TextStyle(fontSize: 12, fontWeight: FontWeight.w800, color: Color(0xFF1E293B)),
                                  ),
                                  const SizedBox(height: 2),
                                  Text(
                                    details,
                                    style: const TextStyle(fontSize: 10.5, color: Color(0xFF64748B), fontWeight: FontWeight.w500),
                                  ),
                                ],
                              ),
                            ),
                            const SizedBox(width: 12),
                            Container(
                              padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
                              decoration: BoxDecoration(
                                color: statusColor.withOpacity(0.08),
                                borderRadius: BorderRadius.circular(6),
                              ),
                              child: Row(
                                children: [
                                  Icon(statusIcon, color: statusColor, size: 11),
                                  const SizedBox(width: 4),
                                  Text(
                                    statusText,
                                    style: TextStyle(fontSize: 9, fontWeight: FontWeight.bold, color: statusColor),
                                  ),
                                ],
                              ),
                            ),
                          ],
                        );
                      },
                    ),
            ],
          ),
        ),

        // 3. Mesajlar & Yapay Zeka Analizi
        if (_teacherNote.isNotEmpty || (child.aiSummary != null && child.aiSummary!.isNotEmpty)) ...[
          const SizedBox(height: 10),
          Container(
            width: double.infinity,
            padding: const EdgeInsets.all(14),
            decoration: BoxDecoration(
              color: Colors.white,
              borderRadius: BorderRadius.circular(24),
              border: Border.all(color: const Color(0xFFE2E8F0)),
            ),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Row(
                  children: [
                    Icon(Icons.chat_bubble_outline_rounded, color: theme.colorScheme.primary, size: 16),
                    const SizedBox(width: 6),
                    const Text(
                      'Geri Bildirim & Analiz',
                      style: TextStyle(fontSize: 12.5, fontWeight: FontWeight.bold, color: Color(0xFF1E293B)),
                    ),
                  ],
                ),
                if (_teacherNote.isNotEmpty) ...[
                  const SizedBox(height: 10),
                  Container(
                    width: double.infinity,
                    padding: const EdgeInsets.all(10),
                    decoration: BoxDecoration(
                      color: const Color(0xFFFEF3C7).withOpacity(0.4),
                      borderRadius: BorderRadius.circular(14),
                      border: Border.all(color: const Color(0xFFFDE68A)),
                    ),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        const Row(
                          children: [
                            Icon(Icons.edit_note_rounded, color: Color(0xFFB45309), size: 14),
                            SizedBox(width: 5),
                            Text(
                              'Öğretmen Notu',
                              style: TextStyle(fontSize: 10.5, fontWeight: FontWeight.bold, color: Color(0xFF92400E)),
                            ),
                          ],
                        ),
                        const SizedBox(height: 5),
                        Text(
                          _teacherNote,
                          style: const TextStyle(fontSize: 11.5, color: Color(0xFF92400E), height: 1.4, fontWeight: FontWeight.w600),
                        ),
                      ],
                    ),
                  ),
                ],
                if (child.aiSummary != null && child.aiSummary!.isNotEmpty) ...[
                  const SizedBox(height: 10),
                  Container(
                    width: double.infinity,
                    padding: const EdgeInsets.all(10),
                    decoration: BoxDecoration(
                      color: const Color(0xFFEEF2FF),
                      borderRadius: BorderRadius.circular(14),
                      border: Border.all(color: const Color(0xFFE0E7FF)),
                    ),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Row(
                          children: [
                            Icon(Icons.auto_awesome_rounded, color: theme.colorScheme.secondary, size: 12),
                            const SizedBox(width: 5),
                            Text(
                              'Yapay Zeka Analizi',
                              style: TextStyle(fontSize: 10.5, fontWeight: FontWeight.bold, color: theme.colorScheme.secondary),
                            ),
                          ],
                        ),
                        const SizedBox(height: 5),
                        Text(
                          child.aiSummary!,
                          style: TextStyle(
                            fontSize: 11.5,
                            color: theme.colorScheme.secondary,
                            height: 1.4,
                            fontWeight: FontWeight.w600,
                          ),
                        ),
                      ],
                    ),
                  ),
                ],
              ],
            ),
          ),
        ],
      ],
    );
  }
}


