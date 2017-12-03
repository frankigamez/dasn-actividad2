using System;
using System.IO;
using DASN.PortableWebApp.Models;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Sinks.Elasticsearch;

namespace DASN.PortableWebApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var privatesettings = "appsettings.private.json";
            if (!new FileInfo(privatesettings).Exists) privatesettings = "appsettings.fake.json";
            
            //Configuration 
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile(privatesettings)
                .AddEnvironmentVariables()
                .Build();            
            
            //Logger
            var loggerConfiguration = new LoggerConfiguration();               
            var type = configuration.GetValue<string>("Logger:Type");            
            if (type.ToLower() == "file")
                loggerConfiguration.WriteTo.RollingFile(pathFormat: "trace/dasn.log");
            if (type.ToLower() == "elk")
                loggerConfiguration.WriteTo.Elasticsearch(options: new ElasticsearchSinkOptions(new Uri(configuration.GetConnectionString("ELK"))));
            loggerConfiguration.WriteTo.Console();
            Log.Logger = loggerConfiguration.CreateLogger(); 
            
            //Server
            var host = WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseUrls("http://0.0.0.0:8080")
                .UseSerilog()
                .UseConfiguration(configuration)
                .Build();    

            //Db Migration
            using (var scope = host.Services.CreateScope())
            using (var context = scope.ServiceProvider.GetRequiredService<DASNDbContext>())                
                context.Database.Migrate();
        
            //Start!
            host.Run();
        }                 
    }
}