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
        // Ã–nce bekleyen migration'larÄ± uygula
        await context.Database.MigrateAsync();

        // EÄŸer okul varsa veritabanÄ± daha Ã¶nce seed edilmiÅŸtir, Ã§Ä±k.
        if (await context.Set<School>().AnyAsync())
        {
            return;
        }

        // 1. Okul OluÅŸtur
        var schoolId = Guid.NewGuid();
        var school = new School("Ã–zel Bilgi OkullarÄ±");
        // School entity'si BaseEntity'den miras aldÄ±ÄŸÄ± iÃ§in SchoolId property'si var. Onu da aynÄ± ID'ye eÅŸitleyelim.
        school.SchoolId = schoolId;
        typeof(School).GetProperty("Id")?.SetValue(school, schoolId); // ID'yi sabitliyoruz ki iliÅŸkilerde kullanalÄ±m
        
        await context.Set<School>().AddAsync(school);

        // 2. SÄ±nÄ±f OluÅŸtur
        var classroomId = Guid.NewGuid();
        var classroom = new Classroom("3/A SÄ±nÄ±fÄ±", schoolId);
        typeof(Classroom).GetProperty("Id")?.SetValue(classroom, classroomId);

        await context.Set<Classroom>().AddAsync(classroom);

        // 3. Ã–ÄŸrenci OluÅŸtur
        var studentId = Guid.NewGuid();
        var student = new Student("Ali", "YÄ±lmaz", new DateTime(2016, 5, 10, 0, 0, 0, DateTimeKind.Utc), classroomId);
        student.SchoolId = schoolId;
        typeof(Student).GetProperty("Id")?.SetValue(student, studentId);

        await context.Set<Student>().AddAsync(student);

        // Åifre: "123456"
        var defaultPasswordHash = BCrypt.Net.BCrypt.HashPassword("123456");

        // 4. Admin KullanÄ±cÄ±sÄ± OluÅŸtur
        var admin = new User("Sistem", "YÃ¶neticisi", "admin@school.com", UserRole.Admin);
        admin.PasswordHash = defaultPasswordHash;
        admin.SchoolId = schoolId;
        await context.Set<User>().AddAsync(admin);

        // 5. Ã–ÄŸretmen KullanÄ±cÄ±sÄ± OluÅŸtur
        var teacher = new User("AyÅŸe", "Ã–ÄŸretmen", "ayse@school.com", UserRole.Teacher);
        teacher.PasswordHash = defaultPasswordHash;
        teacher.SchoolId = schoolId;
        await context.Set<User>().AddAsync(teacher);

        // 6. Veli KullanÄ±cÄ±sÄ± OluÅŸtur
        var parent = new User("Ahmet", "YÄ±lmaz", "veli@school.com", UserRole.Parent);
        parent.PasswordHash = defaultPasswordHash;
        parent.SchoolId = schoolId;
        await context.Set<User>().AddAsync(parent);

        // 7. GÃ¼nlÃ¼k KayÄ±t OluÅŸtur (Ã–ÄŸrenci Ali YÄ±lmaz iÃ§in)
        var dailyRecordId = Guid.NewGuid();
        var dailyRecord = new DailyRecord(studentId, DateTime.UtcNow);
        dailyRecord.SchoolId = schoolId;
        typeof(DailyRecord).GetProperty("Id")?.SetValue(dailyRecord, dailyRecordId);
        dailyRecord.UpdateSleepInfo(new SchoolInfo.Domain.ValueObjects.SleepData(SleepStatus.SleptWell, DateTime.UtcNow.Date.AddHours(-11), DateTime.UtcNow.Date.AddHours(-2)));
        dailyRecord.UpdateWaterConsumption(new SchoolInfo.Domain.ValueObjects.WaterIntake(500));
        dailyRecord.SetTeacherNote("Ali bugÃ¼n Ã§ok neÅŸeliydi.");
        await context.Set<DailyRecord>().AddAsync(dailyRecord);

        // 8. Ã–ÄŸÃ¼n KaydÄ± OluÅŸtur
        var mealRecord = new MealRecord(dailyRecordId, "Ã–ÄŸle YemeÄŸi", new SchoolInfo.Domain.ValueObjects.MealStatus(MealStatusType.All, "Hepsini yedi"));
        mealRecord.SchoolId = schoolId;
        await context.Set<MealRecord>().AddAsync(mealRecord);

        // VeritabanÄ±na kaydet
        await context.SaveChangesAsync();
    }
}
