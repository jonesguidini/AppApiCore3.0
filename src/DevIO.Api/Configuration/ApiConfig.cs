using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DevIO.Api.Configuration
{
    public static class ApiConfig
    {
        public static IServiceCollection WebApiConfig(this IServiceCollection services)
        {
            // adiciona compatibilidade da versão
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

            // CONFIGURAÇÕES DE VERSIONAMENTO DA API -------
            services.AddApiVersioning(options =>
            {
                options.AssumeDefaultVersionWhenUnspecified = true; //quando não tiver versão especificada assume a versão default
                options.DefaultApiVersion = new ApiVersion(1, 0); // pode usar 3 numeros ou 1 numero dependendo do padrão de versionamento
                options.ReportApiVersions = true; // retorna informe da versão da api no header
            });

            services.AddVersionedApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV"; // 'v' + 'Major Version', 'Minor Version', 'Patch'
                options.SubstituteApiVersionInUrl = true;
            });
            //-------------------------------------------------

            // configura o api para retornar apenas mensagens de validações customizadas
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });


            services.AddMvc(option => option.EnableEndpointRouting = false)
              .SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
              .AddNewtonsoftJson(opt =>
              {
                  opt.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                  opt.SerializerSettings.ContractResolver = new DefaultContractResolver();
              });


            
            services.AddCors(options =>
            {
                // CORS de exemplo para desenvolvimento
                options.AddPolicy("Development", builder => builder
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .SetIsOriginAllowed(origin => true) // fix para ASpNet Core 3.1 , desabilitar 'AllowCredentials()' 
                    //.AllowCredentials()
                );

                // CORS de exemplo para produção
                // editar conforme necessidade
                options.AddPolicy("Production", builder => builder
                   .WithMethods("GET")
                   .WithOrigins("http://desenvolvedor.io") // somente este site consegue fazer GET na api
                   .SetIsOriginAllowedToAllowWildcardSubdomains() // permite outras aplicações sob o mesmo domínio
                    //.WithHeaders(HeaderNames.ContentType, "x-custom-header") // obriga que a requisição tenha este header para ter permissão
                    // usar outras configurações e restrições
                .AllowAnyHeader());

            });


            return services;
        }

        public static IApplicationBuilder UseMvcConfiguration(this IApplicationBuilder app)
        {
            app.UseHttpsRedirection(); // Força o redirecionamento para HTTPS 

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            //app.UseCors("Development");
            app.UseMvc();

            return app;
        }
    }
}
