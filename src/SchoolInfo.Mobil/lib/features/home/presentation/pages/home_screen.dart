import 'package:flutter/material.dart';
import '../../../../core/tenant/school_id.dart';
import '../../../auth/domain/entities/student.dart';
import '../../../auth/presentation/pages/login_screen.dart';
import '../../data/repositories/home_repository_impl.dart';
import '../../domain/usecases/get_home_summary.dart';
import '../../domain/entities/home_summary.dart';
import 'activity_screen.dart';
import 'medication_screen.dart';
import 'meal_screen.dart';
import 'profile_screen.dart';
import 'sleep_screen.dart';

// Ana sayfa ekranı. Login sonrası gösterilir ve okul kimliğine göre özet verilerini yükler.
class HomeScreen extends StatefulWidget {
  final SchoolId schoolId;
  final String? userFirstName;
  final String? userLastName;
  final List<Student>? students;

  const HomeScreen({
    super.key,
    required this.schoolId,
    this.userFirstName,
    this.userLastName,
    this.students,
  });

  @override
  State<HomeScreen> createState() => _HomeScreenState();
}

class _HomeScreenState extends State<HomeScreen> {
  late final GetHomeSummary _getHomeSummary;
  int _selectedChildIndex = 0;
  late String _currentFirstName;
  late String _currentLastName;
  late String _currentEmail;
  late String _currentClassroom;
  late String _currentPhone;

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
    _currentFirstName = widget.userFirstName ?? '';
    _currentLastName = widget.userLastName ?? '';
    _currentEmail = '';
    _currentClassroom = '';
    _currentPhone = '';
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

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: const Color(0xFFF8FAFC), // Ultra-clean background
      body: FutureBuilder<HomeSummary>(
        future: _getHomeSummary(widget.schoolId),
        builder: (context, snapshot) {
          if (snapshot.connectionState == ConnectionState.waiting) {
            return const Center(child: CircularProgressIndicator());
          }

          if (snapshot.hasError) {
            return Center(
              child: Padding(
                padding: const EdgeInsets.all(24.0),
                child: Text(
                  'Veri yüklenemedi: ${snapshot.error}',
                  style: const TextStyle(color: Color(0xFFEF4444), fontWeight: FontWeight.bold),
                ),
              ),
            );
          }

          final summary = snapshot.data!;
          final selectedChild = _selectedChild;
          return SafeArea(
            bottom: true,
            child: RefreshIndicator(
              onRefresh: () async {
                setState(() {});
              },
              child: SingleChildScrollView(
                physics: const AlwaysScrollableScrollPhysics(),
                padding: const EdgeInsets.only(bottom: 40),
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
                            _buildChildrenSection(widget.students!, summary.classroom),
                            const SizedBox(height: 20),
                          ],
                          
                          // At a glance summary (Sleep, Meal, Activity)
                          _buildAtAGlanceSection(summary),
                          const SizedBox(height: 24),
                          
                          if (selectedChild != null) ...[
                            _buildSelectedChildSummary(selectedChild, summary.classroom),
                            const SizedBox(height: 24),
                          ],

                          _buildSectionTitle(
                            'Hızlı Erişim',
                            'En sık ziyaret edilen alanlara hızlıca ulaşın',
                          ),
                          const SizedBox(height: 12),
                          _buildQuickActionsRow(context),
                          const SizedBox(height: 28),
                          
                          _buildSectionTitle(
                            'Duyurular & Haberler',
                            'Okuldan gelen en son duyuruları takip edin',
                          ),
                          const SizedBox(height: 14),
                          _buildAnnouncementsSection(),
                          const SizedBox(height: 20),
                        ],
                      ),
                    ),
                  ],
                ),
              ),
            ),
          );
        },
      ),
    );
  }

  void _logout() {
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
                      'MİNİ ADIMLAR ANAOKULU',
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
                onTap: () => setState(() {
                  _selectedChildIndex = index;
                }),
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
  }) {
    return Container(
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

  Widget _buildQuickActionsRow(BuildContext context) {
    final theme = Theme.of(context);
    return SizedBox(
      height: 40,
      child: ListView(
        scrollDirection: Axis.horizontal,
        physics: const BouncingScrollPhysics(),
        children: [
          _buildQuickActionChip(
            context,
            icon: Icons.bedtime_rounded,
            label: 'Uyku',
            color: const Color(0xFFF59E0B),
            onTap: () => Navigator.push(
              context,
              MaterialPageRoute(builder: (_) => const SleepScreen()),
            ),
          ),
          const SizedBox(width: 8),
          _buildQuickActionChip(
            context,
            icon: Icons.restaurant_rounded,
            label: 'Yemek',
            color: const Color(0xFFF97316),
            onTap: () => Navigator.push(
              context,
              MaterialPageRoute(builder: (_) => const MealScreen()),
            ),
          ),
          const SizedBox(width: 8),
          _buildQuickActionChip(
            context,
            icon: Icons.sports_tennis_rounded,
            label: 'Etkinlik',
            color: const Color(0xFF10B981),
            onTap: () => Navigator.push(
              context,
              MaterialPageRoute(builder: (_) => const ActivityScreen()),
            ),
          ),
          const SizedBox(width: 8),
          _buildQuickActionChip(
            context,
            icon: Icons.medical_services_rounded,
            label: 'İlaç',
            color: const Color(0xFFEF4444),
            onTap: () => Navigator.push(
              context,
              MaterialPageRoute(builder: (_) => const MedicationScreen()),
            ),
          ),
          const SizedBox(width: 8),
          _buildQuickActionChip(
            context,
            icon: Icons.person_rounded,
            label: 'Profil',
            color: theme.colorScheme.primary,
            onTap: _openProfileScreen,
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

  Widget _buildSelectedChildSummary(Student child, String className) {
    final theme = Theme.of(context);
    final dob = child.dateOfBirth == DateTime.fromMillisecondsSinceEpoch(0)
        ? '—'
        : '${child.dateOfBirth.day}/${child.dateOfBirth.month}/${child.dateOfBirth.year}';
    
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(24),
        border: Border.all(
          color: const Color(0xFFE2E8F0),
          width: 1,
        ),
        boxShadow: const [
          BoxShadow(
            color: Color.fromRGBO(15, 23, 42, 0.02),
            blurRadius: 24,
            offset: Offset(0, 8),
          ),
        ],
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
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
                            fontSize: 17,
                            fontWeight: FontWeight.w800,
                            color: Color(0xFF0F172A),
                            letterSpacing: -0.4,
                          ),
                        ),
                        const SizedBox(width: 8),
                        Container(
                          padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 3),
                          decoration: BoxDecoration(
                            color: const Color(0xFFD1FAE5),
                            borderRadius: BorderRadius.circular(8),
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
                                  fontSize: 10,
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
                      'Sınıf: ${className.isNotEmpty ? className : 'Bilinmiyor'} • Doğum: $dob',
                      style: const TextStyle(
                        color: Color(0xFF64748B), 
                        fontWeight: FontWeight.w600,
                        fontSize: 12,
                      ),
                    ),
                  ],
                ),
              ),
              Container(
                padding: const EdgeInsets.all(8),
                decoration: BoxDecoration(
                  color: theme.colorScheme.primary.withOpacity(0.06),
                  borderRadius: BorderRadius.circular(12),
                ),
                child: Icon(
                  Icons.child_care_rounded,
                  color: theme.colorScheme.primary,
                  size: 22,
                ),
              ),
            ],
          ),
          const SizedBox(height: 18),
          const Divider(color: Color(0xFFF1F5F9), height: 1),
          const SizedBox(height: 16),
          
          const Text(
            'Bugünkü Rapor',
            style: TextStyle(fontWeight: FontWeight.w800, fontSize: 14, color: Color(0xFF0F172A)),
          ),
          const SizedBox(height: 6),
          Text(
            child.dailyRecordSummary ?? 'Öğretmenden günlük veri bekleniyor. Bu alanda uyku, su tüketimi ve önemli notlar gösterilecektir.',
            style: const TextStyle(fontSize: 13, color: Color(0xFF475569), height: 1.5, fontWeight: FontWeight.w500),
          ),
          
          if (child.aiSummary != null && child.aiSummary!.isNotEmpty) ...[
            const SizedBox(height: 18),
            Container(
              width: double.infinity,
              padding: const EdgeInsets.all(16),
              decoration: BoxDecoration(
                gradient: const LinearGradient(
                  colors: [Color(0xFFFFF1F2), Color(0xFFFDF4FF)],
                  begin: Alignment.topLeft,
                  end: Alignment.bottomRight,
                ),
                borderRadius: BorderRadius.circular(20),
                border: Border.all(
                  color: const Color(0xFFFFE4E6),
                  width: 1,
                ),
              ),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Row(
                    children: [
                      Icon(Icons.auto_awesome_rounded, color: theme.colorScheme.secondary, size: 16),
                      const SizedBox(width: 8),
                      Text(
                        'Yapay Zeka Önerisi',
                        style: TextStyle(
                          fontWeight: FontWeight.w800,
                          fontSize: 13,
                          color: theme.colorScheme.secondary,
                        ),
                      ),
                    ],
                  ),
                  const SizedBox(height: 8),
                  Text(
                    child.aiSummary!,
                    style: const TextStyle(
                      fontSize: 13,
                      color: Color(0xFF4F46E5),
                      height: 1.5,
                      fontWeight: FontWeight.w600,
                    ),
                  ),
                ],
              ),
            ),
          ],
        ],
      ),
    );
  }

  Widget _buildAnnouncementsSection() {
    final theme = Theme.of(context);
    
    // Örnek Duyurular Listesi
    final announcements = [
      _AnnouncementItem(
        title: 'Resim Atölyesi Etkinliği',
        body: 'Bugün saat 15:00’te tüm sınıflarımızın katılımıyla okul bahçesinde resim atölyesi yapılacaktır. Çocuklarımızın yedek kıyafetlerini getirmeyi unutmayınız.',
        date: 'Bugün, 09:30',
        isNew: true,
        category: 'Etkinlik',
        icon: Icons.palette_rounded,
        iconColor: const Color(0xFFEA580C),
      ),
      _AnnouncementItem(
        title: 'Veli Bilgilendirme Toplantısı',
        body: 'Yeni dönem eğitim planı ve okul içi düzenlemeler hakkında görüşmek üzere Cuma günü saat 18:00’de konferans salonunda toplantı yapılacaktır.',
        date: 'Dün, 14:15',
        isNew: false,
        category: 'Toplantı',
        icon: Icons.groups_rounded,
        iconColor: theme.colorScheme.primary,
      ),
    ];

    return ListView.separated(
      shrinkWrap: true,
      physics: const NeverScrollableScrollPhysics(),
      itemCount: announcements.length,
      separatorBuilder: (_, __) => const SizedBox(height: 12),
      itemBuilder: (context, index) {
        final item = announcements[index];
        return Container(
          decoration: BoxDecoration(
            color: Colors.white,
            borderRadius: BorderRadius.circular(20),
            border: Border.all(
              color: item.isNew ? theme.colorScheme.secondary.withOpacity(0.2) : const Color(0xFFE2E8F0),
              width: item.isNew ? 1.2 : 1,
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
                      color: item.iconColor.withOpacity(0.06),
                      borderRadius: BorderRadius.circular(10),
                    ),
                    child: Icon(item.icon, color: item.iconColor, size: 18),
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
                                color: item.isNew 
                                    ? theme.colorScheme.secondary.withOpacity(0.1)
                                    : const Color(0xFFF1F5F9),
                                borderRadius: BorderRadius.circular(8),
                              ),
                              child: Text(
                                item.category,
                                style: TextStyle(
                                  color: item.isNew ? theme.colorScheme.secondary : const Color(0xFF64748B),
                                  fontSize: 10,
                                  fontWeight: FontWeight.bold,
                                ),
                              ),
                            ),
                            if (item.isNew) ...[
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
                          item.date,
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
                item.title,
                style: const TextStyle(
                  fontSize: 15,
                  fontWeight: FontWeight.w800,
                  color: Color(0xFF0F172A),
                  letterSpacing: -0.3,
                ),
              ),
              const SizedBox(height: 4),
              Text(
                item.body,
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
    );
  }
}

class _AnnouncementItem {
  final String title;
  final String body;
  final String date;
  final bool isNew;
  final String category;
  final IconData icon;
  final Color iconColor;

  _AnnouncementItem({
    required this.title,
    required this.body,
    required this.date,
    required this.isNew,
    required this.category,
    required this.icon,
    required this.iconColor,
  });
}
