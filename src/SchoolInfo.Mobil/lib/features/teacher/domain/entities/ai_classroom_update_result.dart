class AIClassroomUpdateResult {
  final bool success;
  final String message;
  final List<String> updatedStudents;

  AIClassroomUpdateResult({
    required this.success,
    required this.message,
    required this.updatedStudents,
  });

  factory AIClassroomUpdateResult.fromJson(Map<String, dynamic> json) {
    final updated = json['updatedStudents'] as List<dynamic>? ??
        json['UpdatedStudents'] as List<dynamic>? ??
        [];
    return AIClassroomUpdateResult(
      success: json['success'] as bool? ?? json['Success'] as bool? ?? false,
      message: json['message'] as String? ?? json['Message'] as String? ?? '',
      updatedStudents: updated.map((e) => e.toString()).toList(),
    );
  }
}
