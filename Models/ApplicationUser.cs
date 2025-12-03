using Microsoft.AspNetCore.Identity;

namespace FitnessCenterApp.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
        public DateTime? BirthDate { get; set; }
        public ICollection<Appointment>? Appointments { get; set; }

        public ICollection<MemberGoal> MemberGoals { get; set; } = new List<MemberGoal>();
    }
}
