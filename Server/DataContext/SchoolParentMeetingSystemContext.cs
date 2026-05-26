using Microsoft.EntityFrameworkCore;
using Repository;
using Repository.Entities;
using Repository.Interfaces;

namespace DataContext
{
    public partial class SchoolParentMeetingSystemContext : DbContext, IContext
    {
        public SchoolParentMeetingSystemContext()
        { }

        public SchoolParentMeetingSystemContext(DbContextOptions<SchoolParentMeetingSystemContext> options)
            : base(options)
        { }

        public DbSet<Student> Students { get; set; }
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<ParentMeeting> ParentMeetings { get; set; }
        public DbSet<ParentAvailability> ParentAvailability { get; set; }
        public DbSet<Parent> Parents { get; set; }
        public DbSet<School> Schools { get; set; }

        public async Task Save()
        {
            await SaveChangesAsync();
        }

        //How to connect to the database
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(
                    "Server=RACHEL\\SQLEXPRESS;Database=SchoolParentMeetingSystem;Trusted_Connection=True;TrustServerCertificate=True"
                );
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // School
            modelBuilder.Entity<School>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                      .HasMaxLength(50)
                      .IsRequired();

                entity.Property(e => e.Password)
                      .IsRequired();

                entity.Property(e => e.Role)
                      .HasMaxLength(20)
                      .IsRequired()
                      .HasDefaultValue("School");

                entity.Property(e => e.MeetingDate)
                      .IsRequired();

                entity.Property(e => e.MeetingStartTime)
                      .IsRequired();

                entity.Property(e => e.MeetingEndTime)
                      .IsRequired();

                entity.Property(e => e.SlotDurationMinutes)
                      .IsRequired();

                entity.HasMany(e => e.Students)
                      .WithOne(s => s.School)
                      .HasForeignKey(s => s.SchoolId)
                      //All students are automatically deleted.
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.Parents)
                      .WithOne()
                      .HasForeignKey(p => p.SchoolId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.parentMeetings)
                      .WithOne()
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Parent
            modelBuilder.Entity<Parent>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasIndex(e => e.ParentIdentity)
                      .IsUnique();

                entity.HasIndex(e => e.ParentEmail)
                      .IsUnique();

                entity.Property(e => e.ParentIdentity)
                      .HasMaxLength(12)
                      .IsRequired();

                entity.Property(e => e.ParentName)
                      .HasMaxLength(50)
                      .IsRequired();

                entity.Property(e => e.ParentEmail)
                      .HasMaxLength(50);

                entity.Property(e => e.SchoolId)
                      .IsRequired();

                entity.HasMany(e => e.Students)
                      .WithOne(s => s.Parent)
                      .HasForeignKey(s => s.ParentId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.ParentMeetings)
                      .WithOne(m => m.Parent)
                      .HasForeignKey(m => m.ParentId)
                      .OnDelete(DeleteBehavior.Restrict);

            });

            // Teacher
            modelBuilder.Entity<Teacher>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.FullName)
                      .HasMaxLength(50)
                      .IsRequired();

                entity.Property(e => e.ClassName)
                      .HasMaxLength(5)
                      .IsRequired();

                entity.HasOne(e => e.School)
                      .WithMany()
                      .HasForeignKey(e => e.SchoolId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.Students)
                      .WithOne(s => s.Teacher)
                      .HasForeignKey(s => s.TeacherId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(e => e.ParentMeetings)
                      .WithOne(m => m.Teacher) 
                      .HasForeignKey(m => m.TeacherId) 
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Student
            modelBuilder.Entity<Student>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasIndex(e => e.StudentIdentity)
                      .IsUnique();

                entity.Property(e => e.StudentIdentity)
                      .HasMaxLength(12)
                      .IsRequired();

                entity.Property(e => e.FirstName)
                      .HasMaxLength(50);

                entity.Property(e => e.LastName)
                      .HasMaxLength(50);

                entity.Property(e => e.ClassName)
                      .HasMaxLength(5)
                      .IsRequired();

                entity.HasOne(e => e.Parent)
                       .WithMany(p => p.Students)
                       .HasForeignKey(e => e.ParentId)
                       .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.School)
                      .WithMany(s => s.Students)
                      .HasForeignKey(e => e.SchoolId)
                      .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.Teacher)
                      .WithMany(t => t.Students)
                      .HasForeignKey(e => e.TeacherId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ParentAvailability
            modelBuilder.Entity<ParentAvailability>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.SchoolId)
                      .IsRequired();

                entity.Property(e => e.ParentIdentity)
                      .HasMaxLength(12)
                      .IsRequired();

                entity.Property(e => e.MeetingDate)
                      .IsRequired();

                entity.Property(e => e.StartTime)
                      .IsRequired();

                entity.Property(e => e.EndTime)
                      .IsRequired();

                entity.Property(e => e.IsAvailable)
                      .IsRequired();
            });

            // ParentMeeting
            modelBuilder.Entity<ParentMeeting>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.ClassName)
                      .HasMaxLength(5)
                      .IsRequired();

                entity.Property(e => e.SchoolId)
                      .IsRequired();

                entity.Property(e => e.MeetingDate)
                      .IsRequired();

                entity.Property(e => e.StartTime)
                      .IsRequired();

                entity.Property(e => e.EndTime)
                      .IsRequired();

                entity.Property(e => e.IsPast)
                      .HasDefaultValue(false);

                entity.HasOne(e => e.Student)
                      .WithMany()
                      .HasForeignKey(e => e.StudentId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Parent)
                      .WithMany(p => p.ParentMeetings)
                      .HasForeignKey(e => e.ParentId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Teacher)
                      .WithMany(t => t.ParentMeetings)
                      .HasForeignKey(e => e.TeacherId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}