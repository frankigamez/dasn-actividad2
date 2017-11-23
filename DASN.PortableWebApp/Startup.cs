using System;
using System.IO;
using DASN.PortableWebApp.Models;
using DASN.PortableWebApp.Models.DataModels;
using DASN.PortableWebApp.Services;
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
        }

        private IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
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
                options.LoginPath = "/Account/Login"; // If the LoginPath is not set here, ASP.NET Core will default to /Account/Login
                options.LogoutPath = "/Account/Logout"; // If the LogoutPath is not set here, ASP.NET Core will default to /Account/Logout
                options.AccessDeniedPath = "/Account/Login"; // If the AccessDeniedPath is not set here, ASP.NET Core will default to /Account/AccessDenied
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
            services.AddDataProtection().PersistKeysToRedis(ConnectionMultiplexer.Connect(redisConnectionString), "DataProtection-Keys");
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
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
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
        }
    }
}