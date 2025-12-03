using System;
using System.ComponentModel.DataAnnotations;

namespace FitnessCenterApp.Models
{
    public class TrainerAvailability
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Antrenör seçimi zorunludur.")]
        [Display(Name = "Antrenör")]
        public int TrainerId { get; set; }

        [Required(ErrorMessage = "Gün alanı zorunludur.")]
        [Display(Name = "Gün")]
        public DayOfWeek Day { get; set; }

        [Required(ErrorMessage = "Başlangıç saati zorunludur.")]
        [Display(Name = "Başlangıç Saati")]
        [DataType(DataType.Time)]
        public TimeSpan StartTime { get; set; }

        [Required(ErrorMessage = "Bitiş saati zorunludur.")]
        [Display(Name = "Bitiş Saati")]
        [DataType(DataType.Time)]
        public TimeSpan EndTime { get; set; }

        [Display(Name = "Aktif mi?")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Antrenör")]
        public Trainer? Trainer { get; set; }
    }
}
