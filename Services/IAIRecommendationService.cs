using System.Collections.Generic;
using System.Threading.Tasks;
using FitnessCenterApp.Models;

namespace FitnessCenterApp.Services
{
    public interface IAIRecommendationService
    {
        Task<string> GetExercisePlanAsync(MemberGoal profile, List<Service> services);
    }
}
