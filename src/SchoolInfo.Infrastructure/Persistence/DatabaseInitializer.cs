using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SchoolInfo.Domain.Entities;
using SchoolInfo.Domain.Enums;

namespace SchoolInfo.Infrastructure.Persistence;

public static class DatabaseInitializer
{
    public static async Task SeedAsync(AppDbContext context)
    {
        // Önce bekleyen migration'ları uygula
        await context.Database.MigrateAsync();

        // Eğer okul varsa veritabanı daha önce seed edilmiştir, çık.
        if (await context.Set<School>().AnyAsync())
        {
            return;
        }

        // 1. Okul Oluştur
        var schoolId = Guid.NewGuid();
        var school = new School("Özel Bilgi Okulları");
        // School entity'si BaseEntity'den miras aldığı için SchoolId property'si var. Onu da aynı ID'ye eşitleyelim.
        school.SchoolId = schoolId;
        typeof(School).GetProperty("Id")?.SetValue(school, schoolId); // ID'yi sabitliyoruz ki ilişkilerde kullanalım
        
        await context.Set<School>().AddAsync(school);

        // 2. Sınıf Oluştur
        var classroomId = Guid.NewGuid();
        var classroom = new Classroom("3/A Sınıfı", schoolId);
        typeof(Classroom).GetProperty("Id")?.SetValue(classroom, classroomId);

        await context.Set<Classroom>().AddAsync(classroom);

        // 3. Öğrenci Oluştur
        var studentId = Guid.NewGuid();
        var student = new Student("Ali", "Yılmaz", new DateTime(2016, 5, 10, 0, 0, 0, DateTimeKind.Utc), classroomId);
        student.SchoolId = schoolId;
        typeof(Student).GetProperty("Id")?.SetValue(student, studentId);

        await context.Set<Student>().AddAsync(student);

        // Şifre: "123456"
        var defaultPasswordHash = BCrypt.Net.BCrypt.HashPassword("123456");

        // 4. Admin Kullanıcısı Oluştur
        var admin = new User("Sistem", "Yöneticisi", "admin@school.com", UserRole.Admin);
        admin.PasswordHash = defaultPasswordHash;
        admin.SchoolId = schoolId;
        await context.Set<User>().AddAsync(admin);

        // 5. Öğretmen Kullanıcısı Oluştur
        var teacher = new User("Ayşe", "Öğretmen", "ayse@school.com", UserRole.Teacher);
        teacher.PasswordHash = defaultPasswordHash;
        teacher.SchoolId = schoolId;
        await context.Set<User>().AddAsync(teacher);

        // 6. Veli Kullanıcısı Oluştur
        var parent = new User("Ahmet", "Yılmaz", "veli@school.com", UserRole.Parent);
        parent.PasswordHash = defaultPasswordHash;
        parent.SchoolId = schoolId;
        await context.Set<User>().AddAsync(parent);

        // 7. Günlük Kayıt Oluştur (Öğrenci Ali Yılmaz için)
        var dailyRecordId = Guid.NewGuid();
        var dailyRecord = new DailyRecord(studentId, DateTime.UtcNow);
        dailyRecord.SchoolId = schoolId;
        typeof(DailyRecord).GetProperty("Id")?.SetValue(dailyRecord, dailyRecordId);
        dailyRecord.UpdateSleepInfo(new SchoolInfo.Domain.ValueObjects.SleepData(SleepStatus.SleptWell, DateTime.UtcNow.Date.AddHours(-11), DateTime.UtcNow.Date.AddHours(-2)));
        dailyRecord.UpdateWaterConsumption(new SchoolInfo.Domain.ValueObjects.WaterIntake(500));
        dailyRecord.SetTeacherNote("Ali bugün çok neşeliydi.");
        await context.Set<DailyRecord>().AddAsync(dailyRecord);

        // 8. Öğün Kaydı Oluştur
        var mealRecord = new MealRecord(dailyRecordId, "Öğle Yemeği", new SchoolInfo.Domain.ValueObjects.MealStatus(MealStatusType.All, "Hepsini yedi"));
        mealRecord.SchoolId = schoolId;
        await context.Set<MealRecord>().AddAsync(mealRecord);

        // Veritabanına kaydet
        await context.SaveChangesAsync();
    }
}
