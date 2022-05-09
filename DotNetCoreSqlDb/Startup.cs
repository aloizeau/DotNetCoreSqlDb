using DotNetCoreSqlDb.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetCoreSqlDb
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            // The following line enables Application Insights telemetry collection.
            services.AddApplicationInsightsTelemetry();
            services.AddDbContext<MyDatabaseContext>(options => options.UseSqlServer(Configuration["SQL-CONNECTION-STRING"]));
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseDeveloperExceptionPage();
            //app.UseEndpoints(endpoints =>
            //{
            //    endpoints.MapGet("api/keyvault", async context =>
            //    {
            //        await context.Response.WriteAsync(Configuration["KEY_VAULT_URI"]);
            //    });
            //    endpoints.MapGet("api/sql", async context =>
            //    {
            //        ConfigurationBuilder builder = new();
            //        builder.AddAzureKeyVault(new System.Uri(Configuration["KEY_VAULT_URI"]), new DefaultAzureCredential());

            //        IConfiguration config = builder.Build();
            //        await context.Response.WriteAsync(config["SQL-CONNECTION-STRING"]);
            //    });
            //    endpoints.MapGet("api/storage", async context =>
            //    {
            //        ConfigurationBuilder builder = new();
            //        builder.AddAzureKeyVault(new System.Uri(Configuration["KEY_VAULT_URI"]), new DefaultAzureCredential());

            //        IConfiguration config = builder.Build();
            //        await context.Response.WriteAsync(config["STORAGE-CONNECTION-STRING"]);
            //    });
            //    endpoints.MapControllers();
            //});


            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetRequiredService<MyDatabaseContext>();
                context.Database.Migrate();
            }

            //if (env.IsDevelopment())
            //{
            //    app.UseDeveloperExceptionPage();
            //}
            //else
            //{
            //    app.UseExceptionHandler("/Home/Error");
            //    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            //    //app.UseHsts();
            //}

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Todos}/{action=Index}/{id?}");
            });
        }
    }
}
