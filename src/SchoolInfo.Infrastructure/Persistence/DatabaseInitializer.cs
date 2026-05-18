using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SchoolInfo.Domain.Entities;
using SchoolInfo.Domain.Enums;
using SchoolInfo.Domain.ValueObjects;

namespace SchoolInfo.Infrastructure.Persistence;

/// <summary>
/// Veritabanının ilk çalışmasında tabloları çoklu okul (multi-tenant) yapısına uygun 
/// ilişkisel gerçekçi test verileri ile tohumlayan (seeding) başlatıcı sınıf.
/// </summary>
public static class DatabaseInitializer
{
    private static void SetId(object entity, string idStr)
    {
        var property = typeof(SchoolInfo.Domain.Common.BaseEntity).GetProperty("Id");
        if (property != null)
        {
            property.SetValue(entity, Guid.Parse(idStr));
        }
    }

    public static async Task SeedAsync(AppDbContext context)
    {
        // 1. Bekleyen migration'ları uygula (Veritabanı yoksa oluşturulur)
        await context.Database.MigrateAsync();

        // 2. Geliştirme ortamında seed kararlılığı için eski verileri temizle
        context.Set<DailySummary>().RemoveRange(context.Set<DailySummary>());
        context.Set<MealRecord>().RemoveRange(context.Set<MealRecord>());
        context.Set<DailyRecord>().RemoveRange(context.Set<DailyRecord>());
        context.Set<Activity>().RemoveRange(context.Set<Activity>());
        context.Set<Student>().RemoveRange(context.Set<Student>());
        context.Set<User>().RemoveRange(context.Set<User>());
        context.Set<Classroom>().RemoveRange(context.Set<Classroom>());
        context.Set<School>().RemoveRange(context.Set<School>());
        await context.SaveChangesAsync();

        // Şifre: "123456" (Hız için tek sefer hash'liyoruz)
        var defaultPasswordHash = BCrypt.Net.BCrypt.HashPassword("123456");

        var today = DateTime.UtcNow.Date;
        var yesterday = today.AddDays(-1);

        // ==========================================
        // 🏫 OKUL 1: YILDIZ ANAOKULU
        // ==========================================
        var school1 = new School("Yıldız Anaokulu");
        SetId(school1, "10b06b00-349f-4318-97be-fb14c330f81d");
        school1.SchoolId = school1.Id;
        await context.Set<School>().AddAsync(school1);

        // --- Sınıflar ---
        var class1 = new Classroom("Papatyalar Sınıfı", school1.Id);
        SetId(class1, "80b06b00-349f-4318-97be-fb14c330f81d");
        await context.Set<Classroom>().AddAsync(class1);

        var class2 = new Classroom("Kelebekler Sınıfı", school1.Id);
        SetId(class2, "90b06b00-349f-4318-97be-fb14c330f81d");
        await context.Set<Classroom>().AddAsync(class2);

        // --- Kullanıcılar (Admin, Öğretmenler, Veliler) ---
        var admin1 = new User("Sibel", "Kaya", "admin1@yildiz.com", UserRole.Admin);
        admin1.PasswordHash = defaultPasswordHash;
        admin1.SchoolId = school1.Id;
        await context.Set<User>().AddAsync(admin1);

        var teacher1_1 = new User("Fatma", "Demir", "fatma@yildiz.com", UserRole.Teacher);
        teacher1_1.PasswordHash = defaultPasswordHash;
        teacher1_1.SchoolId = school1.Id;
        await context.Set<User>().AddAsync(teacher1_1);

        var teacher1_2 = new User("Merve", "Yılmaz", "merve@yildiz.com", UserRole.Teacher);
        teacher1_2.PasswordHash = defaultPasswordHash;
        teacher1_2.SchoolId = school1.Id;
        await context.Set<User>().AddAsync(teacher1_2);

        var parent1_1 = new User("Hasan", "Şahin", "hasan@yildiz.com", UserRole.Parent);
        parent1_1.PasswordHash = defaultPasswordHash;
        parent1_1.SchoolId = school1.Id;
        await context.Set<User>().AddAsync(parent1_1);

        var parent1_2 = new User("Ayşe", "Çelik", "ayse@yildiz.com", UserRole.Parent);
        parent1_2.PasswordHash = defaultPasswordHash;
        parent1_2.SchoolId = school1.Id;
        await context.Set<User>().AddAsync(parent1_2);

        // --- Öğretmen-Sınıf Atamaları (Çoktan Çoğa) ---
        class1.AddTeacher(teacher1_1);
        class2.AddTeacher(teacher1_2);

        // --- Öğrenciler ---
        var student1_1 = new Student("Efe", "Şahin", new DateTime(2018, 4, 12, 0, 0, 0, DateTimeKind.Utc), class1.Id);
        SetId(student1_1, "60c04a00-333e-42ef-9f3b-fa14c330f81d");
        student1_1.SchoolId = school1.Id;
        student1_1.AddParent(parent1_1);
        await context.Set<Student>().AddAsync(student1_1);

        var student1_2 = new Student("Zeynep", "Çelik", new DateTime(2019, 9, 23, 0, 0, 0, DateTimeKind.Utc), class2.Id);
        SetId(student1_2, "70c04a00-333e-42ef-9f3b-fa14c330f81d");
        student1_2.SchoolId = school1.Id;
        student1_2.AddParent(parent1_2);
        await context.Set<Student>().AddAsync(student1_2);

        // --- Aktiviteler (Yıldız Anaokulu) ---
        var activity1_1_yesterday = new Activity("Parmak Boyama", "Sulu boyalar ile el becerilerini geliştirici boyama yapıldı.", yesterday, class1.Id);
        activity1_1_yesterday.SchoolId = school1.Id;
        activity1_1_yesterday.Complete();
        await context.Set<Activity>().AddAsync(activity1_1_yesterday);

        var activity1_1_today = new Activity("Drama Etkinliği", "Ormandaki hayvanlar konulu canlandırma ve drama çalışması yapıldı.", today, class1.Id);
        activity1_1_today.SchoolId = school1.Id;
        await context.Set<Activity>().AddAsync(activity1_1_today);

        var activity1_2_yesterday = new Activity("İngilizce Şarkılar", "İngilizce renkler ve hayvanlar konulu ritmik şarkılar söylendi.", yesterday, class2.Id);
        activity1_2_yesterday.SchoolId = school1.Id;
        activity1_2_yesterday.Complete();
        await context.Set<Activity>().AddAsync(activity1_2_yesterday);

        var activity1_2_today = new Activity("Origami Katlama", "Kağıttan kurbağa ve uçak yapımı ile ince motor kasları çalıştırıldı.", today, class2.Id);
        activity1_2_today.SchoolId = school1.Id;
        await context.Set<Activity>().AddAsync(activity1_2_today);


        // ==========================================
        // 🏫 OKUL 2: GÜNEŞ KREŞİ
        // ==========================================
        var school2 = new School("Güneş Kreşi");
        SetId(school2, "20b06b00-349f-4318-97be-fb14c330f81d");
        school2.SchoolId = school2.Id;
        await context.Set<School>().AddAsync(school2);

        // --- Sınıflar ---
        var class3 = new Classroom("Bulutlar Sınıfı", school2.Id);
        SetId(class3, "a0b06b00-349f-4318-97be-fb14c330f81d");
        await context.Set<Classroom>().AddAsync(class3);

        var class4 = new Classroom("Gökkuşağı Sınıfı", school2.Id);
        SetId(class4, "b0b06b00-349f-4318-97be-fb14c330f81d");
        await context.Set<Classroom>().AddAsync(class4);

        // --- Kullanıcılar ---
        var admin2 = new User("Kemal", "Öztürk", "admin2@gunes.com", UserRole.Admin);
        admin2.PasswordHash = defaultPasswordHash;
        admin2.SchoolId = school2.Id;
        await context.Set<User>().AddAsync(admin2);

        var teacher2_1 = new User("Zehra", "Aydın", "zehra@gunes.com", UserRole.Teacher);
        teacher2_1.PasswordHash = defaultPasswordHash;
        teacher2_1.SchoolId = school2.Id;
        await context.Set<User>().AddAsync(teacher2_1);

        var teacher2_2 = new User("Elif", "Şen", "elif@gunes.com", UserRole.Teacher);
        teacher2_2.PasswordHash = defaultPasswordHash;
        teacher2_2.SchoolId = school2.Id;
        await context.Set<User>().AddAsync(teacher2_2);

        var parent2_1 = new User("Murat", "Koç", "murat@gunes.com", UserRole.Parent);
        parent2_1.PasswordHash = defaultPasswordHash;
        parent2_1.SchoolId = school2.Id;
        await context.Set<User>().AddAsync(parent2_1);

        var parent2_2 = new User("Derya", "Aslan", "derya@gunes.com", UserRole.Parent);
        parent2_2.PasswordHash = defaultPasswordHash;
        parent2_2.SchoolId = school2.Id;
        await context.Set<User>().AddAsync(parent2_2);

        // --- Öğretmen-Sınıf Atamaları ---
        class3.AddTeacher(teacher2_1);
        class4.AddTeacher(teacher2_2);

        // --- Öğrenciler ---
        var student2_1 = new Student("Can", "Koç", new DateTime(2017, 2, 10, 0, 0, 0, DateTimeKind.Utc), class3.Id);
        SetId(student2_1, "60c04a00-333e-42ef-9f3b-fa14c330f82d");
        student2_1.SchoolId = school2.Id;
        student2_1.AddParent(parent2_1);
        await context.Set<Student>().AddAsync(student2_1);

        var student2_2 = new Student("Eylül", "Aslan", new DateTime(2018, 11, 3, 0, 0, 0, DateTimeKind.Utc), class4.Id);
        SetId(student2_2, "70c04a00-333e-42ef-9f3b-fa14c330f82d");
        student2_2.SchoolId = school2.Id;
        student2_2.AddParent(parent2_2);
        await context.Set<Student>().AddAsync(student2_2);

        // --- Aktiviteler (Güneş Kreşi) ---
        var activity2_1_yesterday = new Activity("Legolarla Tasarım", "Lego blokları kullanarak çeşitli geometrik kuleler inşa edildi.", yesterday, class3.Id);
        activity2_1_yesterday.SchoolId = school2.Id;
        activity2_1_yesterday.Complete();
        await context.Set<Activity>().AddAsync(activity2_1_yesterday);

        var activity2_1_today = new Activity("Bahçe Oyunları", "Açık havada saklambaç ve yakalamaç oynanarak fiziksel aktivite sağlandı.", today, class3.Id);
        activity2_1_today.SchoolId = school2.Id;
        await context.Set<Activity>().AddAsync(activity2_1_today);

        var activity2_2_yesterday = new Activity("Müzik ve Ritim", "Ritim aletleri (tef, marakas) eşliğinde ses ve ritim çalışmaları yapıldı.", yesterday, class4.Id);
        activity2_2_yesterday.SchoolId = school2.Id;
        activity2_2_yesterday.Complete();
        await context.Set<Activity>().AddAsync(activity2_2_yesterday);

        var activity2_2_today = new Activity("Doğa Keşfi", "Okul bahçesindeki yapraklar ve böcekler büyüteçle incelenerek gözlem yapıldı.", today, class4.Id);
        activity2_2_today.SchoolId = school2.Id;
        await context.Set<Activity>().AddAsync(activity2_2_today);


        // ======================================================================
        // 📝 GÜNLÜK KAYITLAR (DAILY RECORDS), ÖĞÜNLER VE AI ÖZETLERİ (SEED)
        // ======================================================================
        
        // Liste şeklinde tüm öğrencileri toplayalım ki döngüyle kayıt oluşturalım
        var students = new[]
        {
            new { Entity = student1_1, SchoolId = school1.Id, Parent = parent1_1 },
            new { Entity = student1_2, SchoolId = school1.Id, Parent = parent1_2 },
            new { Entity = student2_1, SchoolId = school2.Id, Parent = parent2_1 },
            new { Entity = student2_2, SchoolId = school2.Id, Parent = parent2_2 }
        };

        foreach (var item in students)
        {
            // --- 1. DÜNÜN KAYDI (YESTERDAY DAILY RECORD) ---
            var recYesterday = new DailyRecord(item.Entity.Id, yesterday);
            recYesterday.SchoolId = item.SchoolId;
            recYesterday.UpdateSleepInfo(new SleepData(SleepStatus.SleptWell, yesterday.AddHours(13), yesterday.AddHours(15)));
            recYesterday.UpdateWaterConsumption(new WaterIntake(600));
            recYesterday.SetTeacherNote($"{item.Entity.FirstName} dün arkadaşlarıyla çok uyumluydu ve tüm grup aktivitelerine neşeyle katıldı.");
            await context.Set<DailyRecord>().AddAsync(recYesterday);

            // Dünün Öğünleri
            var b1 = new MealRecord(recYesterday.Id, "Kahvaltı", new MealStatus(MealStatusType.All, "Tüm tabağını bitirdi."));
            b1.SchoolId = item.SchoolId;
            await context.Set<MealRecord>().AddAsync(b1);

            var l1 = new MealRecord(recYesterday.Id, "Öğle Yemeği", new MealStatus(MealStatusType.Half, "Çorbasını içti, köftenin yarısını yedi."));
            l1.SchoolId = item.SchoolId;
            await context.Set<MealRecord>().AddAsync(l1);

            var s1 = new MealRecord(recYesterday.Id, "İkindi Kahvaltısı", new MealStatus(MealStatusType.All, "Meyve salatasının tamamını afiyetle tüketti."));
            s1.SchoolId = item.SchoolId;
            await context.Set<MealRecord>().AddAsync(s1);

            // Dünün Yapay Zeka Özeti (DailySummary)
            var summaryYesterday = new DailySummary(
                item.Entity.Id, 
                yesterday, 
                $"Bugün {item.Entity.FirstName} okulda harika bir gün geçirdi! Sabah kahvaltısını iştahla bitirdikten sonra arkadaşlarıyla ritim çalışmalarına katıldı. Öğle uykusunda mışıl mışıl çok tatlı uyudu ve dinlenmiş bir şekilde uyandı. Gün boyu neşesi yerindeydi, arkadaşlarına çok yardımcı oldu."
            );
            summaryYesterday.SchoolId = item.SchoolId;
            summaryYesterday.MarkAsRead(); // Dün okundu olarak işaretleyelim
            await context.Set<DailySummary>().AddAsync(summaryYesterday);


            // --- 2. BUGÜNÜN KAYDI (TODAY DAILY RECORD) ---
            var recToday = new DailyRecord(item.Entity.Id, today);
            recToday.SchoolId = item.SchoolId;
            recToday.UpdateSleepInfo(new SleepData(SleepStatus.SleptLittle, today.AddHours(13).AddMinutes(30), today.AddHours(14)));
            recToday.UpdateWaterConsumption(new WaterIntake(400));
            recToday.SetTeacherNote($"{item.Entity.FirstName} bugün biraz uykusuz görünüyordu fakat oyun saatinde enerjisi oldukça yerine geldi.");
            await context.Set<DailyRecord>().AddAsync(recToday);

            // Bugünün Öğünleri
            var b2 = new MealRecord(recToday.Id, "Kahvaltı", new MealStatus(MealStatusType.Little, "Ekmeğini yedi, peyniri bitirmedi."));
            b2.SchoolId = item.SchoolId;
            await context.Set<MealRecord>().AddAsync(b2);

            var l2 = new MealRecord(recToday.Id, "Öğle Yemeği", new MealStatus(MealStatusType.All, "Makarna ve yoğurdun hepsini yedi."));
            l2.SchoolId = item.SchoolId;
            await context.Set<MealRecord>().AddAsync(l2);

            var s2 = new MealRecord(recToday.Id, "İkindi Kahvaltısı", new MealStatus(MealStatusType.None, "Keki yemek istemedi."));
            s2.SchoolId = item.SchoolId;
            await context.Set<MealRecord>().AddAsync(s2);
        }

        // 3. Değişiklikleri kaydet
        await context.SaveChangesAsync();
    }
}
