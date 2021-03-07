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
using Orleans.Configuration;

namespace Conduit
{
    public class Program
    {
        static string pgConn = null;
        public static void Main(string[] args)
        {
            var root = Directory.GetCurrentDirectory();
            var dotenv = Path.Combine(root, ".env");
            Env.Load(dotenv);
            Func<string, string> getEnv = x => Environment.GetEnvironmentVariable(x);
            pgConn =
                $"Host={getEnv("PG_HOST")};" +
                $"Port={getEnv("PG_PORT")};" +
                $"Database={getEnv("PG_DB")};" +
                $"Username={getEnv("PG_USER")};" +
                $"Password={getEnv("PG_PASS")};" +
                $"Application Name={getEnv("PG_APPNAME")}";
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseOrleans((ctx, siloBuilder) =>
                {
                    siloBuilder.Configure((Action<ClusterOptions>)(option =>
                    {
                        option.ClusterId = "dev";
                        option.ServiceId = "Conduitorleans";
                    }));

                    siloBuilder.UseAdoNetClustering(option =>
                    {
                        option.Invariant = "Npgsql";
                        option.ConnectionString = pgConn;
                    });
                    siloBuilder.AddAdoNetGrainStorage("PgStore", options =>
                    {
                        options.Invariant = "Npgsql";
                        options.ConnectionString = pgConn;
                        options.UseJsonFormat = true;
                    });
                    siloBuilder.ConfigureEndpoints(siloPort: 11111, gatewayPort: 30000, listenOnAnyHostAddress: true);


                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
