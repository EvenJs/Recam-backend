using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Remp.Models.Entities;

namespace Remp.DataAccess.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
  public AppDbContext(DbContextOptions<AppContext> options) : base (options)
  {
  }

  public DbSet<ListingCase> ListingCases { get; set; }
  public DbSet<MediaAsset> MediaAssets { get; set; }
  public DbSet<CaseContact> CaseContacts { get; set; }
  public DbSet<AgentListingCase> AgentListingCases { get; set; }
  public DbSet<AgentPhotographyCompany> AgentPhotographyCompanies { get; set; }

  protected override void OModelCreating(ModelBuilder builder)
  {
    base.OnModelCreating(builder);

    // TPH inheritance - store PhotographyCompany and Agent in same User table
    builder.Entity<ApplicationUser>()
      .HasDiscriminator<string>("UserType")
      .HasValue<ApplicationUser>("User")
      .HasValue<PhotographyCompany>("PhotographyCompany")
      .HasValue<Agent>("Agent");

    // Composite PK for AgentListingCase
    builder.Entity<AgentListingCase>()
      .HasKey(al => new { al.AgentId, al.ListingCaseId });

    builder.Entity<AgentListingCase>()
      .HasOne(al => al.Agent)
      .WithMany(a => a.AgentListingCases)
      .HasForeignKey(al => al.AgentId);

    builder.Entity<AgentListingCase>()
      .HasOne(al => al.ListingCase)
      .WithMany(l => l.AgentListingCases)
      .HasForeignKey(al => al.ListingCaseId);

    // Composite PK for AgentPhotographyCompany
    builder.Entity<AgentPhotographyCompany>()
      .HasKey(ap => new { ap.AgentId, ap.PhotographyCompanyId });

    builder.Entity<AgentPhotographyCompany>()
      .HasOne(ap => ap.Agent)
      .WithMany(a => a.AgentPhotographyCompanies)
      .HasForeignKey(ap => ap.AgentId);

    builder.Entity<AgentPhotographyCompany>()
      .HasOne(ap => ap.PhotographyCompany)
      .WithMany(p => p.AgentPhotographyCompanies)
      .HasForeignKey(ap => ap.PhotographyCompanyId);

    // ListingCase → PhotographyCompany
    builder.Entity<ListingCase>()
      .HasOne(l => l.PhotographyCompany)
      .WithMany(p => p.ListingCases)
      .HasForeignKey(l => l.UserId);

    // Soft delete filter - never return deleted records
    builder.Entity<ListingCase>()
      .HasQueryFilter(l => !l.IsDeleted);

    builder.Entity<MediaAsset>()
      .HasQueryFilter(m => !m.IsDeleted);
  }
}