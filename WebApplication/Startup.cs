using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using WebApplication.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace WebApplication
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        private WebAppDBContext dbContext { get; set; }
        private Microsoft.Data.SqlClient.SqlConnection sqlConnection { get; set; }
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<WebAppDBContext>(options =>
            {
                SqlAuthenticationProvider.SetProvider(SqlAuthenticationMethod.ActiveDirectoryDeviceCodeFlow, new CustomAzureSQLAuthProvider());
                var connStr = Configuration.GetConnectionString("azuresql");
                Console.WriteLine($"********************* The connection string used is {connStr}");
                sqlConnection = new Microsoft.Data.SqlClient.SqlConnection(connStr);
                options.UseSqlServer(sqlConnection);
            //sqlServerOptionsAction: sqlOptions =>
            //{
            //    sqlOptions.EnableRetryOnFailure(
            //    maxRetryCount: 10,
            //    maxRetryDelay: TimeSpan.FromSeconds(30),
            //    errorNumbersToAdd: null);
            //});

            });
            OpenDB(services);

            services.AddRazorPages();
        }

        private void OpenDB(IServiceCollection services)
        {
            var connStr = Configuration.GetConnectionString("azuresql");
            using var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();
            dbContext = scope.ServiceProvider.GetService<WebAppDBContext>();

            try
            {
                dbContext.Database.OpenConnection();
            }
            catch (Exception ex)
            {
                var error = ex.Message;
                Console.WriteLine($"************** db coonnection error {error}");
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

           app.UseAuthorization();

            app.Run(async context =>
            {
                await context.Response.WriteAsync(System.Diagnostics.Process.GetCurrentProcess().ProcessName);
            });


        }
    }
}
