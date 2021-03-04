using System;
using System.ComponentModel;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using BookStore.Identities.Contexts;
using BookStore.Identities.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
      services.Configure<DatabaseSetting>(options => Configuration.GetSection(nameof(DatabaseSetting)).Bind(options));
      services.Configure<TokenSetting>(options => Configuration.GetSection(nameof(TokenSetting)).Bind(options));

      var databaseConfigure = Configuration.GetSection(nameof(Settings.TokenSetting));
      services.Configure<DatabaseSetting>(databaseConfigure);
      var databaseSetting = databaseConfigure.Get<DatabaseSetting>();

      var tokenConfigure = Configuration.GetSection(nameof(TokenSetting));
      services.Configure<TokenSetting>(tokenConfigure);
      var tokenSetting = databaseConfigure.Get<TokenSetting>();

      services.AddDbContext<ApplicationDbContext>(options =>
      {
        options.UseSqlServer(databaseSetting.ConnectionString);
        options.UseOpenIddict<Guid>();
      });

      services.Configure<IdentityOptions>(options =>
      {
        options.ClaimsIdentity.UserNameClaimType = OpenIddictConstants.Claims.Name;
        options.ClaimsIdentity.UserIdClaimType = OpenIddictConstants.Claims.Subject;
        options.ClaimsIdentity.RoleClaimType = OpenIddictConstants.Claims.Role;
      });

      services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
        {
          options.Password.RequiredLength = 8;
          options.Password.RequireNonAlphanumeric = false;
          options.Password.RequireUppercase = true;
          options.Password.RequireDigit = true;
          options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromDays(1);
          options.Lockout.MaxFailedAccessAttempts = 5;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

      services.AddOpenIddict()
        .AddCore(options =>
        {
          options.UseEntityFrameworkCore()
            .UseDbContext<ApplicationDbContext>();
        }).AddServer(options =>
        {
          options.SetTokenEndpointUris("/api/authorization/signin");
          options.RegisterScopes(OpenIddictConstants.Scopes.Email, OpenIddictConstants.Scopes.Profile, OpenIddictConstants.Scopes.Roles);
          options.AllowPasswordFlow();
          options.AcceptAnonymousClients();

          var secretKey = Encoding.UTF8.GetBytes(tokenSetting.Secret);
          var securityKey = new SymmetricSecurityKey(secretKey);
          var signInCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha512);
          var signCertificate = new X509Certificate2(secretKey);
          var signCredentials = new X509SigningCredentials(signCertificate);

          options.AddSigningCertificate(signCertificate);
          options.AddEncryptionKey(securityKey);

          options.UseAspNetCore()
            .EnableTokenEndpointPassthrough();

        }).AddValidation(options =>
        {
          options.UseLocalServer();
          options.UseAspNetCore();
        });

      services.AddCors();
      services.AddControllers();
      services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new OpenApiInfo {Title = "BookStore.Identites", Version = "v1"}); });
      services.AddHostedService<Worker>();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
      if (env.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
      }

      app.UseSwagger();
      app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "BookStore.Identites v1"));

      app.UseRouting();

      app.UseCors(options => { options.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod(); });

      app.UseAuthentication();
      app.UseAuthorization();

      app.UseEndpoints(endpoints =>
      {
        endpoints.MapControllers();
        endpoints.MapDefaultControllerRoute();
      });
    }
  }
}