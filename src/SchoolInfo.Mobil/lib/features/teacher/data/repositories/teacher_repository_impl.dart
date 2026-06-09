import 'dart:convert';

import 'package:http/http.dart' as http;

import '../../domain/entities/classroom_activity.dart';
import '../../domain/entities/classroom_daily_record.dart';
import '../../domain/entities/classroom_summary.dart';
import '../../domain/entities/student_daily_record.dart';
import '../../domain/entities/student_meal_record.dart';
import '../../domain/entities/weekly_meal_plan.dart';
import '../../domain/repositories/teacher_repository.dart';
import '../../../auth/domain/entities/student.dart';

class TeacherRepositoryImpl implements TeacherRepository {
  static const _baseUrl = 'http://10.0.2.2:53079';
  final String token;

  TeacherRepositoryImpl(this.token);

  Map<String, String> get _headers => {
    'Content-Type': 'application/json',
    'Authorization': 'Bearer $token',
  };

  @override
  Future<List<ClassroomSummary>> getTeacherClasses() async {
    final uri = Uri.parse('$_baseUrl/api/classrooms/teacher/my');
    final response = await http.get(uri, headers: _headers);
    if (response.statusCode != 200) {
      throw Exception('Sınıflar yüklenemedi: ${response.statusCode}');
    }

    final data = jsonDecode(response.body) as List<dynamic>;
    final summaries = await Future.wait(
      data.map((item) async {
        final map = item as Map<String, dynamic>;
        final id = map['id']?.toString() ?? map['Id']?.toString() ?? '';
        final name =
            map['name'] as String? ?? map['Name'] as String? ?? 'Sınıf';
        final students = await getStudentsForClass(id);
        final reportSent = students.isEmpty ? 0 : (students.length ~/ 2);
        return ClassroomSummary(
          id: id,
          name: name,
          studentCount: students.length,
          reportSent: reportSent,
        );
      }),
    );

    return summaries;
  }

  @override
  Future<List<Student>> getStudentsForClass(String classroomId) async {
    final uri = Uri.parse('$_baseUrl/api/classrooms/$classroomId/students');
    final response = await http.get(uri, headers: _headers);
    if (response.statusCode != 200) {
      throw Exception('Öğrenciler yüklenemedi: ${response.statusCode}');
    }

    final data = jsonDecode(response.body) as List<dynamic>;
    return data
        .map((item) => Student.fromJson(item as Map<String, dynamic>))
        .toList();
  }

  @override
  Future<List<ClassroomDailyRecord>> getClassroomDailyRecords(
    String classroomId,
  ) async {
    final uri = Uri.parse(
      '$_baseUrl/api/classrooms/$classroomId/daily-records/today',
    );
    final response = await http.get(uri, headers: _headers);
    if (response.statusCode != 200) {
      throw Exception('Günlük kayıtlar yüklenemedi: ${response.statusCode}');
    }

    final data = jsonDecode(response.body) as List<dynamic>;
    return data
        .map(
          (item) => ClassroomDailyRecord.fromJson(item as Map<String, dynamic>),
        )
        .toList();
  }

  @override
  Future<List<StudentMealRecord>> getClassroomMealRecords(
    String classroomId,
  ) async {
    final uri = Uri.parse(
      '$_baseUrl/api/classrooms/$classroomId/meal-records/today',
    );
    final response = await http.get(uri, headers: _headers);
    if (response.statusCode != 200) {
      throw Exception('Yemek kayıtları yüklenemedi: ${response.statusCode}');
    }

    final data = jsonDecode(response.body) as List<dynamic>;
    return data
        .map((item) => StudentMealRecord.fromJson(item as Map<String, dynamic>))
        .toList();
  }

  @override
  Future<List<WeeklyMealPlan>> getClassroomWeeklyMealPlans(
    String classroomId,
  ) async {
    final uri = Uri.parse('$_baseUrl/api/classrooms/$classroomId/weekly-meal-plans');
    final response = await http.get(uri, headers: _headers);
    if (response.statusCode != 200) {
      throw Exception('Haftalık yemek planları yüklenemedi: ${response.statusCode}');
    }

    final data = jsonDecode(response.body) as List<dynamic>;
    return data
        .map((item) => WeeklyMealPlan.fromJson(item as Map<String, dynamic>))
        .toList();
  }

  @override
  Future<void> updateClassroomWeeklyMealPlans(
    String classroomId,
    List<WeeklyMealPlan> plans,
  ) async {
    final uri = Uri.parse('$_baseUrl/api/classrooms/$classroomId/weekly-meal-plans');
    final body = jsonEncode({
      'plans': plans
          .map((plan) => {
                'id': plan.id,
                'dayOfWeek': plan.dayOfWeek,
                'mealName': plan.mealName,
                'plannedCalories': plan.plannedCalories,
                'foodContent': plan.foodContent,
                'proteinGrams': plan.proteinGrams,
                'carbsGrams': plan.carbsGrams,
              })
          .toList(),
    });

    final response = await http.put(uri, headers: _headers, body: body);
    if (response.statusCode != 200 && response.statusCode != 204) {
      throw Exception('Haftalık yemek planı güncellenemedi: ${response.statusCode}');
    }
  }

  @override
  Future<StudentDailyRecord?> getStudentDailyRecordToday(String studentId) async {
    final uri = Uri.parse('$_baseUrl/api/daily-records/student/$studentId/today');
    final response = await http.get(uri, headers: _headers);
    if (response.statusCode == 404) {
      return null;
    }
    if (response.statusCode != 200) {
      throw Exception('Günlük kayıt alınamadı: ${response.statusCode}');
    }

    final data = jsonDecode(response.body) as Map<String, dynamic>;
    return StudentDailyRecord.fromJson(data);
  }

  @override
  Future<String> createDailyRecord(String studentId) async {
    final uri = Uri.parse('$_baseUrl/api/daily-records');
    final body = jsonEncode({
      'studentId': studentId,
      'date': DateTime.now().toUtc().toIso8601String(),
    });

    final response = await http.post(uri, headers: _headers, body: body);
    if (response.statusCode != 201 && response.statusCode != 200) {
      throw Exception('Günlük kayıt oluşturulamadı: ${response.statusCode}');
    }

    final createdId = jsonDecode(response.body).toString();
    return createdId;
  }

  @override
  Future<void> updateDailyRecord({
    required String dailyRecordId,
    required int sleepStatus,
    required int waterAmountInMilliliters,
    String? teacherNote,
  }) async {
    final uri = Uri.parse('$_baseUrl/api/daily-records/$dailyRecordId');
    final body = jsonEncode({
      'dailyRecordId': dailyRecordId,
      'sleepStatus': sleepStatus,
      'sleepStartTime': null,
      'sleepEndTime': null,
      'waterAmountInMilliliters': waterAmountInMilliliters,
      'teacherNote': teacherNote,
    });

    final response = await http.put(uri, headers: _headers, body: body);
    if (response.statusCode != 204 && response.statusCode != 200) {
      throw Exception('Günlük kayıt güncellenemedi: ${response.statusCode}');
    }
  }

  @override
  Future<void> updateMealRecord({
    required String studentId,
    required String mealRecordId,
    required int status,
    String? notes,
  }) async {
    final uri = Uri.parse('$_baseUrl/api/meal-records/$studentId');
    final body = jsonEncode({
      'mealRecordId': mealRecordId,
      'statusType': status,
      'description': notes,
    });

    final response = await http.put(uri, headers: _headers, body: body);
    if (response.statusCode != 204 && response.statusCode != 200) {
      throw Exception('Yemek kaydı güncellenemedi: ${response.statusCode}');
    }
  }

  @override
  Future<void> saveMedicationRecord({
    required String studentId,
    required String medicineName,
    required String dosage,
    required String time,
    required bool taken,
    String? note,
  }) async {
    final uri = Uri.parse('$_baseUrl/api/medication-records');
    final body = jsonEncode({
      'studentId': studentId,
      'medicineName': medicineName,
      'dosage': dosage,
      'administrationTime': time,
      'taken': taken,
      'note': note,
    });

    final response = await http.post(uri, headers: _headers, body: body);
    if (response.statusCode != 201 && response.statusCode != 200) {
      throw Exception('İlaç kaydı oluşturulamadı: ${response.statusCode}');
    }
  }

  @override
  Future<List<ClassroomActivity>> getClassroomActivities(
    String classroomId,
  ) async {
    final uri = Uri.parse('$_baseUrl/api/activities/classroom/$classroomId');
    final response = await http.get(uri, headers: _headers);
    if (response.statusCode != 200) {
      throw Exception('Aktiviteler yüklenemedi: ${response.statusCode}');
    }

    final data = jsonDecode(response.body) as List<dynamic>;
    return data
        .map((item) => ClassroomActivity.fromJson(item as Map<String, dynamic>))
        .toList();
  }

  @override
  Future<void> createClassroomActivity({
    required String classroomId,
    required String title,
    required String description,
    required DateTime activityDate,
    required String startTime,
    required String endTime,
    required int type,
  }) async {
    final uri = Uri.parse('$_baseUrl/api/activities');
    final body = jsonEncode({
      'title': title,
      'description': description,
      'activityDate': activityDate.toIso8601String(),
      'startTime': startTime,
      'endTime': endTime,
      'type': type,
      'classroomId': classroomId,
    });

    final response = await http.post(uri, headers: _headers, body: body);
    if (response.statusCode != 201 && response.statusCode != 200) {
      throw Exception('Aktivite oluşturulamadı: ${response.statusCode}');
    }
  }

  @override
  Future<void> completeActivity(String activityId) async {
    final uri = Uri.parse('$_baseUrl/api/activities/$activityId/complete');
    final response = await http.put(uri, headers: _headers);
    if (response.statusCode != 204 && response.statusCode != 200) {
      throw Exception('Aktivite tamamlanamadı: ${response.statusCode}');
    }
  }
}
