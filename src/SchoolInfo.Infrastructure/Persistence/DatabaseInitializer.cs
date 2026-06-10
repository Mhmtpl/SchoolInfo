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

        // Veritabanında zaten veri varsa sıfırlama ve tekrar tohumlama yapma
        if (await context.Set<School>().AnyAsync())
        {
            return;
        }

        context.Set<ClassroomWeeklySchedule>().RemoveRange(context.Set<ClassroomWeeklySchedule>());
        context.Set<WeeklyMealPlan>().RemoveRange(context.Set<WeeklyMealPlan>());
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

        var today = DateTime.UtcNow.AddHours(3).Date;
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

        class1.AddTeacher(teacher1_1);
        class2.AddTeacher(teacher1_2);

        // --- Kullanıcılar (Veliler & Öğrenciler Döngüsü) ---
        var studentsData = new List<dynamic>();
        var random = new Random(42);
        var firstNames = new[] { "Ali", "Ayşe", "Mehmet", "Fatma", "Ahmet", "Zeynep", "Mustafa", "Elif", "Emre", "Defne", "Can", "Eylül", "Burak", "Ceren", "Deniz", "Ece", "Kaan", "Melis", "Mert", "Selin" };
        var lastNames = new[] { "Yılmaz", "Kaya", "Demir", "Şahin", "Çelik", "Yıldız", "Öztürk", "Aydın", "Özdemir", "Arslan" };

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

        class3.AddTeacher(teacher2_1);
        class4.AddTeacher(teacher2_2);

        var allClassrooms = new[] 
        { 
            new { Class = class1, SchoolId = school1.Id }, 
            new { Class = class2, SchoolId = school1.Id }, 
            new { Class = class3, SchoolId = school2.Id }, 
            new { Class = class4, SchoolId = school2.Id } 
        };

        foreach (var c in allClassrooms)
        {
            for (int i = 1; i <= 10; i++)
            {
                // Sabit Test Öğrencisi ve Velisi: Papatyalar sınıfının ilk öğrencisi Efe Şahin olsun.
                bool isTestStudent = (c.Class.Id == Guid.Parse("80b06b00-349f-4318-97be-fb14c330f81d") && i == 1);

                var fName = isTestStudent ? "Efe" : firstNames[random.Next(firstNames.Length)];
                var lName = isTestStudent ? "Şahin" : lastNames[random.Next(lastNames.Length)];
                var student = new Student(fName, lName, new DateTime(2018, random.Next(1, 13), random.Next(1, 28), 0, 0, 0, DateTimeKind.Utc), c.Class.Id);
                student.SchoolId = c.SchoolId;

                if (isTestStudent)
                {
                    SetId(student, "60c04a00-333e-42ef-9f3b-fa14c330f81d"); // Efe Şahin'e sabit ID veriyoruz
                }
                
                // Hasan Yıldız (Sabit Test Velisi)
                var parentEmail1 = isTestStudent ? "hasan@yildiz.com" : $"baba_{c.Class.Id.ToString().Substring(0,4)}_{i}@test.com";
                var parentName1 = isTestStudent ? "Hasan Yıldız" : $"{fName} Babası";
                
                var parent1 = new User(parentName1, lName, parentEmail1, UserRole.Parent);
                parent1.PasswordHash = defaultPasswordHash;
                parent1.SchoolId = c.SchoolId;
                
                var parent2 = new User($"{fName} Annesi", lName, $"anne_{c.Class.Id.ToString().Substring(0,4)}_{i}@test.com", UserRole.Parent);
                parent2.PasswordHash = defaultPasswordHash;
                parent2.SchoolId = c.SchoolId;

                await context.Set<User>().AddAsync(parent1);
                await context.Set<User>().AddAsync(parent2);

                student.AddParent(parent1);
                student.AddParent(parent2);

                await context.Set<Student>().AddAsync(student);

                studentsData.Add(new { Entity = student, SchoolId = c.SchoolId, Parent = parent1 });
            }
        }

        // --- Aktiviteler (Yıldız Anaokulu) ---
        var activity1_1_yesterday = new Activity("Parmak Boyama", "Sulu boyalar ile el becerilerini geliştirici boyama yapıldı.", yesterday, new TimeSpan(10, 0, 0), new TimeSpan(11, 0, 0), ActivityType.Art, class1.Id);
        activity1_1_yesterday.SchoolId = school1.Id;
        activity1_1_yesterday.Complete();
        await context.Set<Activity>().AddAsync(activity1_1_yesterday);

        var activity1_1_today = new Activity("Drama Etkinliği", "Ormandaki hayvanlar konulu canlandırma ve drama çalışması yapıldı.", today, new TimeSpan(14, 0, 0), new TimeSpan(15, 0, 0), ActivityType.Art, class1.Id);
        activity1_1_today.SchoolId = school1.Id;
        await context.Set<Activity>().AddAsync(activity1_1_today);

        var activity1_2_yesterday = new Activity("İngilizce Şarkılar", "İngilizce renkler ve hayvanlar konulu ritmik şarkılar söylendi.", yesterday, new TimeSpan(9, 30, 0), new TimeSpan(10, 30, 0), ActivityType.Music, class2.Id);
        activity1_2_yesterday.SchoolId = school1.Id;
        activity1_2_yesterday.Complete();
        await context.Set<Activity>().AddAsync(activity1_2_yesterday);

        var activity1_2_today = new Activity("Origami Katlama", "Kağıttan kurbağa ve uçak yapımı ile ince motor kasları çalıştırıldı.", today, new TimeSpan(11, 0, 0), new TimeSpan(12, 0, 0), ActivityType.Art, class2.Id);
        activity1_2_today.SchoolId = school1.Id;
        await context.Set<Activity>().AddAsync(activity1_2_today);

        // --- Aktiviteler (Güneş Kreşi) ---
        var activity2_1_yesterday = new Activity("Legolarla Tasarım", "Lego blokları kullanarak çeşitli geometrik kuleler inşa edildi.", yesterday, new TimeSpan(10, 0, 0), new TimeSpan(11, 0, 0), ActivityType.FreePlay, class3.Id);
        activity2_1_yesterday.SchoolId = school2.Id;
        activity2_1_yesterday.Complete();
        await context.Set<Activity>().AddAsync(activity2_1_yesterday);

        var activity2_1_today = new Activity("Bahçe Oyunları", "Açık havada saklambaç ve yakalamaç oynanarak fiziksel aktivite sağlandı.", today, new TimeSpan(15, 0, 0), new TimeSpan(16, 0, 0), ActivityType.Outdoor, class3.Id);
        activity2_1_today.SchoolId = school2.Id;
        await context.Set<Activity>().AddAsync(activity2_1_today);

        var activity2_2_yesterday = new Activity("Müzik ve Ritim", "Ritim aletleri (tef, marakas) eşliğinde ses ve ritim çalışmaları yapıldı.", yesterday, new TimeSpan(11, 0, 0), new TimeSpan(12, 0, 0), ActivityType.Music, class4.Id);
        activity2_2_yesterday.SchoolId = school2.Id;
        activity2_2_yesterday.Complete();
        await context.Set<Activity>().AddAsync(activity2_2_yesterday);

        var activity2_2_today = new Activity("Doğa Keşfi", "Okul bahçesindeki yapraklar ve böcekler büyüteçle incelenerek gözlem yapıldı.", today, new TimeSpan(14, 0, 0), new TimeSpan(15, 0, 0), ActivityType.Science, class4.Id);
        activity2_2_today.SchoolId = school2.Id;
        await context.Set<Activity>().AddAsync(activity2_2_today);

        // ======================================================================
        // 📝 GÜNLÜK KAYITLAR (DAILY RECORDS), ÖĞÜNLER VE AI ÖZETLERİ (SEED)
        // ======================================================================

        foreach (var item in studentsData)
        {
            // --- 1. DÜNÜN KAYDI (YESTERDAY DAILY RECORD) ---
            var recYesterday = new DailyRecord(item.Entity.Id, yesterday);
            recYesterday.SchoolId = item.SchoolId;
            
            // Can Koç (student2_1) için dün devamsızdı diyelim test amaçlı
            if (item.Entity.FirstName == "Can")
            {
                recYesterday.SetAbsentStatus(true);
                recYesterday.SetTeacherNote("Öğrencimiz bugün rahatsızlığı nedeniyle okula katılamadı.");
                await context.Set<DailyRecord>().AddAsync(recYesterday);
            }
            else
            {
                recYesterday.UpdateSleepInfo(new SleepData(SleepStatus.SleptWell, yesterday.AddHours(13), yesterday.AddHours(15)));
                recYesterday.UpdateWaterConsumption(new WaterIntake(600));
                recYesterday.SetTeacherNote($"{item.Entity.FirstName} dün arkadaşlarıyla çok uyumluydu ve tüm grup aktivitelerine neşeyle katıldı.");
                await context.Set<DailyRecord>().AddAsync(recYesterday);

                // Dünün Öğünleri
                var b1 = new MealRecord(recYesterday.Id, "Kahvaltı", new MealStatus(MealStatusType.All, "Tüm tabağını bitirdi."));
                b1.SetNutrition(220, "Haşlanmış Yumurta, Beyaz Peynir, Zeytin, Bitki Çayı", 10.0, 15.0);
                b1.SchoolId = item.SchoolId;
                await context.Set<MealRecord>().AddAsync(b1);

                var l1 = new MealRecord(recYesterday.Id, "Öğle Yemeği", new MealStatus(MealStatusType.Half, "Çorbasını içti, köftenin yarısını yedi."));
                l1.SetNutrition(480, "Mercimek Çorbası, Izgara Köfte, Tereyağlı Makarna", 22.0, 50.0);
                l1.SchoolId = item.SchoolId;
                await context.Set<MealRecord>().AddAsync(l1);

                var s1 = new MealRecord(recYesterday.Id, "İkindi Kahvaltısı", new MealStatus(MealStatusType.All, "Meyve salatasının tamamını afiyetle tüketti."));
                s1.SetNutrition(190, "Ev Yapımı Havuçlu Kek, Süt", 6.0, 28.0);
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
            }



            // Bugünün kaydı boş bırakılıyor. Öğretmenler ve veliler "Giriş Yapılmadı" durumunu görebilecek
            // ve öğretmenlerin bugün için girdiği veriler kalıcı olacaktır.
        }

        // --- Haftalık Yemek Planı Şablonları Tohumlama ---
        await SeedWeeklyPlansForClassroomAsync(context, class1.Id, school1.Id);
        await SeedWeeklyPlansForClassroomAsync(context, class2.Id, school1.Id);
        await SeedWeeklyPlansForClassroomAsync(context, class3.Id, school2.Id);
        await SeedWeeklyPlansForClassroomAsync(context, class4.Id, school2.Id);

        // --- Haftalık Ders Programı Şablonları Tohumlama ---
        await SeedWeeklyScheduleForClassroomAsync(context, class1.Id, school1.Id);
        await SeedWeeklyScheduleForClassroomAsync(context, class2.Id, school1.Id);
        await SeedWeeklyScheduleForClassroomAsync(context, class3.Id, school2.Id);
        await SeedWeeklyScheduleForClassroomAsync(context, class4.Id, school2.Id);

        // --- Bülten Tohumlama ---
        var n1 = new Newsletter(
            "Ekim 2. Hafta Bülteni",
            "Bu hafta çocuklarımızla harika deneyimler yaşadık.",
            "https://images.unsplash.com/photo-1503676260728-1c00da094a0b?q=80&w=2022&auto=format&fit=crop",
            "2-6 Ekim 2026",
            class1.Id);
        n1.SchoolId = school1.Id;
        n1.AddSection("Fen ve Doğa", "Sonbahar yapraklarını inceledik ve yaprak baskısı yaptık.", "Hava olayları (Yağmur nasıl oluşur?)", "Fatma Öğretmen");
        n1.AddSection("Matematik", "1'den 10'a kadar ritmik sayma çalışmaları ve eşleştirme oyunları.", "Basit toplama işlemlerine giriş", "Fatma Öğretmen");
        n1.AddSection("İngilizce", "Colors (Renkler) şarkısı öğrenildi ve oyunlarla pekiştirildi.", "Animals (Hayvanlar)", "Ms. Sarah");
        
        // Reflection ile Published yapalım ki veliler görsün
        var statusProp = typeof(Newsletter).GetProperty("Status");
        statusProp?.SetValue(n1, NewsletterStatus.Published);
        var pubProp = typeof(Newsletter).GetProperty("PublishedAt");
        pubProp?.SetValue(n1, DateTime.UtcNow.AddDays(-2));

        await context.Set<Newsletter>().AddAsync(n1);

        // 3. Değişiklikleri kaydet
        await context.SaveChangesAsync();
    }

    private static async Task SeedWeeklyPlansForClassroomAsync(AppDbContext context, Guid classroomId, Guid schoolId)
    {
        var days = new[]
        {
            new { Day = DayOfWeek.Monday, Breakfast = "Yumurtalı Ekmek, Peynir, Ihlamur", BfCal = 210, BfProt = 9.0, BfCarb = 18.0, Lunch = "Yayla Çorbası, Tavuk Sote, Bulgur Pilavı", LuCal = 520, LuProt = 28.0, LuCarb = 55.0, Snack = "Meyveli Yoğurt, Grissini", SnCal = 180, SnProt = 5.0, SnCarb = 25.0 },
            new { Day = DayOfWeek.Tuesday, Breakfast = "Haşlanmış Yumurta, Beyaz Peynir, Zeytin, Bitki Çayı", BfCal = 220, BfProt = 10.0, BfCarb = 15.0, Lunch = "Mercimek Çorbası, Izgara Köfte, Tereyağlı Makarna", LuCal = 480, LuProt = 22.0, LuCarb = 50.0, Snack = "Ev Yapımı Havuçlu Kek, Süt", SnCal = 190, SnProt = 6.0, SnCarb = 28.0 },
            new { Day = DayOfWeek.Wednesday, Breakfast = "Pankek, Bal, Labne Peyniri, Süt", BfCal = 250, BfProt = 8.0, BfCarb = 32.0, Lunch = "Domates Çorbası, Fırın Mücver, Yoğurt", LuCal = 420, LuProt = 15.0, LuCarb = 40.0, Snack = "Muz, Kuruyemiş", SnCal = 170, SnProt = 4.0, SnCarb = 22.0 },
            new { Day = DayOfWeek.Thursday, Breakfast = "Kaşarlı Tost, Domates, Salatalık, Ihlamur", BfCal = 240, BfProt = 11.0, BfCarb = 28.0, Lunch = "Tarhana Çorbası, Kıymalı Bezelye, Pirinç Pilavı", LuCal = 500, LuProt = 24.0, LuCarb = 58.0, Snack = "Elmalı Tart, Meyve Suyu", SnCal = 200, SnProt = 3.0, SnCarb = 35.0 },
            new { Day = DayOfWeek.Friday, Breakfast = "Simit, Üçgen Peynir, Zeytin, Açık Çay", BfCal = 230, BfProt = 7.0, BfCarb = 34.0, Lunch = "Sebze Çorbası, Fırın Somon, Patates Püresi", LuCal = 490, LuProt = 26.0, LuCarb = 45.0, Snack = "Yulaflı Kurabiye, Süt", SnCal = 180, SnProt = 6.0, SnCarb = 24.0 }
        };

        foreach (var d in days)
        {
            await context.Set<WeeklyMealPlan>().AddAsync(new WeeklyMealPlan(classroomId, d.Day, "Kahvaltı", d.BfCal, d.Breakfast, d.BfProt, d.BfCarb, schoolId));
            await context.Set<WeeklyMealPlan>().AddAsync(new WeeklyMealPlan(classroomId, d.Day, "Öğle Yemeği", d.LuCal, d.Lunch, d.LuProt, d.LuCarb, schoolId));
            await context.Set<WeeklyMealPlan>().AddAsync(new WeeklyMealPlan(classroomId, d.Day, "İkindi Kahvaltısı", d.SnCal, d.Snack, d.SnProt, d.SnCarb, schoolId));
        }
    }

    private static async Task SeedWeeklyScheduleForClassroomAsync(AppDbContext context, Guid classroomId, Guid schoolId)
    {
        for (int day = 1; day <= 5; day++)
        {
            // Sabah Sporu / Serbest Oyun (Oyun: 5)
            await context.Set<ClassroomWeeklySchedule>().AddAsync(new ClassroomWeeklySchedule(
                classroomId, schoolId, day, "Serbest Oyun ve Sabah Sporu", "Güne enerjik bir başlangıç için bahçede veya sınıfta oyun.", 
                new TimeSpan(9, 0, 0), new TimeSpan(9, 45, 0), ActivityType.FreePlay));
            
            // Eğitim / Ders (Ders: 0)
            string dersAdi = day == 1 ? "Türkçe Dil Etkinliği" : day == 2 ? "Matematik" : day == 3 ? "Fen ve Doğa" : day == 4 ? "İngilizce" : "Okuma Yazmaya Hazırlık";
            await context.Set<ClassroomWeeklySchedule>().AddAsync(new ClassroomWeeklySchedule(
                classroomId, schoolId, day, dersAdi, "Temel kavramları öğrenme ve pekiştirme çalışmaları.", 
                new TimeSpan(10, 0, 0), new TimeSpan(10, 45, 0), ActivityType.General));

            // Sanat (Sanat: 6)
            await context.Set<ClassroomWeeklySchedule>().AddAsync(new ClassroomWeeklySchedule(
                classroomId, schoolId, day, "Sanat Atölyesi", "Boyama, kesme ve yapıştırma ile ince motor gelişimi.", 
                new TimeSpan(11, 0, 0), new TimeSpan(11, 45, 0), ActivityType.Art));

            // Öğle Yemeği (Yemek: 2)
            await context.Set<ClassroomWeeklySchedule>().AddAsync(new ClassroomWeeklySchedule(
                classroomId, schoolId, day, "Öğle Yemeği", "Sıcak ve besleyici öğle yemeği saati.", 
                new TimeSpan(12, 0, 0), new TimeSpan(13, 0, 0), ActivityType.Lunch));

            // Uyku (Uyku: 4)
            await context.Set<ClassroomWeeklySchedule>().AddAsync(new ClassroomWeeklySchedule(
                classroomId, schoolId, day, "Öğle Uykusu", "Çocukların günün yorgunluğunu attığı dinlenme saati.", 
                new TimeSpan(13, 30, 0), new TimeSpan(15, 0, 0), ActivityType.Sleep));
        }
    }
}
