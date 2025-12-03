using System;
using System.Linq;
using System.Threading.Tasks;
using FitnessCenterApp.Data;
using FitnessCenterApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenterApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReportsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Authorize]
        [HttpGet("trainers")]
        public async Task<IActionResult> GetTrainers()
        {
            var result = await _context.Trainers
                .Include(t => t.Gym)
                .Include(t => t.Service)
                .Select(t => new
                {
                    t.Id,
                    t.FullName,
                    t.Specialization,
                    GymName = t.Gym.Name,
                    ServiceName = t.Service != null ? t.Service.Name : null,
                    t.ExperienceYears,
                    t.Rating
                })
                .ToListAsync();

            return Ok(result);
        }

        [Authorize]
        [HttpGet("available-trainers")]
        public async Task<IActionResult> GetAvailableTrainers([FromQuery] DateTime date)
        {
            var day = date.DayOfWeek;

            var availabilities = await _context.TrainerAvailabilities
                .Include(a => a.Trainer)
                    .ThenInclude(t => t.Gym)
                .Where(a => a.Day == day && a.IsActive)
                .ToListAsync();

            if (!availabilities.Any())
                return Ok(Array.Empty<object>());

            var trainerIds = availabilities.Select(a => a.TrainerId).Distinct().ToList();

            var appointments = await _context.Appointments
                .Where(a =>
                    trainerIds.Contains(a.TrainerId) &&
                    a.Date == date.Date &&
                    a.Status != AppointmentStatus.Rejected)
                .ToListAsync();

            var result = availabilities
                .GroupBy(a => a.Trainer)
                .Select(g => new
                {
                    TrainerId = g.Key.Id,
                    TrainerName = g.Key.FullName,
                    GymName = g.Key.Gym.Name,
                    AppointmentCount = appointments.Count(x => x.TrainerId == g.Key.Id),
                    Slots = g.Select(a => new
                    {
                        a.StartTime,
                        a.EndTime
                    })
                });

            return Ok(result);
        }

        [Authorize]
        [HttpGet("my-appointments")]
        public async Task<IActionResult> GetMyAppointments()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var result = await _context.Appointments
                .Include(a => a.Trainer)
                    .ThenInclude(t => t.Gym)
                .Include(a => a.Service)
                .Where(a => a.MemberId == user.Id)
                .OrderByDescending(a => a.Date)
                .ThenByDescending(a => a.StartTime)
                .Select(a => new
                {
                    a.Id,
                    Date = a.Date,
                    a.StartTime,
                    a.EndTime,
                    Trainer = a.Trainer.FullName,
                    Gym = a.Trainer.Gym.Name,
                    Service = a.Service.Name,
                    Status = a.Status.ToString(),
                    a.Notes
                })
                .ToListAsync();

            return Ok(result);
        }
    }
}
