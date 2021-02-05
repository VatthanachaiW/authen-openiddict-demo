using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookStore.Identites.Datas;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace BookStore.Identites
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
      services.AddControllersWithViews();

      services.AddDbContext<ApplicationDbContext>(options =>
      {
        options.UseSqlServer(Configuration.GetValue<string>("DefaultDbConnection"));
        options.UseOpenIddict();
      });

      services.Configure<IdentityOptions>(options =>
      {
        options.ClaimsIdentity.UserNameClaimType = Claims.Name;
        options.ClaimsIdentity.UserIdClaimType = Claims.Subject;
        options.ClaimsIdentity.RoleClaimType = Claims.Role;
      });


      services
        .AddOpenIddict()
        .AddCore(options =>
        {
          options.UseEntityFrameworkCore()
            .UseDbContext<ApplicationDbContext>();
        })
        .AddServer(options =>
        {
          options.SetTokenEndpointUris("/connect/token");

          options.RegisterScopes(Scopes.Email, Scopes.Profile, Scopes.Roles);

          options
            .AllowClientCredentialsFlow()
            .AllowPasswordFlow();

          var secretKey = Encoding.UTF8.GetBytes(Configuration.GetValue<string>("PasswordSecret"));
          var securityKey = new SymmetricSecurityKey(secretKey);
          var signInKey = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

          options.AddDevelopmentSigningCertificate();
          options.AddEncryptionKey(new SymmetricSecurityKey(
            Convert.FromBase64String("DRjd/GnduI3Efzen9V9BvbNUfc/VKgXltV7Kbk9sMkY=")));
          /*
           options.AddEphemeralEncryptionKey()
            .AddEphemeralSigningKey()
            .AddSigningCredentials(signInKey);
          */

          options.UseAspNetCore()
            .EnableTokenEndpointPassthrough()
            .EnableAuthorizationEndpointPassthrough()
            .EnableUserinfoEndpointPassthrough()
            .EnableStatusCodePagesIntegration();
        })
        .AddValidation(options =>
        {
          options.UseLocalServer();
          options.UseAspNetCore();
        });

      services.AddCors();
      services.AddHostedService<Worker>();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
      if (env.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
      }

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