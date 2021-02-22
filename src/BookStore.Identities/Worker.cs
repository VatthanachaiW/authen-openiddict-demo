using System;
using System.Threading;
using System.Threading.Tasks;
using BookStore.Identities.Contexts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenIddict.Abstractions;

namespace BookStore.Identities
{
  public class Worker : IHostedService
  {
    private readonly IServiceProvider _serviceProvider;

    public Worker(IServiceProvider serviceProvider)
    {
      _serviceProvider = serviceProvider;
    }

    private async Task CreateScopeAsync(IOpenIddictScopeManager manager)
    {
      if (await manager.FindByNameAsync("book_api") == null)
      {
        var descriptor = new OpenIddictScopeDescriptor
        {
          Name = "book_api",
          Resources =
          {
            "book_api_resource"
          }
        };

        await manager.CreateAsync(descriptor);
      }
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
      using var scope = _serviceProvider.CreateScope();

      var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
      await context.Database.EnsureCreatedAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
  }
}