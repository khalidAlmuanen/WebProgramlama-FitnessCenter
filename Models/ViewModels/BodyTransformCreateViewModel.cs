using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace FitnessCenterApp.Models.ViewModels
{
    public class BodyTransformCreateViewModel
    {
        [Required(ErrorMessage = "Lütfen bir hedef seçin.")]
        [Display(Name = "Hedefiniz")]
        public string GoalType { get; set; } = string.Empty; // Kilo verme / Kas artırma / Form koruma

        [Required(ErrorMessage = "Lütfen süreyi ay olarak girin.")]
        [Range(1, 60, ErrorMessage = "Süre 1 ile 60 ay arasında olmalıdır.")]
        [Display(Name = "Tahmini Süre (Ay)")]
        public int DurationMonths { get; set; }

        [Required(ErrorMessage = "Lütfen başlangıç kilonuzu girin.")]
        [Range(30, 300, ErrorMessage = "Kilo 30 ile 300 kg arasında olmalıdır.")]
        [Display(Name = "Başlangıç Kilonuz (kg)")]
        public double? StartWeightKg { get; set; }

        [Required(ErrorMessage = "Lütfen bir fotoğraf yükleyin.")]
        [Display(Name = "Vücut Fotoğrafınız")]
        public IFormFile ImageFile   { get; set; } = null!;
    }
}
