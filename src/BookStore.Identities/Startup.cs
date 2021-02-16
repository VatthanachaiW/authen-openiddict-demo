using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;

namespace BookStore.Identities
{
  public class Startup
  {
    public Startup(IConfiguration configuration)
    {
      Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
    public void ConfigureServices(IServiceCollection services)
    {
      services.AddDbContext<ApplicationDbContext>(options =>
      {
        options.UseSqlServer(Configuration.GetValue<string>("DefaultDbConnection"));
        options.UseOpenIddict();
      });

      services.Configure<IdentityOptions>(options =>
      {
        options.ClaimsIdentity.UserNameClaimType = OpenIddictConstants.Claims.Name;
        options.ClaimsIdentity.UserIdClaimType = OpenIddictConstants.Claims.Subject;
        options.ClaimsIdentity.RoleClaimType = OpenIddictConstants.Claims.Role;
      });

      services.AddOpenIddict()
        .AddCore(options =>
        {
          options.UseEntityFrameworkCore()
            .UseDbContext<ApplicationDbContext>();
        })
        .AddServer(options =>
        {
          options.SetAuthorizationEndpointUris("/connect/authorize")
            .SetLogoutEndpointUris("/connect/signout")
            .SetIntrospectionEndpointUris("/connect/introspect")
            .SetUserinfoEndpointUris("/connect/info");

          options.RegisterScopes(OpenIddictConstants.Scopes.Email, OpenIddictConstants.Scopes.Profile, OpenIddictConstants.Scopes.Roles);
          options.AllowImplicitFlow();

          options.AddEncryptionKey(new SymmetricSecurityKey(
            Convert.FromBase64String("DRjd/GnduI3Efzen9V9BvbNUfc/VKgXltV7Kbk9sMkY=")));

          options.AddDevelopmentSigningCertificate();

          options.UseAspNetCore()
            .EnableLogoutEndpointPassthrough()
            .EnableStatusCodePagesIntegration()
            .EnableUserinfoEndpointPassthrough()
            .EnableAuthorizationEndpointPassthrough();
        })
        .AddValidation(options =>
        {
          options.UseLocalServer();
          options.UseAspNetCore();
        });

      services.AddControllersWithViews();

      services.AddHostedService<Worker>();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
      app.UseStatusCodePagesWithReExecute("/error");

      app.UseRouting();

      app.UseAuthentication();
      app.UseAuthorization();

      app.UseEndpoints(options =>
      {
        options.MapControllers();
        options.MapDefaultControllerRoute();
      });
    }
  }
}