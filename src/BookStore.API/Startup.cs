using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System.Text;
using BookStore.API.Settings;
using Microsoft.IdentityModel.Tokens;

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
      services.AddCors();


      var tokenConfigure = Configuration.GetSection(nameof(TokenSetting));
      services.Configure<TokenSetting>(tokenConfigure);
      var tokenSetting = tokenConfigure.Get<TokenSetting>();

      services.AddOpenIddict()
        .AddValidation(options =>
        {
          options.SetIssuer("https://localhost:5000/");
          options.AddAudiences("book_api_resource");

          var secretKey = Encoding.UTF8.GetBytes(tokenSetting.Secret);
          var securityKey = new SymmetricSecurityKey(secretKey);
          options.AddEncryptionKey(securityKey);

          options.UseSystemNetHttp();

          options.UseAspNetCore();
          options.UseSystemNetHttp();
        });
      services.AddControllers();
      services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new OpenApiInfo {Title = "BookStore.API", Version = "v1"}); });
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
      if (env.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "BookStore.API v1"));
      }

      app.UseHttpsRedirection();

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