using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using SchoolInfo.Application.Common.Interfaces;

namespace SchoolInfo.Infrastructure.AI;

public class AIClassroomParser : IAIClassroomParser
{
    private readonly SchoolAIAgent _agent;
    private readonly IConfiguration _configuration;

    public AIClassroomParser(SchoolAIAgent agent, IConfiguration configuration)
    {
        _agent = agent;
        _configuration = configuration;
    }

    public async Task<string> ParseClassroomCommandAsync(string command, string studentNamesJson)
    {
        var apiKey = _configuration["AgentFramework:ApiKey"];
        if (string.IsNullOrEmpty(apiKey) || apiKey == "YOUR_API_KEY" || apiKey == "YOUR_GEMINI_API_KEY" || apiKey == "key")
        {
            // API Key is a placeholder. Return a local mock response to allow testing without real Gemini API key!
            return GenerateMockResponse(command, studentNamesJson);
        }

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

    private string GenerateMockResponse(string command, string studentNamesJson)
    {
        try
        {
            using var doc = JsonDocument.Parse(studentNamesJson);
            var students = doc.RootElement.EnumerateArray().Select(s => new {
                id = s.GetProperty("id").GetGuid(),
                name = s.GetProperty("name").GetString() ?? ""
            }).ToList();

            var studentUpdates = new Dictionary<Guid, MockStudentUpdate>();

            // Helper to check if name is in the sentence
            bool ContainsName(string sentence, string name)
            {
                var parts = name.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 0) return false;
                
                if (sentence.Contains(name, StringComparison.OrdinalIgnoreCase)) return true;
                
                var firstName = parts[0];
                var words = sentence.Split(new[] { ' ', '.', ',', '?', '!', ';', ':', '-' }, StringSplitOptions.RemoveEmptyEntries);
                return words.Any(w => w.Equals(firstName, StringComparison.OrdinalIgnoreCase));
            }

            var sentences = command.Split(new[] { '.', ';', ',' }, StringSplitOptions.RemoveEmptyEntries)
                                   .Select(s => s.Trim())
                                   .Where(s => !string.IsNullOrEmpty(s))
                                   .ToList();

            var explicitlyMentionedIds = new HashSet<Guid>();
            foreach (var sentence in sentences)
            {
                foreach (var student in students)
                {
                    if (ContainsName(sentence, student.name))
                    {
                        explicitlyMentionedIds.Add(student.id);
                    }
                }
            }

            foreach (var sentence in sentences)
            {
                var lowerSentence = sentence.ToLower();

                // Target students for this sentence
                var targetStudentIds = students
                    .Where(s => ContainsName(sentence, s.name))
                    .Select(s => s.id)
                    .ToList();

                if (!targetStudentIds.Any())
                {
                    // If no specific student is mentioned, we target either "others" (if any are explicitly mentioned elsewhere)
                    // or "all" (if none are explicitly mentioned in the entire command).
                    if (explicitlyMentionedIds.Any())
                    {
                        targetStudentIds = students
                            .Where(s => !explicitlyMentionedIds.Contains(s.id))
                            .Select(s => s.id)
                            .ToList();
                    }
                    else
                    {
                        targetStudentIds = students.Select(s => s.id).ToList();
                    }
                }

                // Parse action details
                bool isAbsentAction = lowerSentence.Contains("gelmedi") || 
                                      lowerSentence.Contains("yok") || 
                                      lowerSentence.Contains("devamsız");

                bool sleepAction = lowerSentence.Contains("uyku") || 
                                   lowerSentence.Contains("uyudu") || 
                                   lowerSentence.Contains("uyumadı");
                
                int? parsedSleepStatus = null;
                if (sleepAction)
                {
                    if (lowerSentence.Contains("hiç") || lowerSentence.Contains("uyumadı"))
                        parsedSleepStatus = 0; // DidNotSleep
                    else if (lowerSentence.Contains("az"))
                        parsedSleepStatus = 1; // SleptLittle
                    else
                        parsedSleepStatus = 2; // SleptWell
                }

                bool waterAction = lowerSentence.Contains("su") || 
                                   lowerSentence.Contains("ml") || 
                                   lowerSentence.Contains("içti") ||
                                   lowerSentence.Contains("tüketti");
                
                int? parsedWater = null;
                if (waterAction)
                {
                    var match = System.Text.RegularExpressions.Regex.Match(sentence, @"\d+");
                    if (match.Success && int.TryParse(match.Value, out var val))
                    {
                        parsedWater = val;
                    }
                    else
                    {
                        parsedWater = 150; // Default water intake in ml
                    }
                }

                bool foodAction = lowerSentence.Contains("yemek") || 
                                  lowerSentence.Contains("yedi") || 
                                  lowerSentence.Contains("yediler") || 
                                  lowerSentence.Contains("yemeğini") || 
                                  lowerSentence.Contains("kahvaltı") || 
                                  lowerSentence.Contains("öğle") || 
                                  lowerSentence.Contains("ikindi");

                string? parsedMealName = null;
                int? parsedStatusType = null;
                string? statusDesc = null;

                if (foodAction)
                {
                    if (lowerSentence.Contains("kahvaltı") && !lowerSentence.Contains("ikindi"))
                        parsedMealName = "Kahvaltı";
                    else if (lowerSentence.Contains("ikindi"))
                        parsedMealName = "İkindi Kahvaltısı";
                    else
                        parsedMealName = "Öğle Yemeği";

                    if (lowerSentence.Contains("hiç") || lowerSentence.Contains("yemedi"))
                    {
                        parsedStatusType = 0; // None
                        statusDesc = "Hiç yemedi";
                    }
                    else if (lowerSentence.Contains("az"))
                    {
                        parsedStatusType = 1; // Little
                        statusDesc = "Az yedi";
                    }
                    else if (lowerSentence.Contains("yarısını") || lowerSentence.Contains("yarım"))
                    {
                        parsedStatusType = 2; // Half
                        statusDesc = "Yarısını yedi";
                    }
                    else
                    {
                        parsedStatusType = 3; // All
                        statusDesc = "Hepsini yedi";
                    }
                }

                // Apply parsed details to targeted students
                foreach (var studentId in targetStudentIds)
                {
                    var student = students.First(s => s.id == studentId);
                    if (!studentUpdates.TryGetValue(studentId, out var updateItem))
                    {
                        updateItem = new MockStudentUpdate
                        {
                            StudentId = studentId,
                            StudentName = student.name,
                            Meals = new List<MockMealUpdate>()
                        };
                        studentUpdates[studentId] = updateItem;
                    }

                    if (isAbsentAction)
                    {
                        updateItem.UpdateDailyRecord = true;
                        updateItem.IsAbsent = true;
                        updateItem.TeacherNote = "Devamsız";
                        
                        // Clear any updates that wouldn't make sense if they are absent
                        updateItem.SleepStatus = null;
                        updateItem.WaterIntake = null;
                        updateItem.UpdateMeals = false;
                        updateItem.Meals.Clear();
                    }
                    else
                    {
                        // Only apply daily record/meals if they are not marked absent
                        if (updateItem.IsAbsent != true)
                        {
                            if (sleepAction)
                            {
                                updateItem.UpdateDailyRecord = true;
                                updateItem.SleepStatus = parsedSleepStatus;
                            }

                            if (waterAction)
                            {
                                updateItem.UpdateDailyRecord = true;
                                updateItem.WaterIntake = parsedWater;
                            }

                            if (foodAction && parsedMealName != null && parsedStatusType.HasValue)
                            {
                                updateItem.UpdateMeals = true;
                                
                                // Remove existing update for this meal if any
                                updateItem.Meals.RemoveAll(m => m.MealName.Equals(parsedMealName, StringComparison.OrdinalIgnoreCase));
                                
                                updateItem.Meals.Add(new MockMealUpdate
                                {
                                    MealName = parsedMealName,
                                    StatusType = parsedStatusType.Value,
                                    StatusDescription = statusDesc
                                });

                                // Check if we should write a teacher note for eating little
                                if (parsedStatusType.Value == 1)
                                {
                                    updateItem.UpdateDailyRecord = true;
                                    updateItem.TeacherNote = "Yemeğini az yedi.";
                                }
                            }
                        }
                    }
                }
            }

            var resultUpdates = studentUpdates.Values
                .Where(u => u.UpdateDailyRecord || u.UpdateMeals)
                .ToList();

            var result = new { updates = resultUpdates };
            return JsonSerializer.Serialize(result, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }
        catch
        {
            return "{\"updates\":[]}";
        }
    }

    private class MockStudentUpdate
    {
        public Guid StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public bool UpdateDailyRecord { get; set; }
        public int? SleepStatus { get; set; }
        public int? WaterIntake { get; set; }
        public string? TeacherNote { get; set; }
        public bool? IsAbsent { get; set; }
        public bool UpdateMeals { get; set; }
        public List<MockMealUpdate> Meals { get; set; } = new();
    }

    private class MockMealUpdate
    {
        public string MealName { get; set; } = string.Empty;
        public int StatusType { get; set; }
        public string? StatusDescription { get; set; }
    }
}
