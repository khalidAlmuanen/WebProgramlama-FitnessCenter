using System.Linq;
using System.Threading.Tasks;
using FitnessCenterApp.Data;
using FitnessCenterApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenterApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class TrainerAvailabilityController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TrainerAvailabilityController(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(int? trainerId)
        {
            var query = _context.TrainerAvailabilities
                .Include(a => a.Trainer)
                .AsQueryable();

            if (trainerId.HasValue)
            {
                query = query.Where(a => a.TrainerId == trainerId.Value);

                ViewBag.SelectedTrainer = await _context.Trainers
                    .Where(t => t.Id == trainerId.Value)
                    .Select(t => t.FullName)
                    .FirstOrDefaultAsync();
            }

            var list = await query
                .OrderBy(a => a.Trainer.FullName)
                .ThenBy(a => a.Day)
                .ThenBy(a => a.StartTime)
                .ToListAsync();

            await LoadTrainersDropDown(trainerId);

            return View(list);
        }

        public async Task<IActionResult> Create(int? trainerId)
        {
            await LoadTrainersDropDown(trainerId);
            var model = new TrainerAvailability();

            if (trainerId.HasValue)
                model.TrainerId = trainerId.Value;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TrainerAvailability availability)
        {
            if (availability.EndTime <= availability.StartTime)
            {
                ModelState.AddModelError(string.Empty,
                    "Bitiş saati, başlangıç saatinden sonra olmalıdır.");
            }

            bool overlap = await _context.TrainerAvailabilities.AnyAsync(a =>
                a.TrainerId == availability.TrainerId &&
                a.Day == availability.Day &&
                ((availability.StartTime >= a.StartTime && availability.StartTime < a.EndTime) ||
                 (availability.EndTime   >  a.StartTime && availability.EndTime   <= a.EndTime) ||
                 (availability.StartTime <= a.StartTime && availability.EndTime >= a.EndTime)));

            if (overlap)
            {
                ModelState.AddModelError(string.Empty,
                    "Bu antrenör için seçilen gün ve saat aralığında zaten bir müsaitlik kaydı var.");
            }

            if (!ModelState.IsValid)
            {
                await LoadTrainersDropDown(availability.TrainerId);
                return View(availability);
            }

            _context.Add(availability);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { trainerId = availability.TrainerId });
        }

public async Task<IActionResult> Details(int? id)
{
    if (id == null) return NotFound();

    var availability = await _context.TrainerAvailabilities
        .Include(a => a.Trainer)
        .ThenInclude(t => t.Gym)
        .FirstOrDefaultAsync(a => a.Id == id);

    if (availability == null) return NotFound();

    return View(availability);
}
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var availability = await _context.TrainerAvailabilities.FindAsync(id);
            if (availability == null) return NotFound();

            await LoadTrainersDropDown(availability.TrainerId);
            return View(availability);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TrainerAvailability availability)
        {
            if (id != availability.Id) return NotFound();

            if (availability.EndTime <= availability.StartTime)
            {
                ModelState.AddModelError(string.Empty,
                    "Bitiş saati, başlangıç saatinden sonra olmalıdır.");
            }

            bool overlap = await _context.TrainerAvailabilities.AnyAsync(a =>
                a.Id != availability.Id &&
                a.TrainerId == availability.TrainerId &&
                a.Day == availability.Day &&
                ((availability.StartTime >= a.StartTime && availability.StartTime < a.EndTime) ||
                 (availability.EndTime   >  a.StartTime && availability.EndTime   <= a.EndTime) ||
                 (availability.StartTime <= a.StartTime && availability.EndTime >= a.EndTime)));

            if (overlap)
            {
                ModelState.AddModelError(string.Empty,
                    "Bu antrenör için seçilen gün ve saat aralığında zaten bir müsaitlik kaydı var.");
            }

            if (!ModelState.IsValid)
            {
                await LoadTrainersDropDown(availability.TrainerId);
                return View(availability);
            }

            try
            {
                _context.Update(availability);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TrainerAvailabilityExists(availability.Id))
                    return NotFound();
                else
                    throw;
            }

            return RedirectToAction(nameof(Index), new { trainerId = availability.TrainerId });
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var availability = await _context.TrainerAvailabilities
                .Include(a => a.Trainer)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (availability == null) return NotFound();

            return View(availability);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var availability = await _context.TrainerAvailabilities.FindAsync(id);
            if (availability != null)
            {
                int trainerId = availability.TrainerId;

                _context.TrainerAvailabilities.Remove(availability);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index), new { trainerId });
            }

            return RedirectToAction(nameof(Index));
        }

        private bool TrainerAvailabilityExists(int id)
        {
            return _context.TrainerAvailabilities.Any(e => e.Id == id);
        }

        private async Task LoadTrainersDropDown(int? selectedTrainer = null)
        {
            var trainers = await _context.Trainers
                .OrderBy(t => t.FullName)
                .ToListAsync();

            ViewBag.TrainerId = new SelectList(trainers, "Id", "FullName", selectedTrainer);
        }
    }
}
