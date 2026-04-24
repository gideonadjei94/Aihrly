using Aihrly.Api.Entities;
using Aihrly.Api.Enums;
using Microsoft.EntityFrameworkCore;

namespace Aihrly.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<TeamMember> TeamMembers => Set<TeamMember>();
    public DbSet<Job> Jobs => Set<Job>();
    public DbSet<Application> Applications => Set<Application>();
    public DbSet<ApplicationNote> ApplicationNotes => Set<ApplicationNote>();
    public DbSet<ApplicationScore> ApplicationScores => Set<ApplicationScore>();
    public DbSet<StageHistory> StageHistories => Set<StageHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureTeamMember(modelBuilder);
        ConfigureJob(modelBuilder);
        ConfigureApplication(modelBuilder);
        ConfigureApplicationNote(modelBuilder);
        ConfigureApplicationScore(modelBuilder);
        ConfigureStageHistory(modelBuilder);

        SeedTeamMembers(modelBuilder);
    }

    private static void ConfigureTeamMember(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TeamMember>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(200);

            // Store enums as strings so the DB is readable without a lookup table
            entity.Property(e => e.Role)
                  .HasConversion<string>()
                  .IsRequired();

            entity.HasIndex(e => e.Email).IsUnique();
        });
    }

    private static void ConfigureJob(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Job>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(300);
            entity.Property(e => e.Description).IsRequired();
            entity.Property(e => e.Location).IsRequired().HasMaxLength(300);
            entity.Property(e => e.CreatedAt).IsRequired();

            entity.Property(e => e.Status)
                  .HasConversion<string>()
                  .IsRequired();

            // We filter jobs by status often, so index it
            entity.HasIndex(e => e.Status);
        });
    }

    private static void ConfigureApplication(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Application>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CandidateName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.CandidateEmail).IsRequired().HasMaxLength(200);
            entity.Property(e => e.AppliedAt).IsRequired();

            entity.Property(e => e.Stage)
                  .HasConversion<string>()
                  .IsRequired();

            // A candidate cannot apply to the same job twice with the same email
            entity.HasIndex(e => new { e.JobId, e.CandidateEmail }).IsUnique();

            // We filter by stage often
            entity.HasIndex(e => new { e.JobId, e.Stage });

            entity.HasOne(e => e.Job)
                  .WithMany(j => j.Applications)
                  .HasForeignKey(e => e.JobId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureApplicationNote(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ApplicationNote>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Description).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();

            entity.Property(e => e.Type)
                  .HasConversion<string>()
                  .IsRequired();

            // The profile endpoint loads all notes for an application, ordered by date
            entity.HasIndex(e => new { e.ApplicationId, e.CreatedAt });

            entity.HasOne(e => e.Application)
                  .WithMany(a => a.Notes)
                  .HasForeignKey(e => e.ApplicationId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Author)
                  .WithMany()
                  .HasForeignKey(e => e.CreatedBy)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureApplicationScore(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ApplicationScore>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Score).IsRequired();
            entity.Property(e => e.ScoredAt).IsRequired();

            entity.Property(e => e.Dimension)
                  .HasConversion<string>()
                  .IsRequired();

            // Enforces one score per dimension per application at the DB level
            entity.HasIndex(e => new { e.ApplicationId, e.Dimension }).IsUnique();

            entity.HasOne(e => e.Application)
                  .WithMany(a => a.Scores)
                  .HasForeignKey(e => e.ApplicationId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Scorer)
                  .WithMany()
                  .HasForeignKey(e => e.ScoredBy)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureStageHistory(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<StageHistory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ChangedAt).IsRequired();
            entity.Property(e => e.FromStage).HasConversion<string>().IsRequired();
            entity.Property(e => e.ToStage).HasConversion<string>().IsRequired();

            // The profile endpoint loads the full history for an application
            entity.HasIndex(e => new { e.ApplicationId, e.ChangedAt });

            entity.HasOne(e => e.Application)
                  .WithMany(a => a.StageHistory)
                  .HasForeignKey(e => e.ApplicationId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ChangedByMember)
                  .WithMany()
                  .HasForeignKey(e => e.ChangedBy)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void SeedTeamMembers(ModelBuilder modelBuilder)
    {
        // Fixed GUIDs so the README can document them and tests can reference them
        modelBuilder.Entity<TeamMember>().HasData(
            new TeamMember
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Name = "Alice Mensah",
                Email = "alice@aihrly.com",
                Role = TeamMemberRole.Recruiter
            },
            new TeamMember
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Name = "Kwame Boateng",
                Email = "kwame@aihrly.com",
                Role = TeamMemberRole.HiringManager
            },
            new TeamMember
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Name = "Sara Osei",
                Email = "sara@aihrly.com",
                Role = TeamMemberRole.Recruiter
            }
        );
    }
}
