using EmployeeDepartment.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeDepartment.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<EmployeeProfile> EmployeeProfiles => Set<EmployeeProfile>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<AppUser> Users => Set<AppUser>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Department>()
            .Property(x => x.Name)
            .HasMaxLength(100);

        modelBuilder.Entity<Department>()
            .HasIndex(x => x.Name)
            .IsUnique();

        modelBuilder.Entity<Employee>()
            .Property(x => x.FullName)
            .HasMaxLength(120);

        modelBuilder.Entity<Employee>()
            .Property(x => x.Email)
            .HasMaxLength(150);

        modelBuilder.Entity<Employee>()
            .Property(x => x.JobTitle)
            .HasMaxLength(100);

        modelBuilder.Entity<Employee>()
            .Property(x => x.Salary)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<Employee>()
            .HasIndex(x => x.Email)
            .IsUnique();

        modelBuilder.Entity<Employee>()
            .HasOne(x => x.Department)
            .WithMany(x => x.Employees)
            .HasForeignKey(x => x.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Employee>()
            .HasOne(x => x.Profile)
            .WithOne(x => x.Employee)
            .HasForeignKey<EmployeeProfile>(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<EmployeeProfile>()
            .HasIndex(x => x.EmployeeId)
            .IsUnique();

        modelBuilder.Entity<Project>()
            .Property(x => x.Name)
            .HasMaxLength(120);

        modelBuilder.Entity<Project>()
            .Property(x => x.Description)
            .HasMaxLength(500);

        modelBuilder.Entity<Project>()
            .Property(x => x.Budget)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<Employee>()
            .HasMany(x => x.Projects)
            .WithMany(x => x.Employees)
            .UsingEntity<Dictionary<string, object>>(
                "EmployeeProjects",
                right => right
                    .HasOne<Project>()
                    .WithMany()
                    .HasForeignKey("ProjectId")
                    .OnDelete(DeleteBehavior.Cascade),
                left => left
                    .HasOne<Employee>()
                    .WithMany()
                    .HasForeignKey("EmployeeId")
                    .OnDelete(DeleteBehavior.Cascade),
                join =>
                {
                    join.HasKey("EmployeeId", "ProjectId");
                    join.ToTable("EmployeeProjects");
                });

        modelBuilder.Entity<AppUser>()
            .Property(x => x.FullName)
            .HasMaxLength(120);

        modelBuilder.Entity<AppUser>()
            .Property(x => x.UserName)
            .HasMaxLength(50);

        modelBuilder.Entity<AppUser>()
            .Property(x => x.Email)
            .HasMaxLength(150);

        modelBuilder.Entity<AppUser>()
            .Property(x => x.Role)
            .HasMaxLength(20);

        modelBuilder.Entity<AppUser>()
            .HasIndex(x => x.UserName)
            .IsUnique();
    }
}
