using Conduit.Infrastructure.Security;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Conduit.Infrastructure
{
    public static class WebApiDependencyInjection
    {
        public static void AddWebApiInfrastructure(this IServiceCollection services)
        {
            services.AddControllers();
            services.AddCors();
            services.AddMvc()
            .AddFluentValidation(cfg =>
            {
                cfg.RegisterValidatorsFromAssemblyContaining<Startup>();
            });
        }
    }
}
