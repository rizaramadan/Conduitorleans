using Contracts;
using Grains.Users;
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
        public const string invariant = "Npgsql";
        public const string connStr = Constants.ConnStr;

        public const int defaultSiloPort = 11111;
        public const int defaultGatewayPort = 30000;

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
                .Configure (                 (Action<ClusterOptions>)(options => SetupClusterOptions(options)))
                .UseAdoNetClustering (       options => SetupAdoNetClustering(options))
                .AddAdoNetGrainStorage (     Constants.GrainStorage, options => SetupAdoNetGrainStorage(options))
                .ConfigureApplicationParts ( parts => parts.AddApplicationPart(typeof(UserGrain).Assembly).WithReferences())
                .ConfigureEndpoints (        siloPort: _siloPort, gatewayPort: _gatewayPort, listenOnAnyHostAddress: true)
                .ConfigureLogging (          logging => logging.AddConsole());

            var host = builder.Build();
            await host.StartAsync();
            return host;
        }

        private static void SetupClusterOptions(ClusterOptions options)
        {
            options.ClusterId = Constants.ClusterId;
            options.ServiceId = Constants.ServiceId;
        }

        private static void SetupAdoNetClustering(AdoNetClusteringSiloOptions options)
        {
            options.Invariant = invariant;
            options.ConnectionString = connStr;
        }

        private static void SetupAdoNetGrainStorage(AdoNetGrainStorageOptions options)
        {
            options.Invariant = invariant;
            options.ConnectionString = connStr;
            options.UseJsonFormat = true;
        }
    }
}
