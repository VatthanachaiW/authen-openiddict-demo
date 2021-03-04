using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace BookStore.Identities
{
  public class Program
  {
    public static void Main(string[] args)
    {
      CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
          .ConfigureAppConfiguration((context, config) =>
          {
            config.SetBasePath(AppDomain.CurrentDomain.BaseDirectory);
            config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            config.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true);
           // config.AddJsonFile($"swaggersettings.json", optional: false, reloadOnChange: true);
            config.AddEnvironmentVariables();
          })
            .ConfigureWebHostDefaults(builder =>
            {
              builder.UseStartup<Startup>();
            });
  }
}
