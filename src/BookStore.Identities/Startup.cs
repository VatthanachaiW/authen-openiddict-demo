using System;
using BookStore.Identities.Contexts;
using BookStore.Identities.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
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
    public void ConfigureServices(IServiceCollection services)
    {
      services.AddDbContext<ApplicationDbContext>(options =>
      {
        options.UseSqlServer(Configuration.GetValue<string>("DefaultDbConnection"));
        options.UseOpenIddict<Guid>();
      });

      services.AddIdentity<ApplicationUser, ApplicationRole>()
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

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
          options.SetTokenEndpointUris("api/connect/token");

          //   options.RegisterScopes(OpenIddictConstants.Scopes.Email, OpenIddictConstants.Scopes.Profile, OpenIddictConstants.Scopes.Roles);

          options.AllowPasswordFlow();
          options.AcceptAnonymousClients();

          options.AddEncryptionKey(new SymmetricSecurityKey(
            Convert.FromBase64String("DRjd/GnduI3Efzen9V9BvbNUfc/VKgXltV7Kbk9sMkY=")));


          options.AddDevelopmentEncryptionCertificate()
            .AddDevelopmentSigningCertificate();

          options.UseAspNetCore()
            .EnableTokenEndpointPassthrough();
        })
        .AddValidation(options =>
        {
          options.UseLocalServer();
          options.UseAspNetCore();
        });

      services.AddControllers();
      services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new OpenApiInfo {Title = "BookStore.Identities", Version = "v1"}); });
      services.AddHostedService<Worker>();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
      app.UseDeveloperExceptionPage();
      app.UseSwagger();
      app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "BookStore.Identities v1"));


      app.UseRouting();

      app.UseAuthentication();
      app.UseAuthorization();

      app.UseEndpoints(options =>
      {
        options.MapControllers();
        options.MapDefaultControllerRoute();
      });

      app.UseWelcomePage();
    }
  }
}