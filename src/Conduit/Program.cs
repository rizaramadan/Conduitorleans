using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orleans.Hosting;
using System.IO;
using Orleans;
using Orleans.Configuration;
using Grains.Users;

namespace Conduit
{
    public class Program
    {
        static string Env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        static string Appsetting = Env == "Development" ? "Development." : "";

        public static IConfiguration Config { get; } = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile($"appsettings.{Appsetting}json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseOrleans(Startup.ConfigureOrleans(Config))
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });

    }
}
