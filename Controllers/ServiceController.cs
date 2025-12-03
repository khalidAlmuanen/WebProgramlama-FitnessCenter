using FitnessCenterApp.Data;
using FitnessCenterApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenterApp.Controllers
{
    [Authorize] 
    public class ServiceController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ServiceController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var services = await _context.Services
                .Include(s => s.Gym)
                .ToListAsync();

            return View(services);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var service = await _context.Services
                .Include(s => s.Gym)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (service == null) return NotFound();

            return View(service);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            await LoadGymsDropDownList();
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Service service)
        {
            bool exists = await _context.Services
                .AnyAsync(s => s.Name == service.Name && s.GymId == service.GymId);

            if (exists)
            {
                ModelState.AddModelError(string.Empty,
                    "Bu spor salonunda aynı isimde bir hizmet zaten kayıtlı.");
            }

            if (!ModelState.IsValid)
            {
                await LoadGymsDropDownList(service.GymId);
                return View(service);
            }

            _context.Add(service);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var service = await _context.Services.FindAsync(id);
            if (service == null) return NotFound();

            await LoadGymsDropDownList(service.GymId);
            return View(service);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Service service)
        {
            if (id != service.Id) return NotFound();

            bool exists = await _context.Services
                .AnyAsync(s => s.Id != service.Id &&
                               s.Name == service.Name &&
                               s.GymId == service.GymId);

            if (exists)
            {
                ModelState.AddModelError(string.Empty,
                    "Bu spor salonunda aynı isimde bir hizmet zaten kayıtlı.");
            }

            if (!ModelState.IsValid)
            {
                await LoadGymsDropDownList(service.GymId);
                return View(service);
            }

            try
            {
                _context.Update(service);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Services.Any(e => e.Id == service.Id))
                    return NotFound();
                else
                    throw;
            }

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var service = await _context.Services
                .Include(s => s.Gym)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (service == null) return NotFound();

            return View(service);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service != null)
            {
                _context.Services.Remove(service);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task LoadGymsDropDownList(object selectedGym = null)
        {
            var gyms = await _context.Gyms
                .OrderBy(g => g.Name)
                .ToListAsync();

            ViewBag.GymId = new SelectList(gyms, "Id", "Name", selectedGym);
        }
    }
}
