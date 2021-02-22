using System;
using BookStore.Identities.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Identities.Contexts
{
  public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
  {
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
      builder.HasDefaultSchema("identities");

      base.OnModelCreating(builder);
    }
  }
}