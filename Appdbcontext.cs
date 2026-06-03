using Microsoft.EntityFrameworkCore;
using EnrollmentSystemAPI.Models;

namespace EnrollmentSystemAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<FeeSlip>()
        .HasOne(f => f.Student)
        .WithMany()
        .HasForeignKey(f => f.StudentID)
        .OnDelete(DeleteBehavior.Restrict);

    modelBuilder.Entity<FeeSlip>()
        .HasOne(f => f.VerifiedByClerk)
        .WithMany()
        .HasForeignKey(f => f.VerifiedByClerkID)
        .OnDelete(DeleteBehavior.Restrict);
}
        public DbSet<User> Users { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<FeeSlip> FeeSlips { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<DegreeProgram> DegreePrograms { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<EnrollmentSettings> EnrollmentSettings { get; set; }
        public DbSet<Notice> Notices { get; set; }
    }
}