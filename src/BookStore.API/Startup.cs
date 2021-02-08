using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Validation.AspNetCore;

namespace BookStore.API
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
      services.Configure<IdentitySetting>(options => Configuration.GetSection(nameof(IdentitySetting)).Bind(options));

      services.AddAuthentication(options =>
      {
        options.DefaultScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
      });

      var identitySetting = new IdentitySetting();
      Configuration.GetSection(nameof(IdentitySetting)).Bind(identitySetting);

      services.AddOpenIddict()
        .AddValidation(options =>
        {
          options
            .SetIssuer(identitySetting.EndpointUrl)
            .AddAudiences(identitySetting.Audiences);

          options.UseIntrospection()
            .SetClientId(identitySetting.ClientId)
            .SetClientSecret(identitySetting.ClientSecret);

          options.AddEncryptionKey(new SymmetricSecurityKey(
            Convert.FromBase64String("DRjd/GnduI3Efzen9V9BvbNUfc/VKgXltV7Kbk9sMkY=")));

          options.UseSystemNetHttp();
          options.UseAspNetCore();
        });

      services.AddControllersWithViews();

      services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new OpenApiInfo {Title = "BookStore.API", Version = "v1"}); });
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
      if (env.IsDevelopment())
      {
       // app.UseDeveloperExceptionPage();
        
      }

      app.UseHttpsRedirection();

      app.UseRouting();
      app.UseSwagger();
      app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "BookStore.API v1"));

      app.UseAuthentication();
      app.UseAuthorization();

      app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
    }
  }
}