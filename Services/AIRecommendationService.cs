using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FitnessCenterApp.Models;
using Microsoft.Extensions.Configuration;

namespace FitnessCenterApp.Services
{
    public class AIRecommendationService : IAIRecommendationService
    {
        private readonly IConfiguration _config;
        private readonly HttpClient _http;

        public AIRecommendationService(IConfiguration config, IHttpClientFactory httpFactory)
        {
            _config = config;
            _http = httpFactory.CreateClient();
        }

        public async Task<string> GetExercisePlanAsync(MemberGoal profile, List<Service> services)
        {
            var apiKey = _config["OpenAI:ApiKey"];
            var model = _config["OpenAI:Model"] ?? "gpt-4.1-mini";

            if (string.IsNullOrWhiteSpace(apiKey))
                throw new InvalidOperationException("OpenAI API key is not configured.");

            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", apiKey);

            var grouped = services
                .GroupBy(s => s.Category ?? "Genel")
                .Select(g =>
                    $"{g.Key}: " +
                    string.Join(", ", g.Select(x => $"{x.Name} ({x.DurationMinutes} dk)")
                ));

            string servicesText = string.Join(" | ", grouped);

            string profileText =
                $"Boy: {profile?.HeightCm ?? 0} cm, " +
                $"Kilo: {profile?.WeightKg ?? 0} kg, " +
                $"Hedef: {profile?.GoalType ?? "Belirsiz"}, " +
                $"Haftalık antrenman: {profile?.WorkoutsPerWeek ?? 0}";

            string userPrompt = $@"
Kullanıcının bilgileri:
{profileText}

Sistemdeki mevcut hizmetler:
{servicesText}

Görev:
- Kullanıcıya 1 haftalık bir egzersiz planı hazırla.
- Planı Türkçe yaz.
- Günlere göre (Pazartesi, Salı, ...) ayır.
- Her gün için 2-3 egzersiz öner, süre veya set sayısı ile birlikte yaz.
- Mümkün olduğunda yukarıdaki hizmet isimlerini kullan (Cardio, Yoga, Pilates, Personal Training vb).
- Kullanıcı yeni başlayan ise çok ağır program verme.
";

            var requestBody = new
            {
                model = model,
                messages = new[]
                {
                    new { role = "system", content = "Sen deneyimli bir fitness ve beslenme koçusun. Türkçe yanıt ver." },
                    new { role = "user",   content = userPrompt }
                },
                temperature = 0.7
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _http.PostAsync("https://api.openai.com/v1/chat/completions", content);
            var responseJson = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return "AI servisi şu anda yanıt veremiyor. Lütfen daha sonra tekrar deneyin.\n\n" +
                       $"(Detay: {response.StatusCode})";
            }

            using var doc = JsonDocument.Parse(responseJson);

            var reply = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            return reply ?? "Plan oluşturulamadı.";
        }
    }
}
