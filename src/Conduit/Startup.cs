

namespace Conduit
{
    using Conduit.Infrastructure;
    using Grains.Users;
    using MediatR;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.OpenApi.Models;
    using Orleans;
    using Orleans.Configuration;
    using Orleans.Hosting;
    using System;
    using System.Reflection;
    using HostBuilderCtx = Microsoft.Extensions.Hosting.HostBuilderContext;

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
            services.AddMediatR(Assembly.GetExecutingAssembly());
            //services.AddOrleansClusterClient();
            services.AddJwt();
            services.AddWebApiInfrastructure();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Conduitorleans", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Conduitorleans v1"));
            }

            app.UseCors(builder =>
                builder
                    .AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod());

            app.UseAuthentication();
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        public static Action<HostBuilderCtx, ISiloBuilder> ConfigureOrleans(IConfiguration config)
        {
            var oconf = config.GetSection(nameof(Orleans));
            var pgConn = config.GetConnectionString("pg");
            return (ctx, siloBuilder) =>
            {
                siloBuilder.Configure((Action<ClusterOptions>)(o =>
                {
                    o.ClusterId = oconf.GetValue<string>(nameof(o.ClusterId));
                    o.ServiceId = oconf.GetValue<string>(nameof(o.ServiceId));
                }));

                siloBuilder.UseAdoNetClustering(o =>
                {
                    o.Invariant = oconf.GetValue<string>(nameof(o.Invariant));
                    o.ConnectionString = pgConn;
                });
                siloBuilder.AddAdoNetGrainStorage(oconf.GetValue<string>("AddAdoNetGrainStorage"), o =>
                {
                    o.Invariant = oconf.GetValue<string>(nameof(o.Invariant));
                    o.ConnectionString = pgConn;
                    o.UseJsonFormat = true;
                });
                siloBuilder.ConfigureApplicationParts
                (
                    parts => parts.AddApplicationPart(typeof(UserGrain).Assembly).WithReferences()
                );
                siloBuilder.ConfigureEndpoints
                (
                    siloPort: oconf.GetValue<int>("SiloPort"),
                    gatewayPort: oconf.GetValue<int>("GatewayPort"),
                    listenOnAnyHostAddress: true
                );
            };
        }
    }
}
