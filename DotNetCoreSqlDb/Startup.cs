using DotNetCoreSqlDb.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using TodoItem;

namespace DotNetCoreSqlDb
{
    public class Startup
    {
        /// <summary>
        /// Creates a Cosmos DB database and a container with the specified partition key. 
        /// </summary>
        /// <returns></returns>
        private static async Task<CosmosDbService> InitializeCosmosClientInstanceAsync(string cosmosDBConnectionString, string cosmosDBSQLName, string cosmosDBContainerName)
        {
            Microsoft.Azure.Cosmos.CosmosClient client = new Microsoft.Azure.Cosmos.CosmosClient(cosmosDBConnectionString);
            CosmosDbService cosmosDbService = new CosmosDbService(client, cosmosDBSQLName, cosmosDBContainerName);
            Microsoft.Azure.Cosmos.DatabaseResponse database = await client.CreateDatabaseIfNotExistsAsync(cosmosDBContainerName);
            await database.Database.CreateContainerIfNotExistsAsync(cosmosDBContainerName, "/id");
            return cosmosDbService;
        }

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
            services.AddDbContext<MyDatabaseContext>(options => options.UseSqlServer(Configuration["SQL-CONNECTION-STRING"],
                sqlServerOptionsAction: sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(10, TimeSpan.FromSeconds(30), null);
                }));
            services.AddSingleton<ICosmosDbService>(InitializeCosmosClientInstanceAsync(Configuration["COSMOSDB-CONNECTION-STRING"], Configuration["COSMOSDB-SQL-NAME"], Configuration["COSMOSDB-CONTAINER-NAME"]).Result);
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseDeveloperExceptionPage();


            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetRequiredService<MyDatabaseContext>();
                context.Database.Migrate();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Todos}/{action=Index}/{id?}");
                endpoints.MapControllerRoute(
                    name: "Item",
                    pattern: "{controller=Item}/{action=Index}/{id?}");
            });
        }
    }
}
