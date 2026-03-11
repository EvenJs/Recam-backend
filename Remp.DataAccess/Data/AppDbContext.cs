using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Remp.Models.Entities;

namespace Remp.DataAccess.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<ListingCase> ListingCases { get; set; }
    public DbSet<MediaAsset> MediaAssets { get; set; }
    public DbSet<CaseContact> CaseContacts { get; set; }
    public DbSet<AgentListingCase> AgentListingCases { get; set; }
    public DbSet<AgentPhotographyCompany> AgentPhotographyCompanies { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // TPH inheritance
        builder.Entity<ApplicationUser>()
            .HasDiscriminator<string>("UserType")
            .HasValue<ApplicationUser>("User")
            .HasValue<PhotographyCompany>("PhotographyCompany")
            .HasValue<Agent>("Agent");

        // Primary keys
        builder.Entity<CaseContact>().HasKey(c => c.ContactId);
        builder.Entity<MediaAsset>().HasKey(m => m.Id);
        builder.Entity<ListingCase>().HasKey(l => l.Id);

        // Latitude/Longitude precision
        builder.Entity<ListingCase>()
            .Property(l => l.Latitude).HasPrecision(9, 6);
        builder.Entity<ListingCase>()
            .Property(l => l.Longitude).HasPrecision(9, 6);

        // ListingCase → PhotographyCompany
        builder.Entity<ListingCase>()
            .HasOne(l => l.PhotographyCompany)
            .WithMany(p => p.ListingCases)
            .HasForeignKey(l => l.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // CaseContact → ListingCase
        builder.Entity<ListingCase>()
            .HasMany(l => l.CaseContacts)
            .WithOne(c => c.ListingCase)
            .HasForeignKey(c => c.ListingCaseId)
            .OnDelete(DeleteBehavior.Restrict);

        // MediaAsset → ListingCase
        builder.Entity<ListingCase>()
            .HasMany(l => l.MediaAssets)
            .WithOne(m => m.ListingCase)
            .HasForeignKey(m => m.ListingCaseId)
            .OnDelete(DeleteBehavior.Restrict);

        // MediaAsset → ApplicationUser
        builder.Entity<MediaAsset>()
            .HasOne(m => m.UploadedBy)
            .WithMany()
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // AgentListingCase - Composite PK
        builder.Entity<AgentListingCase>()
            .HasKey(al => new { al.AgentId, al.ListingCaseId });

        builder.Entity<ListingCase>()
            .HasMany(l => l.AgentListingCases)
            .WithOne(al => al.ListingCase)
            .HasForeignKey(al => al.ListingCaseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Agent>()
            .HasMany(a => a.AgentListingCases)
            .WithOne(al => al.Agent)
            .HasForeignKey(al => al.AgentId)
            .OnDelete(DeleteBehavior.Restrict);

        // AgentPhotographyCompany - Composite PK
        builder.Entity<AgentPhotographyCompany>()
            .HasKey(ap => new { ap.AgentId, ap.PhotographyCompanyId });

        builder.Entity<Agent>()
            .HasMany(a => a.AgentPhotographyCompanies)
            .WithOne(ap => ap.Agent)
            .HasForeignKey(ap => ap.AgentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<PhotographyCompany>()
            .HasMany(p => p.AgentPhotographyCompanies)
            .WithOne(ap => ap.PhotographyCompany)
            .HasForeignKey(ap => ap.PhotographyCompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        // Soft delete filters
        builder.Entity<ListingCase>()
            .HasQueryFilter(l => !l.IsDeleted);
        builder.Entity<MediaAsset>()
            .HasQueryFilter(m => !m.IsDeleted);
    }
}