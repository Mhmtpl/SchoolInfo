import 'package:flutter/material.dart';
import '../../../../core/tenant/school_id.dart';
import '../../../auth/domain/entities/student.dart';
import '../../../auth/presentation/pages/login_screen.dart';
import '../../data/repositories/home_repository_impl.dart';
import '../../domain/usecases/get_home_summary.dart';
import '../../domain/entities/home_summary.dart';
import '../widgets/dashboard_stat_card.dart';
import '../widgets/home_menu_card.dart';
import 'activity_screen.dart';
import 'announcement_screen.dart';
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
      backgroundColor: const Color(0xFFF4F7FF),
      body: FutureBuilder<HomeSummary>(
        future: _getHomeSummary(widget.schoolId),
        builder: (context, snapshot) {
          if (snapshot.connectionState == ConnectionState.waiting) {
            return const Center(child: CircularProgressIndicator());
          }

          if (snapshot.hasError) {
            return Center(child: Text('Veri yüklenemedi: ${snapshot.error}'));
          }

          final summary = snapshot.data!;
          final selectedChild = _selectedChild;
          return SafeArea(
            bottom: true,
            child: SingleChildScrollView(
              padding: const EdgeInsets.only(bottom: 40),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  _buildHeader(summary, selectedChild),
                  Container(
                    width: double.infinity,
                    decoration: const BoxDecoration(
                      color: Color(0xFFF3F1FF),
                      borderRadius: BorderRadius.only(
                        topLeft: Radius.circular(32),
                        topRight: Radius.circular(32),
                      ),
                    ),
                    child: Padding(
                      padding: const EdgeInsets.fromLTRB(20, 24, 20, 24),
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          _buildAtAGlance(summary),
                          const SizedBox(height: 22),
                          _buildSectionTitle(
                            'Hızlı Erişim',
                            'En sık ziyaret edilen alanlara hızlıca ulaşın',
                          ),
                          const SizedBox(height: 12),
                          _buildQuickActionsRow(context),
                          const SizedBox(height: 28),
                          if (widget.students != null && widget.students!.isNotEmpty) ...[
                            _buildChildrenSection(widget.students!, summary.classroom),
                            const SizedBox(height: 24),
                            if (selectedChild != null) _buildSelectedChildSummary(selectedChild, summary.classroom),
                            const SizedBox(height: 24),
                          ],
                          _buildSectionTitle(
                            'Okul Panosu',
                            'Duyurular ve sık kullanılan sayfalara hızlı erişim',
                          ),
                          const SizedBox(height: 12),
                          _buildMenuGrid(context),
                          const SizedBox(height: 24),
                          _buildAnnouncementBanner(),
                          const SizedBox(height: 40),
                        ],
                      ),
                    ),
                  ),
                ],
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
    if (value.isEmpty) return 'Yıldız';

    final lowerValue = value.toLowerCase();
    final children = widget.students ?? [];
    if (children.any((child) => child.firstName.toLowerCase() == lowerValue)) {
      return 'Yıldız';
    }

    return value.split(' ').first;
  }

  Widget _buildHeader(HomeSummary summary, Student? selectedChild) {
    final headerChildName = selectedChild != null
        ? '${selectedChild.firstName} ${selectedChild.lastName}'
        : summary.childName;
    final headerClassroom = summary.classroom.isNotEmpty
        ? summary.classroom
        : 'Bilinmiyor';
    final headerDetails = selectedChild != null
        ? 'Sınıf: $headerClassroom'
        : 'Giriş Saati: 08:45 | Sınıf: $headerClassroom';

    return Container(
      width: double.infinity,
      decoration: const BoxDecoration(
        gradient: LinearGradient(
          colors: [Color(0xFF6C5CE7), Color(0xFFa29bfe)],
          begin: Alignment.topLeft,
          end: Alignment.bottomRight,
        ),
        borderRadius: BorderRadius.only(
          bottomLeft: Radius.circular(36),
          bottomRight: Radius.circular(36),
        ),
      ),
      padding: const EdgeInsets.only(top: 32, left: 24, right: 24, bottom: 28),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    const Text(
                      'Mini Adımlar Anaokulu',
                      style: TextStyle(
                        color: Colors.white70,
                        fontSize: 14,
                        fontWeight: FontWeight.w500,
                        letterSpacing: 1,
                      ),
                    ),
                    const SizedBox(height: 8),
                    Text(
                      'Merhaba, $_parentFirstName Hanım 👋',
                      style: const TextStyle(
                        color: Colors.white,
                        fontSize: 26,
                        fontWeight: FontWeight.bold,
                      ),
                    ),
                    const SizedBox(height: 10),
                    const Text(
                      'Çocuğunuzun gününü takip edin ve okuldan gelen bildirimleri hızlıca görün.',
                      style: TextStyle(
                        color: Colors.white70,
                        fontSize: 14,
                        height: 1.5,
                      ),
                    ),
                  ],
                ),
              ),
              const SizedBox(width: 12),
              Container(
                padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 10),
                decoration: BoxDecoration(
                  color: const Color.fromRGBO(255, 255, 255, 0.18),
                  borderRadius: BorderRadius.circular(24),
                ),
                child: Row(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        const Text(
                          'Hesap',
                          style: TextStyle(
                            color: Colors.white,
                            fontSize: 12,
                            fontWeight: FontWeight.w600,
                          ),
                        ),
                        const SizedBox(height: 4),
                        Text(
                          _parentFirstName,
                          style: const TextStyle(
                            color: Colors.white70,
                            fontSize: 11,
                          ),
                        ),
                      ],
                    ),
                    const SizedBox(width: 12),
                    InkWell(
                      borderRadius: BorderRadius.circular(32),
                      onTap: _openProfileScreen,
                      child: Container(
                        width: 38,
                        height: 38,
                        decoration: BoxDecoration(
                          shape: BoxShape.circle,
                          border: Border.all(color: Colors.white, width: 2),
                        ),
                        child: const CircleAvatar(
                          radius: 18,
                          backgroundColor: Colors.white,
                          child: Icon(Icons.face, size: 26, color: Color(0xFF6C5CE7)),
                        ),
                      ),
                    ),
                    const SizedBox(width: 10),
                    Container(
                      width: 36,
                      height: 36,
                      decoration: BoxDecoration(
                        color: const Color.fromRGBO(255, 255, 255, 0.18),
                        borderRadius: BorderRadius.circular(14),
                      ),
                      child: IconButton(
                        onPressed: _logout,
                        icon: const Icon(Icons.logout, size: 18, color: Color(0xFF202020)),
                        tooltip: 'Çıkış',
                        splashRadius: 20,
                      ),
                    ),
                  ],
                ),
              ),
            ],
          ),
          const SizedBox(height: 22),
          Container(
            width: double.infinity,
            padding: const EdgeInsets.all(18),
            decoration: BoxDecoration(
              color: const Color.fromRGBO(255, 255, 255, 0.15),
              borderRadius: BorderRadius.circular(24),
            ),
            child: Row(
              children: [
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        '$headerChildName şu an okulda 🎒',
                        style: const TextStyle(
                          color: Colors.white,
                          fontWeight: FontWeight.bold,
                          fontSize: 16,
                        ),
                      ),
                      const SizedBox(height: 8),
                      Text(
                        headerDetails,
                        style: const TextStyle(
                          color: Colors.white70,
                          fontSize: 13,
                        ),
                      ),
                      if (widget.students != null && widget.students!.isNotEmpty) ...[
                        const SizedBox(height: 8),
                        Text(
                          'Çocuk(lar): ${widget.students!.map((s) => '${s.firstName} ${s.lastName}').join(', ')}',
                          maxLines: 2,
                          overflow: TextOverflow.ellipsis,
                          style: const TextStyle(
                            color: Colors.white70,
                            fontSize: 12,
                          ),
                        ),
                      ],
                    ],
                  ),
                ),
                const SizedBox(width: 12),
                Container(
                  width: 56,
                  height: 56,
                  decoration: BoxDecoration(
                    color: Colors.white,
                    borderRadius: BorderRadius.circular(18),
                  ),
                  child: const Icon(
                    Icons.school,
                    color: Color(0xFF6C5CE7),
                    size: 32,
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildAtAGlance(HomeSummary summary) {
    return Wrap(
      spacing: 14,
      runSpacing: 14,
      children: [
        SizedBox(
          width: 110,
          child: DashboardStatCard(
            icon: Icons.restaurant,
            value: summary.mealStatus,
            label: 'Yemek Durumu',
            color: const Color(0xFF6C5CE7),
          ),
        ),
        SizedBox(
          width: 110,
          child: DashboardStatCard(
            icon: Icons.hotel,
            value: summary.sleepStatus,
            label: 'Uyku Durumu',
            color: const Color(0xFFFFA325),
          ),
        ),
        SizedBox(
          width: 110,
          child: DashboardStatCard(
            icon: Icons.sports_tennis,
            value: summary.activityStatus,
            label: 'Aktivite Durumu',
            color: const Color(0xFF00B894),
          ),
        ),
      ],
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
                  fontSize: 20,
                  fontWeight: FontWeight.bold,
                  color: Color(0xFF2D3436),
                ),
              ),
              const SizedBox(height: 4),
              Text(
                subtitle,
                style: const TextStyle(fontSize: 13, color: Colors.grey),
              ),
            ],
          ),
        ),
        if (onViewAll != null)
          TextButton(onPressed: onViewAll, child: const Text('Tümünü Gör')),
      ],
    );
  }

  Widget _buildQuickActionsRow(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.symmetric(horizontal: 20.0),
      child: Wrap(
        spacing: 10,
        runSpacing: 10,
        children: [
          _buildQuickActionChip(
            context,
            icon: Icons.bedtime,
            label: 'Uyku',
            color: const Color(0xFF00B894),
            onTap: () => Navigator.push(
              context,
              MaterialPageRoute(builder: (_) => const SleepScreen()),
            ),
          ),
          _buildQuickActionChip(
            context,
            icon: Icons.restaurant,
            label: 'Yemek',
            color: const Color(0xFF6C5CE7),
            onTap: () => Navigator.push(
              context,
              MaterialPageRoute(builder: (_) => const MealScreen()),
            ),
          ),
          _buildQuickActionChip(
            context,
            icon: Icons.sports_tennis,
            label: 'Etkinlik',
            color: const Color(0xFFFF9F43),
            onTap: () => Navigator.push(
              context,
              MaterialPageRoute(builder: (_) => const ActivityScreen()),
            ),
          ),
          _buildQuickActionChip(
            context,
            icon: Icons.medical_services,
            label: 'İlaç',
            color: const Color(0xFF8E44AD),
            onTap: () => Navigator.push(
              context,
              MaterialPageRoute(builder: (_) => const MedicationScreen()),
            ),
          ),
          _buildQuickActionChip(
            context,
            icon: Icons.person,
            label: 'Profil',
            color: const Color(0xFF2980B9),
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
    return InkWell(
      onTap: onTap,
      borderRadius: BorderRadius.circular(18),
      child: Container(
        padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 12),
        decoration: BoxDecoration(
          color: color.withAlpha((0.12 * 255).round()),
          borderRadius: BorderRadius.circular(18),
        ),
        child: Row(
          mainAxisSize: MainAxisSize.min,
          children: [
            Icon(icon, color: color, size: 20),
            const SizedBox(width: 8),
            Text(
              label,
              style: TextStyle(color: color, fontWeight: FontWeight.bold),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildChildrenSection(List<Student> students, String className) {
    return Padding(
      padding: const EdgeInsets.symmetric(horizontal: 20.0),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          const Padding(
            padding: EdgeInsets.only(bottom: 10.0),
            child: Text(
              'Çocuklarınız',
              style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold),
            ),
          ),
          SizedBox(
            height: 170,
            child: ListView.separated(
              scrollDirection: Axis.horizontal,
              itemCount: students.length,
              separatorBuilder: (context, index) => const SizedBox(width: 10),
              itemBuilder: (context, index) {
                final s = students[index];
                final isSelected = index == _selectedChildIndex;
                final dob = s.dateOfBirth == DateTime.fromMillisecondsSinceEpoch(0)
                    ? '—'
                    : '${s.dateOfBirth.day}/${s.dateOfBirth.month}/${s.dateOfBirth.year}';
                return GestureDetector(
                  onTap: () => setState(() {
                    _selectedChildIndex = index;
                  }),
                  child: AnimatedContainer(
                    duration: const Duration(milliseconds: 180),
                    width: 220,
                    padding: const EdgeInsets.all(14),
                    decoration: BoxDecoration(
                      color: Colors.white,
                      borderRadius: BorderRadius.circular(16),
                      border: Border.all(
                        color: isSelected ? const Color(0xFF6C5CE7) : Colors.transparent,
                        width: 2,
                      ),
                      boxShadow: const [
                        BoxShadow(
                          color: Color.fromRGBO(0, 0, 0, 0.08),
                          blurRadius: 8,
                        ),
                      ],
                    ),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Row(
                          mainAxisAlignment: MainAxisAlignment.spaceBetween,
                          children: [
                            Text(
                              '${s.firstName} ${s.lastName}',
                              style: TextStyle(
                                fontWeight: FontWeight.bold,
                                color: isSelected ? const Color(0xFF6C5CE7) : Colors.black,
                              ),
                            ),
                            Icon(
                              isSelected ? Icons.star : Icons.star_border,
                              color: isSelected ? const Color(0xFF6C5CE7) : Colors.grey,
                              size: 18,
                            ),
                          ],
                        ),
                        const SizedBox(height: 8),
                        Text('Sınıf: ${className.isNotEmpty ? className : 'Bilinmiyor'}', style: const TextStyle(color: Colors.black54, fontSize: 12)),
                        const SizedBox(height: 4),
                        Text('Doğum: $dob', style: const TextStyle(color: Colors.black54, fontSize: 12)),
                        const Spacer(),
                        Text(
                          s.dailyRecordSummary ?? 'Bugün için veri bekleniyor',
                          maxLines: 2,
                          overflow: TextOverflow.ellipsis,
                          style: const TextStyle(fontSize: 12, color: Colors.black87),
                        ),
                      ],
                    ),
                  ),
                );
              },
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildSelectedChildSummary(Student child, String className) {
    final dob = child.dateOfBirth == DateTime.fromMillisecondsSinceEpoch(0)
        ? '—'
        : '${child.dateOfBirth.day}/${child.dateOfBirth.month}/${child.dateOfBirth.year}';
    return Padding(
      padding: const EdgeInsets.symmetric(horizontal: 20.0),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          _buildSectionTitle('Seçili Çocuk', 'Tüm önemli bilgileri ve günlük özetini burada görün'),
          const SizedBox(height: 12),
          Container(
            width: double.infinity,
            padding: const EdgeInsets.all(18),
            decoration: BoxDecoration(
              color: Colors.white,
              borderRadius: BorderRadius.circular(20),
              boxShadow: const [
                BoxShadow(
                  color: Color.fromRGBO(0, 0, 0, 0.08),
                  blurRadius: 12,
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
                          Text(
                            '${child.firstName} ${child.lastName}',
                            style: const TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
                          ),
                          const SizedBox(height: 4),
                          Text('Sınıf: ${className.isNotEmpty ? className : 'Bilinmiyor'}', style: const TextStyle(color: Colors.black54)),
                        ],
                      ),
                    ),
                    Container(
                      padding: const EdgeInsets.all(10),
                      decoration: BoxDecoration(
                        color: const Color.fromRGBO(108, 92, 231, 0.12),
                        borderRadius: BorderRadius.circular(14),
                      ),
                      child: const Icon(
                        Icons.child_care,
                        color: Color(0xFF6C5CE7),
                        size: 24,
                      ),
                    ),
                  ],
                ),
                const SizedBox(height: 16),
                Row(
                  children: [
                    _buildInfoChip('Doğum', dob),
                    const SizedBox(width: 10),
                    _buildInfoChip('Durum', child.dailyRecordSummary != null ? 'Güncellenmiş' : 'Bekleniyor'),
                  ],
                ),
                const SizedBox(height: 18),
                const Text('Bugünkü Özet', style: TextStyle(fontWeight: FontWeight.bold, fontSize: 15)),
                const SizedBox(height: 8),
                Text(
                  child.dailyRecordSummary ?? 'Öğretmenden günlük veri bekleniyor. Bu alanda uyku, su tüketimi ve önemli notlar gösterilecektir.',
                  style: const TextStyle(fontSize: 14, color: Colors.black87, height: 1.4),
                ),
                if (child.aiSummary != null && child.aiSummary!.isNotEmpty) ...[
                  const SizedBox(height: 14),
                  const Text('Yapay Zeka Önerisi', style: TextStyle(fontWeight: FontWeight.bold, fontSize: 15)),
                  const SizedBox(height: 8),
                  Text(
                    child.aiSummary!,
                    style: const TextStyle(fontSize: 14, color: Colors.black87, height: 1.4),
                  ),
                ],
              ],
            ),
          ),
          const SizedBox(height: 20),
        ],
      ),
    );
  }

  Widget _buildInfoChip(String label, String value) {
    return Container(
      padding: const EdgeInsets.symmetric(vertical: 8, horizontal: 12),
      decoration: BoxDecoration(
        color: const Color.fromRGBO(108, 92, 231, 0.08),
        borderRadius: BorderRadius.circular(14),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(label, style: const TextStyle(fontSize: 11, color: Colors.black54)),
          const SizedBox(height: 4),
          Text(value, style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 13)),
        ],
      ),
    );
  }

  Widget _buildMenuGrid(BuildContext context) {
    final items = [
      _MenuItem(
        title: 'Yemek',
        icon: Icons.restaurant,
        onTap: () => Navigator.push(
          context,
          MaterialPageRoute(builder: (_) => const MealScreen()),
        ),
      ),
      _MenuItem(
        title: 'Uyku',
        icon: Icons.bedtime,
        onTap: () => Navigator.push(
          context,
          MaterialPageRoute(builder: (_) => const SleepScreen()),
        ),
      ),
      _MenuItem(
        title: 'Etkinlik',
        icon: Icons.sports_tennis,
        onTap: () => Navigator.push(
          context,
          MaterialPageRoute(builder: (_) => const ActivityScreen()),
        ),
      ),
      _MenuItem(
        title: 'İlaç Takibi',
        icon: Icons.medical_services,
        onTap: () => Navigator.push(
          context,
          MaterialPageRoute(builder: (_) => const MedicationScreen()),
        ),
      ),
      _MenuItem(
        title: 'Duyurular',
        icon: Icons.announcement,
        onTap: () => Navigator.push(
          context,
          MaterialPageRoute(builder: (_) => const AnnouncementScreen()),
        ),
      ),
      _MenuItem(
        title: 'Profil',
        icon: Icons.person,
        onTap: _openProfileScreen,
      ),
    ];

    return GridView.builder(
      shrinkWrap: true,
      physics: const NeverScrollableScrollPhysics(),
      itemCount: items.length,
      gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
        crossAxisCount: 2,
        crossAxisSpacing: 12,
        mainAxisSpacing: 12,
        childAspectRatio: 2.5,
      ),
      itemBuilder: (context, index) {
        final item = items[index];
        return HomeMenuCard(
          title: item.title,
          icon: item.icon,
          onTap: item.onTap,
        );
      },
    );
  }

  Widget _buildAnnouncementBanner() {
    return Container(
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(24),
        boxShadow: const [
          BoxShadow(
            color: Color.fromRGBO(0, 0, 0, 0.06),
            blurRadius: 12,
            offset: Offset(0, 6),
          ),
        ],
      ),
      child: const Row(
        children: [
          Icon(Icons.announcement, color: Color(0xFF6C5CE7), size: 28),
          SizedBox(width: 14),
          Expanded(
            child: Text(
              'Bugün etkinlik var: Resim atölyesi 15:00’te.',
              style: TextStyle(color: Color(0xFF202020), fontSize: 15),
            ),
          ),
        ],
      ),
    );
  }
}

class _MenuItem {
  final String title;
  final IconData icon;
  final VoidCallback onTap;

  _MenuItem({required this.title, required this.icon, required this.onTap});
}
