import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../domain/entities/classroom_activity.dart';
import '../../domain/entities/classroom_daily_record.dart';
import '../../domain/entities/classroom_summary.dart';
import '../../domain/entities/student_meal_record.dart';
import '../../domain/entities/weekly_meal_plan.dart';
import '../../domain/repositories/teacher_repository.dart';
import '../../data/repositories/teacher_repository_impl.dart';
import '../../../auth/domain/entities/login_result.dart';
import '../../../auth/domain/entities/student.dart';

final currentTeacherProvider = StateProvider<LoginResult?>((ref) => null);

final teacherRepositoryProvider = Provider<TeacherRepository>((ref) {
  final loginResult = ref.watch(currentTeacherProvider);
  final token = loginResult?.token ?? '';
  return TeacherRepositoryImpl(token);
});

final teacherClassroomListProvider = FutureProvider<List<ClassroomSummary>>((
  ref,
) async {
  final repository = ref.watch(teacherRepositoryProvider);
  return repository.getTeacherClasses();
});

final classroomStudentsProvider = FutureProvider.family<List<Student>, String>((
  ref,
  classroomId,
) async {
  final repository = ref.watch(teacherRepositoryProvider);
  return repository.getStudentsForClass(classroomId);
});

final classroomDailyRecordsProvider =
    FutureProvider.family<List<ClassroomDailyRecord>, String>(
      (ref, classroomId) async {
        final repository = ref.watch(teacherRepositoryProvider);
        return repository.getClassroomDailyRecords(classroomId);
      },
    );

final classroomMealRecordsProvider =
    FutureProvider.family<List<StudentMealRecord>, String>(
      (ref, classroomId) async {
        final repository = ref.watch(teacherRepositoryProvider);
        return repository.getClassroomMealRecords(classroomId);
      },
    );
final classroomWeeklyMealPlansProvider =
    FutureProvider.family<List<WeeklyMealPlan>, String>(
      (ref, classroomId) async {
        final repository = ref.watch(teacherRepositoryProvider);
        return repository.getClassroomWeeklyMealPlans(classroomId);
      },
    );
final classroomActivitiesProvider =
    FutureProvider.family<List<ClassroomActivity>, String>(
      (ref, classroomId) async {
        final repository = ref.watch(teacherRepositoryProvider);
        return repository.getClassroomActivities(classroomId);
      },
    );
