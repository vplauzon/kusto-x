using Azure.Core;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Builder;
using System.Buffers;
using System.Text;
using Azure.Identity;
using Kustox.Compiler;
using Kustox.Runtime;
using Kustox.KustoState;

namespace Kustox.Workbench
{
    public static class Program
    {
        public static string Version
        {
            get
            {
                var assembly = typeof(Program).Assembly;
                var version = assembly.GetName().Version;

                return version!.ToString();
            }
        }
        public static string ApiUrl => "api/command";

        public static async Task Main(params string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            //  Add services to the container
            builder.Services.AddRazorPages();
            builder.Services.AddControllers();
            builder.Services.AddScoped<UserIdentityContext>();
            builder.Services.AddSingleton(new KustoxCompiler());
            builder.Services.AddSingleton(CreateProcedureEnvironmentRuntime());

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            if (!builder.Environment.IsDevelopment())
            {
                builder.Services.AddScoped<UserIdentityMiddleware>();
            }

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                //  Swagger:  browse at /swagger
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseMiddleware<UserIdentityMiddleware>();
            }
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();
            app.MapRazorPages();
            app.MapControllers();

            await app.RunAsync();
        }

        #region Services
        private static ProcedureEnvironmentRuntime CreateProcedureEnvironmentRuntime()
        {
            var kustoCluster = Environment.GetEnvironmentVariable("kustoCluster");
            //var kustoDbSandbox = Environment.GetEnvironmentVariable("kustoDb-sandbox");
            var kustoDbState = Environment.GetEnvironmentVariable("kustoDb-state");
            var credentials = CreateTokenCredential();
            var connectionProvider =
                new ConnectionProvider(new Uri(kustoCluster!), kustoDbState!, credentials);
            var hubStore = new KustoStorageHub(connectionProvider);
            var runtime = new ProcedureEnvironmentRuntime(
                hubStore.ProcedureRunStore,
                hubStore.ProcedureRunRegistry,
                connectionProvider);

            return runtime;
        }

        private static TokenCredential CreateTokenCredential()
        {
            var tenantId = Environment.GetEnvironmentVariable("tenantId");
            var appId = Environment.GetEnvironmentVariable("appId");
            var appKey = Environment.GetEnvironmentVariable("appKey");

            if (string.IsNullOrWhiteSpace(tenantId)
                || string.IsNullOrWhiteSpace(appId)
                || string.IsNullOrWhiteSpace(appKey))
            {
                return new ManagedIdentityCredential();
            }
            else
            {
                var credential = new ClientSecretCredential(tenantId, appId, appKey);

                return credential;
            }
        }
        #endregion
    }
}