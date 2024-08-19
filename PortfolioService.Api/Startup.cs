using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using PortfolioService.Core.Helper;
using PortfolioService.Core.Interfaces;
using System;
using CodeTest.Infrastructure.Persistence;
using StockService;
using Mongo2Go;
using MongoDB.Driver;
using PortfolioService.Infrastructure.Persistence;
using PortfolioService.Core.Dtos;
using Microsoft.Extensions.Options;
using PortfolioService.Api.Handlers;

namespace CodeTest
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }


        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<MongoDbRunner>(_ =>
            {
                var runner = MongoDbRunner.Start();
                runner.Import("portfolioServiceDb", "Portfolios", @"..\..\..\..\scripts\portfolios.json", true);
                return runner;
            });

            services.AddScoped<IMongoClient>(provider =>
            {
                var runner = provider.GetRequiredService<MongoDbRunner>();
                return new MongoClient(runner.ConnectionString);
            });

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "CodeTest", Version = "v1" });
            });

            services.AddMemoryCache();
            services.AddScoped<IDataService,DataService>();
            services.AddScoped<IStockService, StockService.StockService>();
            services.AddScoped<ICurrencyLayerService, CurrencyLayerService>();
            services.AddScoped<IPortfolioService, PortfolioService.Core.Services.PortfolioService>();

            
            services.Configure<CurrencyLayerOptions>(Configuration.GetSection("CurrencyLayerSettings"));

            services.AddHttpClient<ICurrencyLayerService, CurrencyLayerService>((serviceProvider, client) =>
            {
                
                var options = serviceProvider.GetRequiredService<IOptions<CurrencyLayerOptions>>()?.Value;

                if (options == null)
                {
                    throw new ArgumentNullException(nameof(options));
                }

                client.BaseAddress = new Uri(options?.BaseUrl);
                client.DefaultRequestHeaders.Add("Accept", "application/json");

            });

            
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "CodeTest v1"));
            }

            app.UseHttpsRedirection();


            app.UseMiddleware<ExceptionHandler>();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}