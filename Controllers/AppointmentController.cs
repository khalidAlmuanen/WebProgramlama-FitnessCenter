using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FitnessCenterApp.Data;
using FitnessCenterApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenterApp.Controllers
{
    [Authorize]
    public class AppointmentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AppointmentController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> MyAppointments()
        {
            var userId = _userManager.GetUserId(User);

            var appointments = await _context.Appointments
                .Include(a => a.Trainer).ThenInclude(t => t.Gym)
                .Include(a => a.Service)
                .Where(a => a.MemberId == userId)
                .OrderByDescending(a => a.Date)
                .ThenByDescending(a => a.StartTime)
                .ToListAsync();

            return View(appointments);
        }

        public async Task<IActionResult> Create(int? gymId, int? trainerId, int? serviceId, DateTime? date)
        {
            await FillDropDowns(gymId, trainerId, serviceId);

            if (trainerId.HasValue && serviceId.HasValue && date.HasValue)
            {
                var slots = await GetAvailableSlots(trainerId.Value, serviceId.Value, date.Value);
                ViewBag.Slots = slots;
            }
            else
            {
                ViewBag.Slots = new List<string>();
            }

            var model = new Appointment
            {
                TrainerId = trainerId ?? 0,
                ServiceId = serviceId ?? 0,
                Date = date ?? DateTime.Today
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int? gymId, Appointment appointment)
        {
            ModelState.Remove(nameof(Appointment.MemberId));
            appointment.MemberId = _userManager.GetUserId(User);
            appointment.Status = AppointmentStatus.Pending;

            if (appointment.StartTime == TimeSpan.Zero || appointment.EndTime == TimeSpan.Zero)
            {
                ModelState.AddModelError(string.Empty, "Lütfen bir saat aralığı seçiniz.");
            }

            await ValidateAppointment(appointment);

            if (!ModelState.IsValid)
            {
                await FillDropDowns(gymId, appointment.TrainerId, appointment.ServiceId);
                var slots = await GetAvailableSlots(appointment.TrainerId, appointment.ServiceId, appointment.Date);
                ViewBag.Slots = slots;
                return View(appointment);
            }

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            if (User.IsInRole("Admin"))
                return RedirectToAction(nameof(Index));

            return RedirectToAction(nameof(MyAppointments));
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var appointment = await _context.Appointments
                .Include(a => a.Trainer).ThenInclude(t => t.Gym)
                .Include(a => a.Service)
                .Include(a => a.Member)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null) return NotFound();

            return View(appointment);
        }


        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var list = await _context.Appointments
                .Include(a => a.Trainer).ThenInclude(t => t.Gym)
                .Include(a => a.Service)
                .Include(a => a.Member)
                .OrderByDescending(a => a.Date)
                .ThenByDescending(a => a.StartTime)
                .ToListAsync();

            return View(list);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var appointment = await _context.Appointments
                .Include(a => a.Trainer)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null) return NotFound();

            var gymId = appointment.Trainer?.GymId;
            await FillDropDowns(gymId, appointment.TrainerId, appointment.ServiceId);

            ViewBag.Slots = new List<string>();

            return View(appointment);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, int? gymId, Appointment appointment)
        {
            if (id != appointment.Id) return NotFound();

            ModelState.Remove(nameof(Appointment.MemberId));
            ModelState.Remove(nameof(Appointment.Status));

            var existing = await _context.Appointments
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == id);

            if (existing == null) return NotFound();

            appointment.MemberId = existing.MemberId;
            appointment.Status = existing.Status;

            await ValidateAppointment(appointment);

            if (!ModelState.IsValid)
            {
                await FillDropDowns(gymId, appointment.TrainerId, appointment.ServiceId);
                ViewBag.Slots = new List<string>();
                return View(appointment);
            }

            _context.Update(appointment);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var appointment = await _context.Appointments
                .Include(a => a.Trainer).ThenInclude(t => t.Gym)
                .Include(a => a.Service)
                .Include(a => a.Member)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null) return NotFound();

            return View(appointment);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment != null)
            {
                _context.Appointments.Remove(appointment);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return NotFound();

            appointment.Status = AppointmentStatus.Approved;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return NotFound();

            appointment.Status = AppointmentStatus.Rejected;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private async Task FillDropDowns(int? gymId, int? trainerId, int? serviceId)
        {
            var gyms = await _context.Gyms
                .OrderBy(g => g.Name)
                .ToListAsync();
            ViewBag.GymId = new SelectList(gyms, "Id", "Name", gymId);

            var trainersQuery = _context.Trainers.AsQueryable();
            if (gymId.HasValue)
                trainersQuery = trainersQuery.Where(t => t.GymId == gymId.Value);

            var trainers = await trainersQuery
                .OrderBy(t => t.FullName)
                .ToListAsync();
            ViewBag.TrainerId = new SelectList(trainers, "Id", "FullName", trainerId);

            var servicesQuery = _context.Services.AsQueryable();

            if (trainerId.HasValue)
            {
                var trainer = await _context.Trainers
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.Id == trainerId.Value);

                if (trainer?.ServiceId != null)
                {
                    int mainServiceId = trainer.ServiceId.Value;
                    servicesQuery = servicesQuery.Where(s => s.Id == mainServiceId);
                }
                else if (gymId.HasValue)
                {
                    servicesQuery = servicesQuery.Where(s => s.GymId == gymId.Value);
                }
            }
            else if (gymId.HasValue)
            {
                servicesQuery = servicesQuery.Where(s => s.GymId == gymId.Value);
            }

            var services = await servicesQuery
                .OrderBy(s => s.Name)
                .ToListAsync();

            ViewBag.ServiceId = new SelectList(services, "Id", "Name", serviceId);
        }

        private async Task<List<string>> GetAvailableSlots(int trainerId, int serviceId, DateTime date)
        {
            var result = new List<string>();

            var service = await _context.Services.FindAsync(serviceId);
            int duration = service?.DurationMinutes ?? 60;

            var dayOfWeek = date.DayOfWeek;

            var availabilities = await _context.TrainerAvailabilities
                .Where(a => a.TrainerId == trainerId && a.Day == dayOfWeek && a.IsActive)
                .ToListAsync();

            var appointments = await _context.Appointments
                .Where(a => a.TrainerId == trainerId
                            && a.Date == date
                            && a.Status != AppointmentStatus.Rejected)
                .ToListAsync();

            foreach (var av in availabilities)
            {
                var current = av.StartTime;
                var end = av.EndTime;

                while (current + TimeSpan.FromMinutes(duration) <= end)
                {
                    var slotStart = current;
                    var slotEnd = current + TimeSpan.FromMinutes(duration);

                    bool overlap = appointments.Any(a =>
                        (slotStart >= a.StartTime && slotStart < a.EndTime) ||
                        (slotEnd > a.StartTime && slotEnd <= a.EndTime) ||
                        (slotStart <= a.StartTime && slotEnd >= a.EndTime));

                    if (!overlap)
                    {
                        result.Add($"{slotStart:hh\\:mm}-{slotEnd:hh\\:mm}");
                    }

                    current += TimeSpan.FromMinutes(duration);
                }
            }

            return result;
        }

        private async Task ValidateAppointment(Appointment appointment)
        {
            if (appointment.EndTime <= appointment.StartTime)
            {
                ModelState.AddModelError(string.Empty,
                    "Bitiş saati, başlangıç saatinden sonra olmalıdır.");
                return;
            }

            var day = appointment.Date.DayOfWeek;

            bool hasAvailability = await _context.TrainerAvailabilities.AnyAsync(a =>
                a.TrainerId == appointment.TrainerId &&
                a.Day == day &&
                a.IsActive &&
                appointment.StartTime >= a.StartTime &&
                appointment.EndTime <= a.EndTime);

            if (!hasAvailability)
            {
                ModelState.AddModelError(string.Empty,
                    "Seçilen gün ve saat için antrenörün tanımlı bir müsaitliği yoktur.");
            }

            bool overlap = await _context.Appointments.AnyAsync(a =>
                a.TrainerId == appointment.TrainerId &&
                a.Date == appointment.Date &&
                a.Status != AppointmentStatus.Rejected &&
                (
                    (appointment.StartTime >= a.StartTime && appointment.StartTime < a.EndTime) ||
                    (appointment.EndTime > a.StartTime && appointment.EndTime <= a.EndTime) ||
                    (appointment.StartTime <= a.StartTime && appointment.EndTime >= a.EndTime)
                )
            );

            if (overlap)
            {
                ModelState.AddModelError(string.Empty,
                    "Bu saat aralığında antrenörün başka bir randevusu bulunmaktadır.");
            }
        }
    }
}
