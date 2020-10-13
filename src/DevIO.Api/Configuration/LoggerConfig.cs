using Elmah.Io.AspNetCore;
using Elmah.Io.Extensions.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace DevIO.Api.Configuration
{
    public static class LoggerConfig
    {
        public static IServiceCollection AddLogginConfiguration(this IServiceCollection services)
        {
            // configuração base do Elmah (costuma ser necessário)
            services.AddElmahIo(o =>
            {
                o.ApiKey = "c072c3145dd341f6863f58ec895e57a6";
                o.LogId = new Guid("cee94a08-7cb3-4ed7-a6dd-f163505a77d8");
            });

            // configurar o Elmah como provider de logs do asp net core (outros logs alem das exceptions padrão usado acima)
            // tem que instalar o pacote 'Elmah.Io.Extensions.Logging'
            //services.AddLogging(builder =>
            //{
            //    builder.AddElmahIo(o =>
            //    {
            //        o.ApiKey = "c072c3145dd341f6863f58ec895e57a6";
            //        o.LogId = new Guid("cee94a08-7cb3-4ed7-a6dd-f163505a77d8");
            //    });

            //    builder.AddFilter<ElmahIoLoggerProvider>(null, LogLevel.Warning);

            //});

            return services;
        }

        public static IApplicationBuilder UseLogginConfiguration(this IApplicationBuilder app)
        {
            app.UseElmahIo();


            return app;
        }
    }
}
