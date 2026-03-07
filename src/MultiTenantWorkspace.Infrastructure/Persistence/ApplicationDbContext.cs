using Microsoft.EntityFrameworkCore;
using ProjectManagementSaaS.Domain.Entities;

namespace ProjectManagementSaaS.Infrastructure.Persistence;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<ApplicationUser> Users => Set<ApplicationUser>();
    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<TaskComment> TaskComments => Set<TaskComment>();
    public DbSet<WorkTask> Tasks => Set<WorkTask>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Organization>(entity =>
        {
            entity.HasIndex(x => x.Slug).IsUnique();
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Slug).HasMaxLength(100).IsRequired();
        });

        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            entity.HasIndex(x => x.Email).IsUnique();
            entity.Property(x => x.FullName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(200).IsRequired();
            entity.Property(x => x.PasswordHash).IsRequired();
            entity.HasOne(x => x.Organization)
                .WithMany(x => x.Users)
                .HasForeignKey(x => x.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasIndex(x => x.TokenHash).IsUnique();
            entity.Property(x => x.TokenHash).HasMaxLength(200).IsRequired();
            entity.Property(x => x.ReplacedByTokenHash).HasMaxLength(200);
            entity.Property(x => x.ReasonRevoked).HasMaxLength(200);
            entity.HasOne(x => x.User)
                .WithMany(x => x.RefreshTokens)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Project>(entity =>
        {
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(2000);
            entity.HasOne(x => x.Organization)
                .WithMany(x => x.Projects)
                .HasForeignKey(x => x.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.CreatedByUser)
                .WithMany(x => x.CreatedProjects)
                .HasForeignKey(x => x.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<WorkTask>(entity =>
        {
            entity.Property(x => x.Title).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(2000);
            entity.HasOne(x => x.Project)
                .WithMany(x => x.Tasks)
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.AssignedToUser)
                .WithMany(x => x.AssignedTasks)
                .HasForeignKey(x => x.AssignedToUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TaskComment>(entity =>
        {
            entity.Property(x => x.Content).HasMaxLength(2000).IsRequired();
            entity.HasOne(x => x.Task)
                .WithMany(x => x.Comments)
                .HasForeignKey(x => x.TaskId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.AuthorUser)
                .WithMany(x => x.Comments)
                .HasForeignKey(x => x.AuthorUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ActivityLog>(entity =>
        {
            entity.Property(x => x.EntityType).HasMaxLength(100).IsRequired();
            entity.Property(x => x.EntityId).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Action).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(2000).IsRequired();
            entity.HasOne(x => x.Organization)
                .WithMany(x => x.ActivityLogs)
                .HasForeignKey(x => x.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
