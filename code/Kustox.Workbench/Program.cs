using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;

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

        public static void Main(params string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorPages();

            //  Register UserIdentityContext as a transient service
            //  Transient so each request can have a different one
            builder.Services.AddTransient<UserIdentityContext>();

            // Register the UserIdentityMiddleware using IMiddleware interface
            builder.Services.AddScoped<IMiddleware, UserIdentityCapture>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
            }
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();
            app.MapRazorPages();

            app.Run();
        }
    }
}