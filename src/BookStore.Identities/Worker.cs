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

    public async Task StartAsync(CancellationToken cancellationToken)
    {
      using (var scope = _serviceProvider.CreateScope())
      {
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await context.Database.EnsureCreatedAsync(cancellationToken);

        var scopeManager = scope.ServiceProvider.GetRequiredService<IOpenIddictScopeManager>();
        if (await scopeManager.FindByNameAsync("book_api", cancellationToken) == null)
        {
          await scopeManager.CreateAsync(new OpenIddictScopeDescriptor
          {
            Name = "book_api",
            Resources = {"book_api"},
            Description = "Book API"
          }, cancellationToken);
        }
      }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
  }
}