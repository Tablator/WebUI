namespace Tablator.Presentation.Web.UI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Configuration;
    using Tablator.Presentation.Web.UI.Models.Configuration;
    using Serilog;
    using Tablator.BusinessLogic.Services;
    using DataAccess.Repositories;

    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();

            Log.Logger = new LoggerConfiguration()
           .MinimumLevel.Debug().WriteTo.File(System.IO.Path.Combine(Configuration["Logging:FilePath"], $"log_{DateTime.Now.Year}-{DateTime.Now.Month}-{DateTime.Now.Day}.log"))
           .CreateLogger();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            services.Configure<CatalogSettings>(options => Configuration.GetSection("Catalog").Bind(options));

            services.AddScoped<ICatalogRepository, CatalogRepository>(repo => new CatalogRepository(Configuration.GetSection("Catalog")["RootDirectory"]));
            services.AddScoped<ITablatureRepository, TablatureRepository>(repo => new TablatureRepository(Configuration.GetSection("Catalog")["RootDirectory"]));

            //services.AddScoped<IStorageFileService, StorageFileService>(serv => new StorageFileService(Configuration.GetSection("Catalog")["RootDirectory"]));
            services.AddScoped<ICatalogService, CatalogService>();
            services.AddScoped<ITablatureService, TablatureService>();

            //services.AddScoped<IGuitarChordRenderingBuilderService, GuitarChordRenderingBuilderService>();
            //services.AddScoped<IGuitarTablatureRenderingBuilderService, GuitarTablatureRenderingBuilderService>();
            //services.AddScoped<ITablatureRenderingBuilderService, TablatureRenderingBuilderService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddSerilog();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //app.Run(async (context) =>
            //{
            //    await context.Response.WriteAsync("Hello World!");
            //});

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                  name: "Tab",
                  template: "tab/{*urlPath}",
                  defaults: new { controller = "Tab", action = "Get" });

                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}