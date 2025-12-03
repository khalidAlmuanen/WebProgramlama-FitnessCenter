using System;
using System.ComponentModel.DataAnnotations;

namespace FitnessCenterApp.Models
{
    public enum AppointmentStatus
    {
        [Display(Name = "Beklemede")]
        Pending = 0,

        [Display(Name = "Onaylandı")]
        Approved = 1,

        [Display(Name = "Reddedildi")]
        Rejected = 2
    }

    public class Appointment
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Üye")]
        public string MemberId { get; set; }

        [Required]
        [Display(Name = "Antrenör")]
        public int TrainerId { get; set; }

        [Required]
        [Display(Name = "Hizmet")]
        public int ServiceId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Tarih")]
        public DateTime Date { get; set; }

        [Required]
        [DataType(DataType.Time)]
        [Display(Name = "Başlangıç Saati")]
        public TimeSpan StartTime { get; set; }

        [Required]
        [DataType(DataType.Time)]
        [Display(Name = "Bitiş Saati")]
        public TimeSpan EndTime { get; set; }

        [Display(Name = "Durum")]
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;

        [MaxLength(500)]
        [Display(Name = "Notlar")]
        public string? Notes { get; set; }

        [Display(Name = "Üye")]
        public ApplicationUser? Member { get; set; }

        [Display(Name = "Antrenör")]
        public Trainer? Trainer { get; set; }

        [Display(Name = "Hizmet")]
        public Service? Service { get; set; }
    }
}
