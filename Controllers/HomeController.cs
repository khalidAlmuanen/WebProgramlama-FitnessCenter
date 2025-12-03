using FitnessCenterApp.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;
    public HomeController(ApplicationDbContext context)
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
}
