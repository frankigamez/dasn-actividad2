using System;
using DASN.PortableWebApp.Models;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DASN.PortableWebApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = BuildWebHost(args);

            try
            {
                using (var scope = host.Services.CreateScope())
                using (var context = scope.ServiceProvider.GetRequiredService<DASNDbContext>())
                    context.Database.Migrate();
            }
            catch (Exception ex)
            {
            }
        
        
            host.Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseUrls("http://0.0.0.0:8080")
                .Build();          
    }
}