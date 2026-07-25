import 'dart:math' as math;
import 'package:flutter/material.dart';

/// Canlı EKG dalgasını çizen animasyonlu widget.
class EcgLiveWave extends StatefulWidget {
  final double bpm;
  final bool hasEcgData;
  final Color color;

  const EcgLiveWave({
    super.key,
    required this.bpm,
    required this.hasEcgData,
    required this.color,
  });

  @override
  State<EcgLiveWave> createState() => _EcgLiveWaveState();
}

class _EcgLiveWaveState extends State<EcgLiveWave> with SingleTickerProviderStateMixin {
  late AnimationController _controller;
  List<double?> _waveData = [];
  double _scanX = 0;
  double _lastTime = 0;

  @override
  void initState() {
    super.initState();
    // 60 FPS çizim için sürekli dönen bir animasyon tetikleyicisi
    _controller = AnimationController(
      vsync: this,
      duration: const Duration(seconds: 1),
    )..addListener(_tick)..repeat();
  }

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }

  void _tick() {
    if (!mounted) return;

    final double width = 220.0; // Sabit dalga genişliği (Kart içi için uygun)
    if (_waveData.length != width.toInt()) {
      _waveData = List<double?>.filled(width.toInt(), null);
    }

    final double now = DateTime.now().millisecondsSinceEpoch / 1000.0;
    if (_lastTime == 0) {
      _lastTime = now;
    }
    final double dt = now - _lastTime;
    _lastTime = now;

    // Nabız hızına göre dalga boyu
    final double bpm = widget.bpm > 0 ? widget.bpm : 75.0;
    final double wavelength = (60.0 / bpm) * 110.0; // Pikler arası genişlik

    // Tarama hızı
    final double speed = 110.0 * dt; 
    final double nextScanX = (_scanX + speed) % width;

    int startX = _scanX.floor();
    int endX = nextScanX.floor();

    // EKG dalga formülünü hesaplayıp diziye doldurur
    void fillPoint(int x) {
      if (x < 0 || x >= _waveData.length) return;
      final double phase = (x % wavelength) / wavelength;
      _waveData[x] = _getEcgY(phase, widget.hasEcgData);
    }

    if (endX < startX) {
      // Ekran sonundan başa geçiş
      for (int x = startX; x < width.toInt(); x++) {
        fillPoint(x);
      }
      for (int x = 0; x <= endX; x++) {
        fillPoint(x);
      }
    } else {
      for (int x = startX; x <= endX; x++) {
        fillPoint(x);
      }
    }

    _scanX = nextScanX;

    // Tarama ucunun önündeki eski verileri temizler (Silgi boşluğu - 12 piksel)
    final int gapSize = 12;
    for (int i = 1; i <= gapSize; i++) {
      int clearX = ((_scanX + i) % width).floor();
      if (clearX >= 0 && clearX < _waveData.length) {
        _waveData[clearX] = null;
      }
    }

    setState(() {});
  }

  double _getEcgY(double t, bool hasEcgData) {
    if (!hasEcgData) {
      // Veri yoksa düz izoelektrik hat, ufacık gürültülü
      return 0.02 * math.sin(t * math.pi * 10) * math.sin(t * math.pi * 2);
    }

    // Keskin ve Gerçekçi EKG Dalga Formülü (PQRST)
    // P dalgası
    double pVal = 0.0;
    if (t >= 0.05 && t <= 0.15) {
      pVal = 0.12 * math.sin((t - 0.05) / 0.1 * math.pi);
    }

    // QRS kompleksi (keskin iğneler)
    double qrsVal = 0.0;
    if (t >= 0.17 && t <= 0.23) {
      double qrsPhase = (t - 0.17) / 0.06;
      if (qrsPhase < 0.25) {
        qrsPhase = qrsPhase / 0.25;
        qrsVal = -0.25 * qrsPhase;
      } else if (qrsPhase < 0.75) {
        qrsPhase = (qrsPhase - 0.25) / 0.50;
        qrsVal = -0.25 + (1.55 * qrsPhase);
      } else {
        qrsPhase = (qrsPhase - 0.75) / 0.25;
        qrsVal = 1.30 - (1.50 * qrsPhase);
      }
    }

    // T dalgası
    double tVal = 0.0;
    if (t >= 0.35 && t <= 0.55) {
      tVal = 0.28 * math.sin((t - 0.35) / 0.20 * math.pi);
    }

    // Gerçekçi kılcal gürültü
    double noise = 0.015 * math.sin(t * math.pi * 32);

    return pVal + qrsVal + tVal + noise;
  }

  @override
  Widget build(BuildContext context) {
    return Container(
      width: 220,
      height: 48,
      decoration: BoxDecoration(
        color: Colors.black.withOpacity(0.02),
        borderRadius: BorderRadius.circular(12),
        border: Border.all(color: widget.color.withOpacity(0.08)),
      ),
      child: ClipRRect(
        borderRadius: BorderRadius.circular(12),
        child: CustomPaint(
          painter: _EcgPainter(
            waveData: _waveData,
            color: widget.color,
            scanX: _scanX,
          ),
        ),
      ),
    );
  }
}

class _EcgPainter extends CustomPainter {
  final List<double?> waveData;
  final Color color;
  final double scanX;

  _EcgPainter({
    required this.waveData,
    required this.color,
    required this.scanX,
  });

  @override
  void paint(Canvas canvas, Size size) {
    if (waveData.isEmpty) return;

    final double centerY = size.height / 2;
    final double amplitude = size.height * 0.32;

    // 1. Durum Rengine Duyarlı Hafif Izgara Çizimi
    final Paint gridPaint = Paint()
      ..color = color.withOpacity(0.04)
      ..strokeWidth = 0.8;

    final double gridSize = 10.0;
    for (double x = 0; x < size.width; x += gridSize) {
      canvas.drawLine(Offset(x, 0), Offset(x, size.height), gridPaint);
    }
    for (double y = 0; y < size.height; y += gridSize) {
      canvas.drawLine(Offset(0, y), Offset(size.width, y), gridPaint);
    }

    // Merkez kılavuz çizgisi
    final Paint baselinePaint = Paint()
      ..color = color.withOpacity(0.08)
      ..strokeWidth = 1.0;
    canvas.drawLine(Offset(0, centerY), Offset(size.width, centerY), baselinePaint);

    // 2. EKG Çizgisi Çizimi
    final Paint linePaint = Paint()
      ..color = color
      ..strokeWidth = 1.8
      ..style = PaintingStyle.stroke
      ..strokeCap = StrokeCap.round
      ..strokeJoin = StrokeJoin.round;

    final Path path = Path();
    bool isDrawing = false;

    for (int x = 0; x < waveData.length; x++) {
      // Ekrana ölçekle
      final double drawX = (x / waveData.length) * size.width;
      final double? valY = waveData[x];

      if (valY == null) {
        isDrawing = false;
      } else {
        final double drawY = centerY - (valY * amplitude);
        if (!isDrawing) {
          path.moveTo(drawX, drawY);
          isDrawing = true;
        } else {
          path.lineTo(drawX, drawY);
        }
      }
    }
    canvas.drawPath(path, linePaint);

    // 3. Parlayan Uç Nokta Çizimi
    if (scanX < waveData.length) {
      final double targetX = (scanX / waveData.length) * size.width;
      final double? valY = waveData[scanX.floor()];
      if (valY != null) {
        final double targetY = centerY - (valY * amplitude);

        // Beyaz nokta
        final Paint dotPaint = Paint()..color = Colors.white;
        canvas.drawCircle(Offset(targetX, targetY), 2.2, dotPaint);

        // Dış parlaması
        final Paint glowPaint = Paint()
          ..color = color.withOpacity(0.4)
          ..maskFilter = const MaskFilter.blur(BlurStyle.normal, 4);
        canvas.drawCircle(Offset(targetX, targetY), 4.5, glowPaint);
      }
    }
  }

  @override
  bool shouldRepaint(covariant _EcgPainter oldDelegate) {
    return true; // Her animasyon adımında tekrar çizilmeli
  }
}
