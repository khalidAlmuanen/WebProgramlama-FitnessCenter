using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FitnessCenterApp.Services
{
    public class OpenAIBodyTransformAIService : IBodyTransformAIService
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<OpenAIBodyTransformAIService> _logger;

        public OpenAIBodyTransformAIService(
            IConfiguration configuration,
            IWebHostEnvironment env,
            ILogger<OpenAIBodyTransformAIService> logger)
        {
            _configuration = configuration;
            _env = env;
            _logger = logger;
        }

        public async Task<(string generatedImagePath, double expectedChangePercent)> GenerateTransformedImageAsync(
            string originalImagePath,
            string goalType,
            int durationMonths,
            double startWeightKg)
        {
            var expectedChangePercent = CalculateExpectedChangePercent(goalType, durationMonths);

            var prompt = BuildPrompt(goalType, durationMonths, expectedChangePercent, startWeightKg);

            _logger.LogInformation("BodyTransform prompt: {Prompt}", prompt);

            var imageBytes = await CallOpenAIImageEditAsync(originalImagePath, prompt);

            var generatedRelativePath = await SaveGeneratedImageAsync(imageBytes);

            return (generatedRelativePath, expectedChangePercent);
        }
       private double CalculateExpectedChangePercent(string goalType, int months)
{
    if (string.IsNullOrWhiteSpace(goalType))
        return 0;

    goalType = goalType.ToLower();

    if (goalType.Contains("kilo") || goalType.Contains("verme"))
    {
        return -1.5 * months;
    }

    if (goalType.Contains("kas") || goalType.Contains("artır"))
    {
        return 0.8 * months;
    }

    return 0;
}

        private string BuildPrompt(string goalType, int months, double percent, double weight)
{
    var abs = Math.Abs(percent);
    string direction;

    if (percent < 0)
    {
        direction =
            "slightly slimmer and healthier body: a bit less fat around the face, neck and belly, " +
            "improved posture and overall healthier look. " +
            "Approximately " + abs.ToString("0") + "% less body fat overall.";
    }
    else if (percent > 0)
    {
        direction =
            "slightly more athletic and fitter body: improved posture, a little more definition on the arms " +
            "and shoulders under the clothes, but still realistic and modest. " +
            "Approximately " + abs.ToString("0") + "% improvement in muscle tone.";
    }
    else
    {
        direction =
            "maintain the current body shape with only very subtle improvements in fitness.";
    }

    var sb = new StringBuilder();
    sb.AppendLine("IMPORTANT: This is an edit of the original photo.");
    sb.AppendLine("You MUST keep the EXACT SAME PERSON, same identity, same facial features, same skin tone, same hair style,");
    sb.AppendLine("same shirt pattern and colors, same background and camera angle.");
    sb.AppendLine("Do NOT change the face structure, eyes, nose, mouth, beard, or hairstyle.");
    sb.AppendLine("Do NOT change the shirt, its colors, or the environment.");
    sb.AppendLine();
    sb.AppendLine("The result must be fully clothed, safe-for-work, non-sexual, with no nudity and no emphasis on any private body parts.");
    sb.AppendLine();
    sb.AppendLine("Goal: " + goalType + " for " + months + " months.");
    if (weight > 0)
    {
        sb.AppendLine("Approx starting weight: " + weight.ToString("0") + " kg.");
    }
    sb.AppendLine("Apply the following body transformation only:");
    sb.AppendLine(direction);
    sb.AppendLine("Make the transformation clearly visible but still realistic, avoiding any cartoonish, exaggerated or sexualized look.");

    return sb.ToString();
}
        private async Task<byte[]> CallOpenAIImageEditAsync(string originalImagePath, string prompt)
        {
            var apiKey = _configuration["OpenAI:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new InvalidOperationException("OpenAI:ApiKey is not configured.");

            var model = "gpt-image-1";

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            var physicalPath = Path.Combine(_env.WebRootPath, originalImagePath.TrimStart('/'));
            if (!File.Exists(physicalPath))
                throw new FileNotFoundException("Original image not found.", physicalPath);

            var imageBytes = await File.ReadAllBytesAsync(physicalPath);
            var imgContent = new ByteArrayContent(imageBytes);
            imgContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");

            using var form = new MultipartFormDataContent();
            form.Add(new StringContent(model), "model");
            form.Add(new StringContent(prompt), "prompt");
            form.Add(imgContent, "image", "input.png");

            var response = await client.PostAsync("https://api.openai.com/v1/images/edits", form);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"OpenAI ERROR: {response.StatusCode}\n{json}");

            var parsed = JsonSerializer.Deserialize<OpenAIImageResponse>(json);
            if (parsed?.data == null || parsed.data.Length == 0 || string.IsNullOrWhiteSpace(parsed.data[0].b64_json))
                throw new Exception("OpenAI returned empty image data. JSON:\n" + json);

            return Convert.FromBase64String(parsed.data[0].b64_json);
        }
        private async Task<string> SaveGeneratedImageAsync(byte[] imageBytes)
        {
            var folder = Path.Combine(_env.WebRootPath, "uploads", "generated");
            Directory.CreateDirectory(folder);

            var fileName = Guid.NewGuid().ToString() + ".png";
            var fullPath = Path.Combine(folder, fileName);

            await File.WriteAllBytesAsync(fullPath, imageBytes);

            var relative = "/uploads/generated/" + fileName;
            _logger.LogInformation("Generated image saved at {Path}", relative);
            return relative;
        }

        private class OpenAIImageResponse
        {
            public OpenAIImageData[] data { get; set; } = Array.Empty<OpenAIImageData>();
        }

        private class OpenAIImageData
        {
            public string b64_json { get; set; } = string.Empty;
        }
    }
}
