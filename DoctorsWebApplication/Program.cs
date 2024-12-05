using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Negotiate;
using DoctorsWebApplication.Data;
using Microsoft.SqlServer.Dac;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.AspNetCore.Hosting;
using System.Diagnostics;
using System.Net;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection.Extensions;
using App.QueueService;
using App.ScopedService;
using App.TimerHostedService;

namespace DoctorsWebApplication
{

    public partial class Program
    {
        public static void Main(string[] args)
        {
            //host console
            CreateHostBuilder(args).Build().Run();
        }

        #region Done this way on purpose because the EF Core uses this method at design time

        // EF Core uses this method at design time to access the DbContext
        public static IHostBuilder CreateHostBuilder(string[] args)
            => Host.CreateDefaultBuilder(args).ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseContentRoot(Directory.GetCurrentDirectory());
                    webBuilder.UseIIS();
                    webBuilder.UseStartup<StartUp>();
                    webBuilder.UseUrls();

                    

                }).ConfigureServices((context, services) =>
                {
                    //NOTE - This is an example of various services that can be used with the .NET 9.0 ASPNET web core application

                    //1. Cloud service...AZURE
                    services.AddHostedService<App.CloudService.Worker>();
                    //2. Worker Service - Local
                    services.AddHostedService<App.WorkerService.Worker>();
                    
                    //3. Queues stuff and processes it asynchrously...
                    //Queue Service. Includes a monitoring service in app configuration...
                    services.AddSingleton<MonitorLoop>();
                    services.AddHostedService<QueuedHostedService>();
                    services.AddSingleton<IBackgroundTaskQueue>(_ =>
                    {
                        if (!int.TryParse(context.Configuration["QueueCapacity"], out var queueCapacity))
                        {
                            queueCapacity = 100;
                        }

                        return new DefaultBackgroundTaskQueue(queueCapacity);
                    });

                    //4. Scoped Service...
                    services.AddHostedService<App.ScopedService.ScopedBackgroundService>();
                    services.AddScoped<IScopedProcessingService, App.ScopedService.DefaultScopedProcessingService>();

                    //5. Signal Completion Service
                    services.AddHostedService<App.SignalCompletionService.Worker>();

                    //6. Timer Hosted Service...
                    services.AddHostedService<App.TimerHostedService.TimerService>();


                });

        #endregion

    }

    public class StartUp
    {

        #region Start Constructor and Services
        public StartUp(IConfiguration configuration)
        {
            Configuration = configuration;
        }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public static IConfiguration Configuration { get; private set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

        // This method gets called by the runtime. Use this method to add services to the container.
        public static void ConfigureServices(IServiceCollection services)
        {
            var connectionString = Configuration.GetConnectionString("2023DoctorsDatabaseDefault");

            // Add DbContext to the injection container
            services.AddDbContext<DoctorsDatabase2023Context>(options =>
                        options.UseSqlServer(connectionString));

            services.Configure<IISOptions>(options =>
            {
                options.ForwardClientCertificate = false;
            });
        }

        #endregion

        #region Make sure the SQL Server database exists using DACPAC if necessary

        public static void MakeSureSQLServerDatabaseExists(bool isProduction, bool canConnect)
        {
            //Sort out database connectivity...
            var connectionString = Configuration.GetConnectionString("2023DoctorsDatabaseDefault") ?? throw new InvalidOperationException("Connection string '2023DoctorsDatabaseDefault' not found.");
            var svc = new DacServices(connectionString);
            string dacpacPath = ".\\DoctorsDatabase.dacpac";

            if (!isProduction)
            {
                //Set the option for DACPAC
                var dacExtractOptions = new DacExtractOptions
                {
                    ExtractTarget = DacExtractTarget.DacPac,
                    ExtractAllTableData = true,
                    VerifyExtraction = true
                };

                //So in Development make sure the DACPAC is up to date...
                if (canConnect)
                {
                    svc.Extract(dacpacPath, "DoctorsDatabase", "DoctorsWebApplication", new Version(1, 2, 0), ".NET 9.0 ASPNET Web Core App to store info about doctors", extractOptions: dacExtractOptions);
                }
            }

            var dacOptions = new DacDeployOptions
            {
                BackupDatabaseBeforeChanges = true,
                CreateNewDatabase = true
            };

            if (!canConnect)
            {
                svc.Deploy(
                    DacPackage.Load(dacpacPath),
                    targetDatabaseName: "aspnet-53bc9b9d-9d6a-45d4-8429-2a2761773502",
                    upgradeExisting: true,
                    options: dacOptions
                    );
            }
        }

        #endregion

        #region Configure the web application

        public static void Configure(IApplicationBuilder app, IWebHostEnvironment env, DoctorsDatabase2023Context context)
        {
            var connectionString = Configuration.GetConnectionString("2023DoctorsDatabaseDefault");

            var appBuilder = WebApplication.CreateBuilder();

            //SQL Server database needs to exist.. if not install DACPAC
            MakeSureSQLServerDatabaseExists(env.IsProduction(), context.Database.CanConnect());

            //DI for the local DbContext
            appBuilder.Services.AddDbContext<DoctorsDatabase2023Context>(options => options.UseSqlServer(connectionString));

            appBuilder.Services.Configure<IISOptions>(options =>
            {
                options.ForwardClientCertificate = false;
            });

            //Show message friendly pages on database exceptions
            appBuilder.Services.AddDatabaseDeveloperPageExceptionFilter();

            appBuilder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();

            //Windows Authentication...
            appBuilder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme).AddNegotiate();

            appBuilder.Services.AddAuthorization(options =>
            {
                // By default, all incoming requests will be authorized according to the default policy.
                options.FallbackPolicy = options.DefaultPolicy;
            });

            appBuilder.Services.AddRazorPages();

            //Server farm considerations...
            appBuilder.Services.AddDistributedMemoryCache();
            
            //Run up the web application...
            var App = appBuilder.Build();

            ////////////////////////////////////////////////////////////////////////////////////////////////////////
            //Monitor the Queue Service...
            //MonitorLoop monitorLoop = App.Services.GetRequiredService<MonitorLoop>()!;
            //monitorLoop.StartMonitorLoop();
            ////////////////////////////////////////////////////////////////////////////////////////////////////////
            ///
            if (App.Environment.IsDevelopment())
            {
                App.UseMigrationsEndPoint();
                App.UseDeveloperExceptionPage();
            }
            else
            {
                App.UseExceptionHandler("/Error");
                App.UseHsts();
            }

            App.UseHttpsRedirection();
            App.UseStaticFiles();
            App.UseRouting();
            App.UseAuthorization();
            App.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            App.Run();
        }

        #endregion

    }
}
