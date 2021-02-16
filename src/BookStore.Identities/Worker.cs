using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BookStore.Identities.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenIddict.Abstractions;

namespace BookStore.Identities
{
  public class Worker : IHostedService
  {
    private IServiceProvider _serviceProvider;

    public Worker(IServiceProvider serviceProvider)
    {
      _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
      using IServiceScope scope = _serviceProvider.CreateScope();

      var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
      await context.Database.EnsureCreatedAsync();

      await CreateUserProfile(scope);
      //await CreateApplicationAsync(scope);
      await CreateScopeAsync(scope);
    }

    private async Task CreateScopeAsync(IServiceScope scope)
    {
      var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictScopeManager>();
      if (await manager.FindByNameAsync("bookapi") == null)
      {
        var descriptor = new OpenIddictScopeDescriptor
        {
          Name = "bookapi",
          Resources = {"book_api"},
          DisplayName = "Book API",
          Description = "Book API Service"
        };

        await manager.CreateAsync(descriptor);
      }
    }

    private async Task CreateUserProfile(IServiceScope scope)
    {
      var manager = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
      if (!await manager.Profiles.AnyAsync())
      {
        var users = new List<Profile>
        {
          new Profile
          {
            Username = "testa",
            Password = "P@ssw0rd!",
            Firstname = "Tester",
            Lastname = "A"
          },
          new Profile
          {
            Username = "testb",
            Password = "P@ssw0rd!",
            Firstname = "Tester",
            Lastname = "B"
          }
        };

        await manager.Profiles.AddRangeAsync(users);
        await manager.SaveChangesAsync();
      }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
  }
}