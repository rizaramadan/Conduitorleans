using System.Threading.Tasks;

namespace SiloHost
{
    public static class Program
    {
        static async Task<int> Main(string[] args)
        {
            int? siloPort = null;
            if (args.Length > 0 && int.TryParse(args[0], out var siloPortArg))
                siloPort = siloPortArg;

            int? gatewayPort = null;
            if (args.Length > 1 && int.TryParse(args[1], out var gatewayPortArg))
                siloPort = gatewayPortArg;

            var server = new SiloServer();
            return await server.Start(siloPort, gatewayPort);
        }
    }
}
