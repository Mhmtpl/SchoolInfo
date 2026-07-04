import '../entities/classroom_activity.dart';
import '../entities/classroom_daily_record.dart';
import '../entities/classroom_summary.dart';
import '../entities/student_daily_record.dart';
import '../entities/student_meal_record.dart';
import '../entities/weekly_meal_plan.dart';
import '../entities/ai_classroom_update_result.dart';
import '../../../auth/domain/entities/student.dart';

abstract class TeacherRepository {
  Future<List<ClassroomSummary>> getTeacherClasses();
  Future<List<Student>> getStudentsForClass(String classroomId);
  Future<List<ClassroomDailyRecord>> getClassroomDailyRecords(
    String classroomId,
  );
  Future<List<StudentMealRecord>> getClassroomMealRecords(String classroomId);
  Future<List<WeeklyMealPlan>> getClassroomWeeklyMealPlans(String classroomId);
  Future<void> updateClassroomWeeklyMealPlans(
    String classroomId,
    List<WeeklyMealPlan> plans,
  );
  Future<List<ClassroomActivity>> getClassroomActivities(String classroomId);
  Future<void> createClassroomActivity({
    required String classroomId,
    required String title,
    required String description,
    required DateTime activityDate,
    required String startTime,
    required String endTime,
    required int type,
  });
  Future<void> completeActivity(String activityId);

  Future<StudentDailyRecord?> getStudentDailyRecordToday(String studentId);
  Future<String> createDailyRecord(String studentId);
  Future<void> updateDailyRecord({
    required String dailyRecordId,
    required int sleepStatus,
    required int waterAmountInMilliliters,
    String? teacherNote,
  });
  Future<void> updateMealRecord({
    required String studentId,
    required String mealRecordId,
    required int status,
    String? notes,
  });
  Future<void> saveMedicationRecord({
    required String studentId,
    required String medicineName,
    required String dosage,
    required String time,
    required bool taken,
    String? note,
  });

  Future<AIClassroomUpdateResult> aiUpdateClassroom({
    required String classroomId,
    required String command,
    required String dateStr,
  });
}
