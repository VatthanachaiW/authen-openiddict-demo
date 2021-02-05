using Microsoft.EntityFrameworkCore;

namespace BookStore.Identites.Datas
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