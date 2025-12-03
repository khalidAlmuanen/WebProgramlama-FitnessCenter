using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FitnessCenterApp.Models
{
    public class Trainer
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ad Soyad alanı zorunludur.")]
        [MaxLength(150, ErrorMessage = "Ad Soyad en fazla 150 karakter olabilir.")]
        [Display(Name = "Ad Soyad")]
        public string FullName { get; set; }

        [MaxLength(100, ErrorMessage = "Uzmanlık alanı en fazla 100 karakter olabilir.")]
        [Display(Name = "Uzmanlık Alanı")]
        public string? Specialization { get; set; } 
        [Range(0, 50, ErrorMessage = "Deneyim yılı 0 ile 50 arasında olmalıdır.")]
        [Display(Name = "Deneyim (Yıl)")]
        public int? ExperienceYears { get; set; }  

        [Range(0, 5, ErrorMessage = "Puan 0 ile 5 arasında olmalıdır.")]
        [Display(Name = "Puan")]
        public double? Rating { get; set; }  

        [Required(ErrorMessage = "Spor salonu seçimi zorunludur.")]
        [Display(Name = "Spor Salonu")]
        public int GymId { get; set; }

        [Display(Name = "Ana Hizmet")]
        public int? ServiceId { get; set; }

        public Gym? Gym { get; set; }

        [Display(Name = "Hizmet")]
        public Service? Service { get; set; }

        [Display(Name = "Müsaitlikler")]
        public ICollection<TrainerAvailability>? Availabilities { get; set; } = new List<TrainerAvailability>();

        [Display(Name = "Randevular")]
        public ICollection<Appointment>? Appointments { get; set; } = new List<Appointment>();
    }
}
