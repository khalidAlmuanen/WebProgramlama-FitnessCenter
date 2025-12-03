using FitnessCenterApp.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenterApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Gym> Gyms { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Trainer> Trainers { get; set; }
        public DbSet<TrainerAvailability> TrainerAvailabilities { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<MemberGoal> MemberGoals { get; set; }
        public DbSet<BodyTransformRequest> BodyTransformRequests { get; set; }



        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Service>()
                .HasOne(s => s.Gym)
                .WithMany(g => g.Services)
                .HasForeignKey(s => s.GymId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Trainer>()
                .HasOne(t => t.Gym)
                .WithMany(g => g.Trainers)
                .HasForeignKey(t => t.GymId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Trainer>()
                .HasOne(t => t.Service)
                .WithMany(s => s.Trainers)
                .HasForeignKey(t => t.ServiceId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<TrainerAvailability>()
                .HasOne(a => a.Trainer)
                .WithMany(t => t.Availabilities)
                .HasForeignKey(a => a.TrainerId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Appointment>()
                .HasOne(a => a.Member)
                .WithMany(u => u.Appointments)
                .HasForeignKey(a => a.MemberId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Appointment>()
                .HasOne(a => a.Trainer)
                .WithMany(t => t.Appointments)
                .HasForeignKey(a => a.TrainerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Appointment>()
                .HasOne(a => a.Service)
                .WithMany(s => s.Appointments)
                .HasForeignKey(a => a.ServiceId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Service>()
                .Property(s => s.Price)
                .HasPrecision(10, 2);


                builder.Entity<Gym>()
                .HasIndex(g => g.Name)
                .IsUnique();

                builder.Entity<Service>()
                .HasIndex(s => new { s.Name, s.GymId })
                .IsUnique();

                builder.Entity<Trainer>()
                .HasIndex(t => new { t.FullName, t.GymId })
                .IsUnique();

        }
    }
}
