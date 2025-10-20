using BookingApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace BookingApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<AppUser, IdentityRole<int>, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<PatientProfile> PatientProfiles { get; set; }
        public DbSet<Specialty> Specialties { get; set; }
        public DbSet<DoctorProfile> DoctorProfiles { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<MedicalRecord> MedicalRecords { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<OtpVerification> OtpVerifications { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Booking relations
            builder.Entity<Booking>()
                .HasOne(b => b.PatientProfile)
                .WithMany(p => p.Bookings)
                .HasForeignKey(b => b.PatientId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Booking>()
                .HasOne(b => b.DoctorProfile)
                .WithMany(d => d.Bookings)
                .HasForeignKey(b => b.DoctorId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Booking>()
                .HasOne(b => b.Service)
                .WithMany(s => s.Bookings)
                .HasForeignKey(b => b.ServiceId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Booking>()
                .HasOne(b => b.Schedule)
                .WithMany(s => s.Bookings)
                .HasForeignKey(b => b.ScheduleId)
                .OnDelete(DeleteBehavior.NoAction);

            // DoctorProfile
            builder.Entity<DoctorProfile>()
                .HasOne(d => d.User)
                .WithOne(u => u.DoctorProfile)
                .HasForeignKey<DoctorProfile>(d => d.UserId);

            builder.Entity<DoctorProfile>()
                 .HasIndex(d => d.UserId)
                 .IsUnique();

            // PatientProfile
            builder.Entity<PatientProfile>()
                .HasOne(p => p.User)
                .WithOne(u => u.PatientProfile)
                .HasForeignKey<PatientProfile>(p => p.UserId);

            builder.Entity<PatientProfile>()
                .HasIndex(p => p.UserId)
                .IsUnique();

            // Notification
            builder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId);

            // Payment
            builder.Entity<Payment>()
                .HasOne(p => p.Booking)
                .WithOne()
                .HasForeignKey<Payment>(p => p.BookingId);

            // Schedule
            builder.Entity<Schedule>()
                .HasOne(s => s.DoctorProfile)
                .WithMany()
                .HasForeignKey(s => s.DoctorId);

            builder.Entity<Schedule>()
                .ToTable(t => t.HasCheckConstraint("CK_Schedule_Time", "[StartTime] < [EndTime]"));

            // MedicalRecord
            builder.Entity<MedicalRecord>()
                .HasOne(m => m.PatientProfile)
                .WithMany(p => p.MedicalRecords)
                .HasForeignKey(m => m.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<MedicalRecord>()
                .HasOne(m => m.DoctorProfile)
                .WithMany()
                .HasForeignKey(m => m.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<MedicalRecord>()
                .HasOne(m => m.Booking)
                .WithOne() // Mỗi booking chỉ có 1 hồ sơ y tế
                .HasForeignKey<MedicalRecord>(m => m.BookingId)
                .OnDelete(DeleteBehavior.Cascade);


            // RefreshTokens
            builder.Entity<RefreshToken>()
                .HasOne(r => r.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Feedback
            builder.Entity<Feedback>()
                .HasOne(f => f.DoctorProfile)
                .WithMany(d => d.Feedbacks)
                .HasForeignKey(f => f.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Feedback>()
               .HasOne(f => f.PatientProfile)
                .WithMany(p => p.Feedbacks)
               .HasForeignKey(f => f.PatientId)
               .OnDelete(DeleteBehavior.Restrict);

            // OTP Configuration
            builder.Entity<OtpVerification>(entity =>
            {
                entity.HasIndex(o => o.Identifier);
                entity.HasIndex(o => new { o.Identifier, o.Code });
                entity.HasIndex(o => o.ExpiresAt);
            });

            // Seed initial data for Specialties
            builder.Entity<Specialty>().HasData(
                new Specialty { Id = 1, Name = "Cardiology" },
                new Specialty { Id = 2, Name = "Dermatology" },
                new Specialty { Id = 3, Name = "Neurology" },
                new Specialty { Id = 4, Name = "Pediatrics" },
                new Specialty { Id = 5, Name = "Psychiatry" }
            );

            // Seed initial data for Services
            builder.Entity<Service>().HasData(
               new Service { Id = 1, Title = "General Consultation", Description = "A general health check-up and consultation.", Price = 50.00m, DurationInMinutes = 30, Status = ServiceStatus.Active },
               new Service { Id = 2, Title = "Specialist Consultation", Description = "Consultation with a medical specialist.", Price = 100.00m, DurationInMinutes = 45, Status = ServiceStatus.Active },
               new Service { Id = 3, Title = "Follow-up Visit", Description = "A follow-up visit to monitor progress.", Price = 30.00m, DurationInMinutes = 20, Status = ServiceStatus.Active },
               new Service { Id = 4, Title = "Diagnostic Test", Description = "Various diagnostic tests as recommended by the doctor.", Price = 150.00m, DurationInMinutes = 60, Status = ServiceStatus.Active },
               new Service { Id = 5, Title = "Therapy Session", Description = "A session for physical or mental therapy.", Price = 80.00m, DurationInMinutes = 60, Status = ServiceStatus.Active }
           );

            // seed initial roles
            builder.Entity<IdentityRole<int>>().HasData(
                new IdentityRole<int> { Id = 1, Name = "Admin", NormalizedName = "ADMIN" },
                new IdentityRole<int> { Id = 2, Name = "Doctor", NormalizedName = "DOCTOR" },
                new IdentityRole<int> { Id = 3, Name = "Patient", NormalizedName = "PATIENT" },
                new IdentityRole<int> { Id = 4, Name = "Staff", NormalizedName = "STAFF" }
            );

            // Seed Admin User (with PasswordHash)
            var hasher = new PasswordHasher<AppUser>();
            var adminUser = new AppUser
            {
                Id = 1,
                UserName = "admin",
                NormalizedUserName = "ADMIN",
                Email = "admin@gmail.com",
                NormalizedEmail = "ADMIN@GMAIL.COM",
                EmailConfirmed = true,
                FullName = "System Administrator",
                SecurityStamp = Guid.NewGuid().ToString()
            };
            adminUser.PasswordHash = hasher.HashPassword(adminUser, "Admin@123");

            builder.Entity<AppUser>().HasData(adminUser);

            // Assign Admin role to Admin User
            builder.Entity<IdentityUserRole<int>>().HasData(
                new IdentityUserRole<int> { UserId = 1, RoleId = 1 }
            );
        }
    }
}
