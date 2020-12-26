using GrainInterfaces;
using Grains.Hello;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using System;
using System.Threading.Tasks;

namespace SiloHost
{
    public class SiloServer
    {
        const string invariant = "Npgsql";
        const string connStr = "Host=localhost;Port=5432;Database=orleans_basic;Username=postgres;Password=mainmain;Application Name=orleans_basic_web_client";

        
        const int defaultSiloPort = 11111;
        const int defaultGatewayPort = 30000;

        private int _siloPort;
        private int _gatewayPort;

        public async Task<int> Start(int? siloPort, int? gatewayPort)
        {
            _siloPort = siloPort ?? defaultSiloPort;
            _gatewayPort = gatewayPort ?? defaultGatewayPort;
            try
            {
                var host = await StartSilo();
                Console.WriteLine("\n\n Press Enter to terminate...\n\n");
                Console.ReadLine();
                await host.StopAsync();
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return 1;
            }
        }

        private async Task<ISiloHost> StartSilo()
        {
            var builder = new SiloHostBuilder()
                .Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = Constants.ClusterId;
                    options.ServiceId = Constants.ServiceId;
                })
                .UseAdoNetClustering(options =>
                {
                    options.Invariant = invariant;
                    options.ConnectionString = connStr;
                })
                .AddAdoNetGrainStorage(Constants.GrainStorage, options =>
                {
                    options.Invariant = invariant;
                    options.ConnectionString = connStr;
                    options.UseJsonFormat = true;
                })
                .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(HelloGrain).Assembly).WithReferences())
                .ConfigureEndpoints(siloPort: _siloPort, gatewayPort: _gatewayPort, listenOnAnyHostAddress: true)
                .ConfigureLogging(logging => logging.AddConsole());

            var host = builder.Build();
            await host.StartAsync();
            return host;
        }
    }
}
