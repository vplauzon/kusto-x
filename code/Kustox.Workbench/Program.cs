using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Builder;
using System.Buffers;
using System.Text;

namespace Kustox.Workbench
{
    public static class Program
    {
        private static readonly Lazy<Uri> _externalCommandApi = new Lazy<Uri>(() =>
        {
            var apiUrl = Environment.GetEnvironmentVariable("ApiUrl");

            if (apiUrl != null)
            {
                return new Uri(apiUrl);
            }
            else
            {
                throw new InvalidOperationException("Environment variable 'ApiUrl' not defined");
            }
        });

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
            builder.Services.AddScoped<UserIdentityContext>();

            if (!builder.Environment.IsDevelopment())
            {
                builder.Services.AddScoped<UserIdentityMiddleware>();
            }

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                app.UseMiddleware<UserIdentityMiddleware>();
            }
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();
            app.MapRazorPages();
            app.MapPost("api/command", CommandApiProxyAsync);

            await app.RunAsync();
        }

        private static async Task CommandApiProxyAsync(HttpContext context)
        {
            var requestResult = await context.Request.BodyReader.ReadAsync();
            var requestBuffer = requestResult.Buffer.ToArray();
#if DEBUG
            var requestText = ASCIIEncoding.UTF8.GetString(requestBuffer);
#endif

            using var httpClient = new HttpClient();
            var response = await httpClient.PostAsync(
                _externalCommandApi.Value,
                new ByteArrayContent(requestBuffer));

            context.Response.StatusCode = (int)response.StatusCode;
            context.Response.ContentType = response.Content.Headers.ContentType?.ToString();
            
            await response.Content.CopyToAsync(context.Response.Body);
        }
    }
}