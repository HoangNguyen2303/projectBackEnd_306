using EduTrack.Application.Interfaces;
using EduTrack.Domain.Entities;
using EduTrack.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<AppUser>, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Teacher> Teachers => Set<Teacher>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<ClassSection> Classes => Set<ClassSection>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<Grade> Grades => Set<Grade>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // --- unique keys ---
        builder.Entity<Department>().HasIndex(x => x.Code).IsUnique();
        builder.Entity<Student>().HasIndex(x => x.StudentCode).IsUnique();
        builder.Entity<Student>().HasIndex(x => x.Email).IsUnique();
        builder.Entity<Teacher>().HasIndex(x => x.TeacherCode).IsUnique();
        builder.Entity<Teacher>().HasIndex(x => x.Email).IsUnique();
        builder.Entity<Course>().HasIndex(x => x.CourseCode).IsUnique();
        builder.Entity<ClassSection>().HasIndex(x => x.ClassCode).IsUnique();

        // --- Department schema constraints ---
        builder.Entity<Department>(entity =>
        {
            entity.Property(x => x.Code).HasMaxLength(10).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(120).IsRequired();
        });

        // --- Course / Class / Enrollment schema constraints ---
        builder.Entity<Course>(entity =>
        {
            entity.Property(x => x.CourseCode).HasMaxLength(20).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(150).IsRequired();
            entity.ToTable("Courses", table =>
                table.HasCheckConstraint("CK_Courses_Credits", "[Credits] BETWEEN 1 AND 10"));
        });

        builder.Entity<ClassSection>(entity =>
        {
            entity.Property(x => x.ClassCode).HasMaxLength(20).IsRequired();
            entity.Property(x => x.Semester).HasMaxLength(20).IsRequired();
            entity.ToTable("Classes", table =>
                table.HasCheckConstraint("CK_Classes_MaxCapacity", "[MaxCapacity] > 0"));
        });

        // a student cannot enroll in the same class twice
        builder.Entity<Enrollment>().HasIndex(x => new { x.StudentId, x.ClassId }).IsUnique();
        // one grade per enrollment
        builder.Entity<Grade>().HasIndex(x => x.EnrollmentId).IsUnique();

        // money/score precision
        builder.Entity<Grade>().Property(x => x.Attendance).HasPrecision(5, 2);
        builder.Entity<Grade>().Property(x => x.Midterm).HasPrecision(5, 2);
        builder.Entity<Grade>().Property(x => x.Final).HasPrecision(5, 2);
        builder.Entity<Grade>().Property(x => x.Total).HasPrecision(5, 2);

        // avoid multiple cascade paths
        builder.Entity<Enrollment>()
            .HasOne(e => e.Class).WithMany(c => c.Enrollments)
            .HasForeignKey(e => e.ClassId).OnDelete(DeleteBehavior.Restrict);


        // Class is reachable from Department via both Course and Teacher (both cascade) —
        // restrict the Teacher edge so SQL Server doesn't reject the FK for multiple cascade paths.
        builder.Entity<ClassSection>()
            .HasOne(c => c.Teacher).WithMany(t => t.Classes)
            .HasForeignKey(c => c.TeacherId).OnDelete(DeleteBehavior.Restrict);

        // Prevent accidental deletion of a course together with all of its classes.
        builder.Entity<ClassSection>()
            .HasOne(c => c.Course).WithMany(c => c.Classes)
            .HasForeignKey(c => c.CourseId).OnDelete(DeleteBehavior.Restrict);

        // Departments with related academic data must be deleted explicitly and safely.
        builder.Entity<Student>()
            .HasOne(student => student.Department).WithMany(department => department.Students)
            .HasForeignKey(student => student.DepartmentId).OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Teacher>()
            .HasOne(teacher => teacher.Department).WithMany(department => department.Teachers)
            .HasForeignKey(teacher => teacher.DepartmentId).OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Course>()
            .HasOne(course => course.Department).WithMany(department => department.Courses)
            .HasForeignKey(course => course.DepartmentId).OnDelete(DeleteBehavior.Restrict);

        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
