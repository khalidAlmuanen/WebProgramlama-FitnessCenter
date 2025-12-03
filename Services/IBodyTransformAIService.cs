using System.Threading.Tasks;

namespace FitnessCenterApp.Services
{
    public interface IBodyTransformAIService
    {
        Task<(string generatedImagePath, double expectedChangePercent)> GenerateTransformedImageAsync(
            string originalImagePath,
            string goalType,
            int durationMonths,
            double startWeightKg);
    }
}
