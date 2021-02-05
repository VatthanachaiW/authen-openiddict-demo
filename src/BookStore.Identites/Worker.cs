using System;
using System.Threading;
using System.Threading.Tasks;
using BookStore.Identites.Datas;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenIddict.Abstractions;

namespace BookStore.Identites
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
      using var scope = _serviceProvider.CreateScope();
      {
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await context.Database.EnsureCreatedAsync();
        var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();

        if (await manager.FindByClientIdAsync("client") == null)
        {
          await manager.CreateAsync(new OpenIddictApplicationDescriptor
          {
            ClientId = "client",
            ClientSecret = "secret",
            DisplayName = "Client application",
            Permissions =
            {
              OpenIddictConstants.Permissions.Endpoints.Token,
              OpenIddictConstants.Permissions.GrantTypes.ClientCredentials,
              OpenIddictConstants.Permissions.Scopes.Email,
              OpenIddictConstants.Permissions.Scopes.Profile,
              OpenIddictConstants.Permissions.Scopes.Roles,
              OpenIddictConstants.Permissions.Prefixes.Scope + "api",
            }
          });

          await manager.CreateAsync(new OpenIddictApplicationDescriptor
          {
            ClientId = "book_api",
            //ClientSecret = "secret",
            DisplayName = "API application",
            Permissions =
            {
              OpenIddictConstants.Permissions.Endpoints.Introspection,
            }
          });
        }

        var scopeManager = scope.ServiceProvider.GetRequiredService<IOpenIddictScopeManager>();
        if (await scopeManager.FindByNameAsync("api") == null)
        {
          await scopeManager.CreateAsync(new OpenIddictScopeDescriptor
          {
            Name = "api",
            Resources =
            {
              "book_api"
            }
          });
        }
      }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
  }
}