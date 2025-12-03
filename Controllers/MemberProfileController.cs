using System.Threading.Tasks;
using FitnessCenterApp.Data;
using FitnessCenterApp.Models;
using FitnessCenterApp.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenterApp.Controllers
{
    [Authorize] 
    public class MemberProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public MemberProfileController(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        private Task<ApplicationUser?> GetCurrentUserAsync()
            => _userManager.GetUserAsync(User);

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
                return Challenge(); 

            var goal = await _context.MemberGoals
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.UserId == user.Id);

            var vm = new MemberProfileViewModel
            {
                HeightCm       = goal?.HeightCm,
                WeightKg       = goal?.WeightKg,
                GoalType       = goal?.GoalType,
                WorkoutsPerWeek = goal?.WorkoutsPerWeek,
                Notes          = goal?.Notes
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(MemberProfileViewModel model)
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
                return Challenge();

            if (!ModelState.IsValid)
                return View(model);

            var goal = await _context.MemberGoals
                .FirstOrDefaultAsync(m => m.UserId == user.Id);

            if (goal == null)
            {
                goal = new MemberGoal
                {
                    UserId = user.Id
                };
                _context.MemberGoals.Add(goal);
            }

            goal.HeightCm        = model.HeightCm;
            goal.WeightKg        = model.WeightKg;
            goal.GoalType        = model.GoalType;
            goal.WorkoutsPerWeek = model.WorkoutsPerWeek;
            goal.Notes           = model.Notes;

            await _context.SaveChangesAsync();

            TempData["ProfileSaved"] = "Profil bilgileriniz başarıyla güncellendi.";
            return RedirectToAction(nameof(Index));
        }
    }
}
