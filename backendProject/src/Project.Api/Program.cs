using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Project.Infrastructure.Persistence;

namespace Project.Api
{
    public class Program
    {
        private const string SeedArgument = "seed";

        public static async Task<int> Main(string[] args)
        {
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.GetCultureInfo("ru-RU");

            var seedRequested = args.Any(a => string.Equals(a, SeedArgument, StringComparison.OrdinalIgnoreCase));
            var hostArgs = args.Where(a => !string.Equals(a, SeedArgument, StringComparison.OrdinalIgnoreCase)).ToArray();

            var host = CreateHostBuilder(hostArgs).Build();

            if (seedRequested)
            {
                using var scope = host.Services.CreateScope();

                await scope.ServiceProvider.GetRequiredService<MongoIndexInitializer>().StartAsync(default);
                await scope.ServiceProvider.GetRequiredService<DataSeeder>().SeedAsync();
                return 0;
            }

            await host.RunAsync();
            return 0;
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>());
    }
}
