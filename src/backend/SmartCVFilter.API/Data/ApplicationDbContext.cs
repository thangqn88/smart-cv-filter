using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SmartCVFilter.API.Models;

namespace SmartCVFilter.API.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<JobPost> JobPosts { get; set; }
    public DbSet<Applicant> Applicants { get; set; }
    public DbSet<CVFile> CVFiles { get; set; }
    public DbSet<ScreeningResult> ScreeningResults { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure JobPost relationships
        builder.Entity<JobPost>()
            .HasOne(j => j.User)
            .WithMany(u => u.JobPosts)
            .HasForeignKey(j => j.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure Applicant relationships
        builder.Entity<Applicant>()
            .HasOne(a => a.JobPost)
            .WithMany(j => j.Applicants)
            .HasForeignKey(a => a.JobPostId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure CVFile relationships
        builder.Entity<CVFile>()
            .HasOne(c => c.Applicant)
            .WithMany(a => a.CVFiles)
            .HasForeignKey(c => c.ApplicantId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure ScreeningResult relationships
        builder.Entity<ScreeningResult>()
            .HasOne(s => s.Applicant)
            .WithMany(a => a.ScreeningResults)
            .HasForeignKey(s => s.ApplicantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<ScreeningResult>()
            .HasOne(s => s.JobPost)
            .WithMany()
            .HasForeignKey(s => s.JobPostId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure indexes
        builder.Entity<JobPost>()
            .HasIndex(j => j.Status);

        builder.Entity<JobPost>()
            .HasIndex(j => j.PostedDate);

        builder.Entity<Applicant>()
            .HasIndex(a => a.Status);

        builder.Entity<Applicant>()
            .HasIndex(a => a.AppliedDate);

        builder.Entity<ScreeningResult>()
            .HasIndex(s => s.Status);

        // Configure string properties for JSON storage (PostgreSQL uses text type)
        builder.Entity<ScreeningResult>()
            .Property(s => s.Strengths)
            .HasColumnType("text");

        builder.Entity<ScreeningResult>()
            .Property(s => s.Weaknesses)
            .HasColumnType("text");

        builder.Entity<ScreeningResult>()
            .Property(s => s.DetailedAnalysis)
            .HasColumnType("text");

        // Configure CVFile ExtractedText to use text type for large content
        builder.Entity<CVFile>()
            .Property(c => c.ExtractedText)
            .HasColumnType("text");
    }
}
