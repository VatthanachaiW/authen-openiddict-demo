using BookStore.Identities.Models;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Identities
{
  public class ApplicationDbContext : DbContext
  {
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
      builder.HasDefaultSchema("sec");

      base.OnModelCreating(builder);
    }

    public DbSet<Profile> Profiles { get; set; }
  }
}