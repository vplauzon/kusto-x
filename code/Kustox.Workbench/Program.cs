using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Builder;

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

        public static async Task Main(params string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorPages();
            builder.Services.AddScoped<UserIdentityMiddleware>();
            builder.Services.AddScoped<UserIdentityContext>();

            var app = builder.Build();

            app.UseMiddleware<UserIdentityMiddleware>();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
            }
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();
            app.MapRazorPages();

            await app.RunAsync();
        }
    }
}