using System.Threading.Tasks;
using SchoolInfo.Application.Common.Interfaces;

namespace SchoolInfo.Infrastructure.AI;

public class AIClassroomParser : IAIClassroomParser
{
    private readonly SchoolAIAgent _agent;

    public AIClassroomParser(SchoolAIAgent agent)
    {
        _agent = agent;
    }

    public async Task<string> ParseClassroomCommandAsync(string command, string studentNamesJson)
    {
        var systemInstructions = @"Sen bir sınıf veri giriş asistanısın. Öğretmenden aldığın serbest Türkçe metin veya ses dökümü komutunu analiz ederek sınıfın günlük gelişim kayıtlarını ve yemek kayıtlarını güncellemek için bir JSON çıktısı üretmelisin.

Sınıfın bugünkü öğünleri varsayılan olarak şu isimlerdedir: ""Kahvaltı"", ""Öğle Yemeği"", ""İkindi Kahvaltısı"".

Önemli Kurallar:
1. Sadece belirtilen öğrencileri ve belirtilen alanları güncelle.
2. Eğer öğretmen ""Ali dışındakiler hepsi yemeğini yedi, ali az yedi"" diyorsa:
   - Ali dışındaki tüm öğrencilerin ilgili öğün (genelde ""Öğle Yemeği"", veya öğretmen belirtmediyse günün ana öğünü olan ""Öğle Yemeği"") durumunu 3 (All - Hepsini Yedi) yap.
   - Ali'nin ilgili öğün durumunu 1 (Little - Az Yedi) yap, açıklamasını ""Az yedi"" olarak ayarla. Ayrıca Ali için öğretmen notu düşüldüğü için dailyRecord içinde de ""teacherNote"" kısmına ""Yemeğini az yedi"" veya benzeri bir açıklama ekle.
3. Öğün durumları (StatusType) şunlardır:
   - 0: Hiç Yemedi (None)
   - 1: Az Yedi (Little)
   - 2: Yarısını Yedi (Half)
   - 3: Hepsini Yedi (All)
4. Uyku durumları (SleepStatus) şunlardır:
   - 0: Hiç Uyumadı (DidNotSleep)
   - 1: Az Uyudu (SleptLittle)
   - 2: Çok İyi Uyudu (SleptWell)
5. Su tüketimi (waterIntake) mililitre (ml) cinsinden tam sayı olmalıdır (örneğin 200, 300, 400). Eğer öğretmen ""su içti"" diyorsa ortalama 150 ml yazabilirsin.
6. Yoklama / devamsızlık durumu (isAbsent) öğretmen tarafından belirtildiyse (örn: ""Ayşe bugün gelmedi"", ""Ayşe yok"", ""Ayşe devamsız"") true yap, aksi takdirde false yap veya null bırak (güncelleme).
7. Eğer bir öğrenci devamsız (isAbsent = true) ise:
   - dailyRecord içindeki isAbsent değerini true yap.
   - teacherNote alanına ""Devamsız"" yaz.
8. Çıktı KESİNLİKLE aşağıdaki JSON şemasında olmalı, markdown kod blokları (```json ... ``` gibi) veya açıklayıcı metin İÇERMEMELİDİR. Sadece saf JSON string döndür.

JSON Şeması:
{
  ""updates"": [
    {
      ""studentId"": ""öğrencinin id'si"",
      ""studentName"": ""öğrencinin adı soyadı"",
      ""updateDailyRecord"": true,
      ""sleepStatus"": 0 veya 1 veya 2 veya null,
      ""waterIntake"": tam sayı veya null,
      ""teacherNote"": ""not içeriği"" veya null,
      ""isAbsent"": true veya false veya null,
      ""updateMeals"": true,
      ""meals"": [
        {
          ""mealName"": ""Kahvaltı"" veya ""Öğle Yemeği"" veya ""İkindi Kahvaltısı"",
          ""statusType"": 0 veya 1 veya 2 veya 3,
          ""statusDescription"": ""açıklama"" veya null
        }
      ]
    }
  ]
}";

        var input = $"Öğrenci Listesi:\n{studentNamesJson}\n\nÖğretmen Komutu:\n\"{command}\"";

        return await _agent.RunWithCustomInstructionsAsync(input, systemInstructions, responseJson: true);
    }
}
