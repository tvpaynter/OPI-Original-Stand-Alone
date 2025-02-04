using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System;
using System.Xml;
using UTG.Common;
using UTG.Common.Handlers;
using UTG.Data;
using UTG.Exceptions;
using UTG.Formatters;
using UTG.Interfaces;
using UTG.Services;
using UTG.StoreAndForward;

namespace UTG
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
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder =>
                    {
                        builder.AllowAnyOrigin();
                        builder.AllowAnyHeader();
                        builder.AllowAnyMethod();
                    });
            });
            services.AddTransient<HostService>();
            services.AddTransient<TerminalService>();
            services.AddSingleton<HttpPostHandler>();
            services.AddScoped<TcpIpHandler>();
            services.AddTransient<Func<string, string, string,IUTGService>>(serviceProvider => (TransToken, Pan, TransType) =>
            {
                if (Utils.IsProcessWithTerminal(TransToken, Pan, TransType))
                {
                    return serviceProvider.GetService<TerminalService>();
                }
                else
                {
                    return serviceProvider.GetService<HostService>();
                }
            });
            services.AddSingleton(new Config { ConnectionString = Configuration["DatabaseConnectionString"] });
            services.AddSingleton<IDatabaseSetup, DatabaseSetup>();
            services.AddSingleton<IOfflineDBManager, OfflineDBManager>();
            services.AddSingleton<IOfflineTransactionProcessor, OfflineTransactionProcessor>();
            services.AddMvc(options =>
            {
                options.InputFormatters.Insert(0, new XMLInputFormatter());
                options.OutputFormatters.Insert(0, new XMLOutputFormatter());
            });
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "UTG", Version = "v1", Description = "UTG API" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();            
            }
            else
            {
                app.UseHsts();
            }
            serviceProvider.GetService<IDatabaseSetup>().SetupDB();
            app.UseMiddleware<GlobalErrorHandlingMiddleware>(Configuration["service"]);
            app.UseMiddleware<StoreAndForwardMiddleware>();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "UTG v1");
                c.DisplayRequestDuration();
            });

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
           
        }
    }
}
