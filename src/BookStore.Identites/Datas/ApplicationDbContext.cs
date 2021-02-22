using Microsoft.EntityFrameworkCore;

namespace BookStore.Identities.Datas
{
  public class ApplicationDbContext : DbContext
  {
    public ApplicationDbContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      modelBuilder.HasDefaultSchema("ids");
      base.OnModelCreating(modelBuilder);
    }
  }
}