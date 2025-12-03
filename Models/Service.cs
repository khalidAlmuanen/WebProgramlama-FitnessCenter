using System.ComponentModel.DataAnnotations;

namespace FitnessCenterApp.Models
{
    public class Service
    {
        public int Id { get; set; }

        [Required, MaxLength(150)]
        public string Name { get; set; }

        [MaxLength(100)]
        public string? Category { get; set; }  

        [Range(1, 600)]
        public int DurationMinutes { get; set; }

        [Range(0, 100000)]
        public decimal Price { get; set; }     

        public string? Description { get; set; }

        [Required]
        [Display(Name = "Gym")]
        public int GymId { get; set; }

        public Gym? Gym { get; set; }
        public ICollection<Trainer>? Trainers { get; set; }
        public ICollection<Appointment>? Appointments { get; set; }
    }
}
