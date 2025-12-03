using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FitnessCenterApp.Data;
using FitnessCenterApp.Models;
using FitnessCenterApp.Models.ViewModels;
using FitnessCenterApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FitnessCenterApp.Controllers
{
    [Authorize]
    public class BodyTransformController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IBodyTransformAIService _aiService;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<BodyTransformController> _logger;

        public BodyTransformController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IBodyTransformAIService aiService,
            IWebHostEnvironment env,
            ILogger<BodyTransformController> logger)
        {
            _context = context;
            _userManager = userManager;
            _aiService = aiService;
            _env = env;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Create()
        {
            var vm = new BodyTransformCreateViewModel
            {
                DurationMonths = 12
            };
            return View(vm);  
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BodyTransformCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (model.ImageFile == null || model.ImageFile.Length == 0)
            {
                ModelState.AddModelError("ImageFile", "Lütfen bir fotoğraf yükleyiniz.");
                return View(model);
            }

            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "original");
            Directory.CreateDirectory(uploadsFolder);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ImageFile.FileName);
            var physicalPath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(physicalPath, FileMode.Create))
            {
                await model.ImageFile.CopyToAsync(stream);
            }

            var relativeOriginalPath = "/uploads/original/" + fileName;
            _logger.LogInformation("Original image saved successfully at {Path}", physicalPath);

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var request = new BodyTransformRequest
            {
                MemberId = user.Id,
                GoalType = model.GoalType,
                DurationMonths = model.DurationMonths,
                StartWeightKg = model.StartWeightKg,
                OriginalImagePath = relativeOriginalPath,
                CreatedAt = DateTime.UtcNow
            };

            _context.BodyTransformRequests.Add(request);
            await _context.SaveChangesAsync();

            try
            {
                _logger.LogInformation("Calling AI service for BodyTransform request {Id}", request.Id);

                var (generatedPath, expectedPercent) =
                    await _aiService.GenerateTransformedImageAsync(
                        request.OriginalImagePath,
                        request.GoalType,
                        request.DurationMonths,
                        request.StartWeightKg ?? 0);

                request.GeneratedImagePath = generatedPath;
                request.ExpectedChangePercent = expectedPercent;

                _context.BodyTransformRequests.Update(request);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "AI FAILED for RequestId={Id}. OriginalImage={Img}, Goal={Goal}, Months={Months}",
                    request.Id, request.OriginalImagePath, request.GoalType, request.DurationMonths);

                TempData["BodyTransformError"] =
                    "Yapay zekâ servisi şu anda yanıt veremiyor. Orijinal görüntü gösteriliyor.";
            }

            return View("Result", request); 
        }

        public async Task<IActionResult> MyRequests()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var list = await _context.BodyTransformRequests
                .Where(b => b.MemberId == user.Id)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            return View(list); 
        }
    }
}
