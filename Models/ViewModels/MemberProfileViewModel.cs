using System.ComponentModel.DataAnnotations;

namespace FitnessCenterApp.Models.ViewModels
{
    public class MemberProfileViewModel
    {
        [Display(Name = "Boy (cm)")]
        [Range(100, 250, ErrorMessage = "Boy 100 ile 250 cm arasında olmalıdır.")]
        public int? HeightCm { get; set; }

        [Display(Name = "Kilo (kg)")]
        [Range(30, 250, ErrorMessage = "Kilo 30 ile 250 kg arasında olmalıdır.")]
        public double? WeightKg { get; set; }

        [Display(Name = "Hedef Türü")]
        [StringLength(50)]
        public string? GoalType { get; set; }

        [Display(Name = "Haftalık Antrenman Sayısı")]
        [Range(1, 14, ErrorMessage = "1 ile 14 arasında olmalıdır.")]
        public int? WorkoutsPerWeek { get; set; }

        [Display(Name = "Notlar")]
        [StringLength(500)]
        public string? Notes { get; set; }
    }
}
