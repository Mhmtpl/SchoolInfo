import 'dart:async';
import 'package:flutter/material.dart';
import 'package:anaokulu/core/services/biometric_signalr_service.dart';
import 'package:anaokulu/features/home/presentation/widgets/ecg_live_wave.dart';
import 'package:anaokulu/features/home/presentation/pages/biometrics_history_screen.dart';

class BiometricDashboardCard extends StatefulWidget {
  final String studentId;
  final String studentName;
  final int studentAge;
  final String token;
  final double liveHeartRate;
  final bool liveHasSignal;
  final String liveHubStatus;

  const BiometricDashboardCard({
    super.key,
    required this.studentId,
    required this.studentName,
    required this.studentAge,
    required this.token,
    required this.liveHeartRate,
    required this.liveHasSignal,
    required this.liveHubStatus,
  });

  @override
  State<BiometricDashboardCard> createState() => _BiometricDashboardCardState();
}

class _BiometricDashboardCardState extends State<BiometricDashboardCard> with SingleTickerProviderStateMixin {
  // Kalp ikonu nabız animasyonu için
  late AnimationController _heartAnimController;
  late Animation<double> _heartScaleAnim;

  @override
  void initState() {
    super.initState();
    
    _heartAnimController = AnimationController(
      vsync: this,
      duration: const Duration(milliseconds: 600),
    );
    _heartScaleAnim = Tween<double>(begin: 1.0, end: 1.25).animate(
      CurvedAnimation(parent: _heartAnimController, curve: Curves.bounceOut),
    );
  }

  @override
  void didUpdateWidget(covariant BiometricDashboardCard oldWidget) {
    super.didUpdateWidget(oldWidget);
    if (widget.liveHeartRate != oldWidget.liveHeartRate && widget.liveHeartRate > 0) {
      _heartAnimController.forward().then((_) => _heartAnimController.reverse());
    }
  }

  @override
  void dispose() {
    _heartAnimController.dispose();
    super.dispose();
  }

  Map<String, dynamic> _getHeartRateThresholds(int age) {
    if (age <= 2) {
      return {'low': 80, 'normalMax': 130, 'warningMax': 145};
    } else if (age <= 5) {
      return {'low': 80, 'normalMax': 120, 'warningMax': 135};
    } else if (age <= 10) {
      return {'low': 70, 'normalMax': 110, 'warningMax': 125};
    } else {
      return {'low': 60, 'normalMax': 100, 'warningMax': 115};
    }
  }

  Map<String, dynamic> _getStatusStyle(double bpm) {
    if (!widget.liveHasSignal || bpm <= 0) {
      return {
        'color': const Color(0xFF94A3B8), // Gri
        'bg': const Color(0xFFF1F5F9),
        'border': const Color(0xFFE2E8F0),
        'text': 'CİHAZ DIŞI / SENSÖR YOK',
      };
    }

    final thresholds = _getHeartRateThresholds(widget.studentAge);

    if (bpm < thresholds['low']) {
      return {
        'color': const Color(0xFF0284C7), // Mavi
        'bg': const Color(0xFFF0F9FF),
        'border': const Color(0xFFE0F2FE),
        'text': 'DÜŞÜK NABIZ',
      };
    } else if (bpm <= thresholds['normalMax']) {
      return {
        'color': const Color(0xFF16A34A), // Yeşil
        'bg': const Color(0xFFF0FDF4),
        'border': const Color(0xFFDCFCE7),
        'text': 'SİNYAL AKTİF',
      };
    } else if (bpm <= thresholds['warningMax']) {
      return {
        'color': const Color(0xFFEA580C), // Turuncu
        'bg': const Color(0xFFFFF7ED),
        'border': const Color(0xFFFFEDD5),
        'text': 'YÜKSEK NABIZ',
      };
    } else {
      return {
        'color': const Color(0xFFDC2626), // Kırmızı
        'bg': const Color(0xFFFEF2F2),
        'border': const Color(0xFFFEE2E2),
        'text': 'KRİTİK NABIZ',
      };
    }
  }

  @override
  Widget build(BuildContext context) {
    final style = _getStatusStyle(widget.liveHeartRate);
    final Color statusColor = style['color'] as Color;
    final Color bgColor = style['bg'] as Color;
    final Color borderColor = style['border'] as Color;
    final String statusText = style['text'] as String;

    return GestureDetector(
      onTap: () {
        Navigator.push(
          context,
          MaterialPageRoute(
            builder: (_) => BiometricsHistoryScreen(
              studentId: widget.studentId,
              studentName: widget.studentName,
              token: widget.token,
              studentAge: widget.studentAge,
            ),
          ),
        );
      },
      child: AnimatedContainer(
        duration: const Duration(milliseconds: 300),
        padding: const EdgeInsets.all(18),
        decoration: BoxDecoration(
          color: bgColor,
          borderRadius: BorderRadius.circular(24),
          border: Border.all(color: borderColor, width: 1.5),
          boxShadow: [
            BoxShadow(
              color: statusColor.withOpacity(0.04),
              blurRadius: 16,
              offset: const Offset(0, 8),
            ),
          ],
        ),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // Başlık satırı
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                Row(
                  children: [
                    Icon(Icons.sensors_rounded, color: statusColor, size: 20),
                    const SizedBox(width: 8),
                    const Text(
                      'Canlı Ölçüm Takibi',
                      style: TextStyle(
                        fontSize: 14,
                        fontWeight: FontWeight.bold,
                        color: Color(0xFF1E293B),
                      ),
                    ),
                  ],
                ),
                // SignalR Bağlantı göstergesi
                Container(
                  padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
                  decoration: BoxDecoration(
                    color: widget.liveHubStatus == "Bağlandı"
                        ? const Color(0xFFDCFCE7)
                        : const Color(0xFFFEE2E2),
                    borderRadius: BorderRadius.circular(8),
                  ),
                  child: Text(
                    widget.liveHubStatus == "Bağlandı" ? 'Canlı' : 'Bağlanıyor...',
                    style: TextStyle(
                      fontSize: 10,
                      fontWeight: FontWeight.bold,
                      color: widget.liveHubStatus == "Bağlandı"
                          ? const Color(0xFF15803D)
                          : const Color(0xFFB91C1C),
                    ),
                  ),
                ),
              ],
            ),
            const SizedBox(height: 18),

            // Kalp Hızı Ölçümü ve EKG Dalgası satırı
            Row(
              children: [
                // Kalp İkonu ve Nabız Değeri
                ScaleTransition(
                  scale: _heartScaleAnim,
                  child: Container(
                    padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 12),
                    decoration: BoxDecoration(
                      color: Colors.white,
                      borderRadius: BorderRadius.circular(16),
                      boxShadow: const [
                        BoxShadow(
                          color: Color.fromRGBO(0, 0, 0, 0.03),
                          blurRadius: 6,
                          offset: Offset(0, 2),
                        ),
                      ],
                    ),
                    child: Row(
                      children: [
                        Icon(
                          Icons.favorite,
                          color: widget.liveHasSignal ? const Color(0xFFEF4444) : Colors.grey,
                          size: 26,
                        ),
                        const SizedBox(width: 10),
                        Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Text(
                              widget.liveHeartRate > 0 ? '${widget.liveHeartRate.round()}' : '--',
                              style: TextStyle(
                                fontSize: 24,
                                fontWeight: FontWeight.bold,
                                color: statusColor,
                              ),
                            ),
                            const Text(
                              'BPM',
                              style: TextStyle(
                                fontSize: 10,
                                fontWeight: FontWeight.bold,
                                color: Color(0xFF64748B),
                              ),
                            ),
                          ],
                        ),
                      ],
                    ),
                  ),
                ),
                const SizedBox(width: 14),

                // Canlı EKG Dalgası Canvası
                Expanded(
                  child: EcgLiveWave(
                    bpm: widget.liveHeartRate,
                    hasEcgData: widget.liveHasSignal,
                    color: statusColor,
                  ),
                ),
              ],
            ),
            const SizedBox(height: 14),

            // Durum etiketi ve diğer veriler (SpO2 & Ateş) satırı
            Row(
              mainAxisAlignment: MainAxisAlignment.start,
              children: [
                // Durum Rozeti
                Container(
                  padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 5),
                  decoration: BoxDecoration(
                    color: statusColor.withOpacity(0.08),
                    borderRadius: BorderRadius.circular(8),
                  ),
                  child: Text(
                    statusText,
                    style: TextStyle(
                      fontSize: 10,
                      fontWeight: FontWeight.bold,
                      color: statusColor,
                    ),
                  ),
                ),
              ],
            ),
            const SizedBox(height: 12),
            const Divider(height: 1, color: Color(0xFFE2E8F0)),
            const SizedBox(height: 12),
            GestureDetector(
              onTap: () {
                Navigator.push(
                  context,
                  MaterialPageRoute(
                    builder: (_) => BiometricsHistoryScreen(
                      studentId: widget.studentId,
                      studentName: widget.studentName,
                      token: widget.token,
                      studentAge: widget.studentAge,
                    ),
                  ),
                );
              },
              child: Row(
                mainAxisAlignment: MainAxisAlignment.center,
                children: const [
                  Icon(Icons.history_rounded, size: 14, color: Color(0xFF6366F1)),
                  SizedBox(width: 6),
                  Text(
                    'Ölçüm Geçmişini İncele',
                    style: TextStyle(
                      fontSize: 12,
                      fontWeight: FontWeight.bold,
                      color: Color(0xFF6366F1),
                    ),
                  ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildSubMetric(IconData icon, String value, Color color) {
    return Row(
      children: [
        Icon(icon, color: color.withOpacity(0.8), size: 14),
        const SizedBox(width: 4),
        Text(
          value,
          style: const TextStyle(
            fontSize: 11,
            fontWeight: FontWeight.bold,
            color: Color(0xFF475569),
          ),
        ),
      ],
    );
  }
}
