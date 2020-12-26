using GrainInterfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using System;

namespace Conduit.Services
{
    public static class OrleansDependencyInjection
    {
        const string invariant = "Npgsql";
        const string connStr = "Host=localhost;Port=5432;Database=orleans_basic;Username=postgres;Password=mainmain;Application Name=orleans_basic_web_client";
        
        public static void AddOrleansClusterClient(this IServiceCollection service) 
        {
            var client = ConnectClient();
            service.AddSingleton(client);
        }

        private static IClusterClient ConnectClient()
        {
            IClusterClient client;
            client = new ClientBuilder()
                .UseAdoNetClustering(options =>
                {
                    options.Invariant = invariant;
                    options.ConnectionString = connStr;
                })
                .Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = Constants.ClusterId;
                    options.ServiceId = Constants.ServiceId;
                })
                .ConfigureLogging(logging => logging.AddConsole())
                .Build();

            client.Connect().Wait();
            Console.WriteLine("Client successfully connected to silo host \n");
            return client;
        }
    }
}
