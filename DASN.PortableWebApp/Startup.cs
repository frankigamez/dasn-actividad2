using System;
using System.IO;
using System.Reflection;
using DASN.PortableWebApp.Models;
using DASN.PortableWebApp.Models.DataModels;
using DASN.PortableWebApp.Services;
using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using log4net.ElasticSearch;
using log4net.Layout;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace DASN.PortableWebApp
{
    public class Startup
    {
        private readonly ILog _log;

        public Startup(IHostingEnvironment env)
        {
            var privatesettings = "appsettings.private.json";
            if (!new FileInfo(privatesettings).Exists) privatesettings = "appsettings.fake.json";

            Configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile(privatesettings)
                .AddEnvironmentVariables()
                .Build();

            ConfigureLogger();
            
            _log = LogManager.GetLogger(typeof(Startup));
            _log.Debug("DASN Start!");            
        }         

        private IConfiguration Configuration { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) => _log.TraceVoid(() =>
        {
            //Database:
            //----------------------
            services.AddDbContext<DASNDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DASNDB")));
            //----------------------


            //Identity:
            //----------------------
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<DASNDbContext>()
                .AddDefaultTokenProviders();

            services.Configure<IdentityOptions>(options =>
            {
                // Password settings
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = false;
                options.Password.RequiredUniqueChars = 6;

                // Lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                // User settings
                options.User.RequireUniqueEmail = true;
                options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyz" +
                                                         "ABCDEFGHIJKLMNOPQRSTUVWXYZ" +
                                                         "0123456789!#$%&'*+-/=?^_`{|}~.@";
            });

            // Cookie settings
            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.HttpOnly = true;
                options.Cookie.Expiration = TimeSpan.FromDays(30);
                options.LoginPath =
                    "/Account/Login"; // If the LoginPath is not set here, ASP.NET Core will default to /Account/Login
                options.LogoutPath =
                    "/Account/Logout"; // If the LogoutPath is not set here, ASP.NET Core will default to /Account/Logout
                options.AccessDeniedPath =
                    "/Account/Login"; // If the AccessDeniedPath is not set here, ASP.NET Core will default to /Account/AccessDenied
                options.SlidingExpiration = true;
            });
            //----------------------


            //Messaging:
            //----------------------
            services.Configure<EmailSenderServiceSettings>(Configuration.GetSection("EmailSenderServiceSettings"));
            //----------------------


            //Caching:
            //----------------------
            var redisConnectionString = Configuration.GetConnectionString("Redis");
            services.AddDataProtection().PersistKeysToRedis(ConnectionMultiplexer.Connect(redisConnectionString),
                "DataProtection-Keys");
            services.AddOptions();

            services.AddDistributedRedisCache(option =>
            {
                option.Configuration = redisConnectionString;
                option.InstanceName = "dasncache";
            });
            //----------------------


            //Services:
            //----------------------
            services.AddTransient<EmailSenderService>();
            services.AddTransient<DASNoteService>();
            services.AddTransient<AntibotService>();
            services.AddTransient<DASNDbContext>();
            services.AddTransient<UserManager<ApplicationUser>>();
            services.AddTransient<SignInManager<ApplicationUser>>();
            //----------------------

            services.AddMvc();
        });
        

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env) => _log.TraceVoid(() =>
        {
            app.UseExceptionHandler("/Home/Error");

            app.UseStaticFiles();

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        });
        
        
        private void ConfigureLogger()
        {        
            var type = Configuration.GetValue<string>("Logger:Type");
            IAppender appender = null;
            
            var level = Level.All;
            var name = $"{type}Appender";
            var layout = new PatternLayout()
            {
                ConversionPattern = "%-5p %d{hh:mm:ss} %logger %message%newline"
            };
            layout.ActivateOptions();

            if (type.ToLower() == "file")
            {
                appender = new RollingFileAppender
                {
                    File = "trace/dasn.log",
                    Layout = layout, Threshold = level, Name = name
                };
                ((RollingFileAppender)appender).ActivateOptions();
            }

            if (type.ToLower() == "elk")
            {
                appender = new ElasticSearchAppender
                {
                    ConnectionString = Configuration.GetConnectionString("ELK"),
                    Layout = layout, Threshold = level, Name = name
                };
                ((ElasticSearchAppender)appender).ActivateOptions();
            }

            if (type.ToLower() == "console")
            {
                appender = new ConsoleAppender
                {
                    Layout = layout, Threshold = level, Name = name
                };
                ((ConsoleAppender)appender).ActivateOptions();
            }

            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            BasicConfigurator.Configure(logRepository, appender);            
        }
    }
}