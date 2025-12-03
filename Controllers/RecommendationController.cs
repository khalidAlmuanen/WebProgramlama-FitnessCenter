using System.Linq;
using System.Threading.Tasks;
using FitnessCenterApp.Data;
using FitnessCenterApp.Models;
using FitnessCenterApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenterApp.Controllers
{
    [Authorize] 
    public class RecommendationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAIRecommendationService _aiService;

        public RecommendationController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IAIRecommendationService aiService)
        {
            _context = context;
            _userManager = userManager;
            _aiService = aiService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User)!;

            var profile = await _context.MemberGoals
                .FirstOrDefaultAsync(m => m.UserId == userId);

            var services = await _context.Services
                .Include(s => s.Gym)
                .OrderBy(s => s.Name)
                .ToListAsync();

            var plan = await _aiService.GetExercisePlanAsync(profile, services);

            ViewBag.Plan = plan;
            return View();
        }
    }
}
