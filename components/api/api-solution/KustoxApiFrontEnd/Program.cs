using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KustoxApiFrontEnd
{
    public class Program
    {
        public static void Main(string[] args)
        {
            PatchClusterAddresses("planning", "status");

            CreateHostBuilder(args).Build().Run();
        }

        private static void PatchClusterAddresses(params string[] clusterNames)
        {
            foreach (var cluster in clusterNames)
            {
                var variableValue = Environment.GetEnvironmentVariable(cluster);

                if (!string.IsNullOrWhiteSpace(variableValue))
                {
                    Environment.SetEnvironmentVariable(
                        $"ReverseProxy__Clusters__{cluster}Cluster__Destinations__{cluster}Cluster/destination1__Address",
                        variableValue);
                }
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
