using AutoMapper;
using DevIO.Api.Configuration;
using DevIO.Api.Extensions;
using DevIO.Data.Context;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System;

namespace DevIO.Api
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
            // adiciona configura��o para automapper
            // N�o esquecer de instalar o automapper antes c o comando: 'install-package AutoMapper.Extensions.Microsoft.DependencyInjection'
            services.AddAutoMapper(typeof(Startup));

            // adiciona configura��o de conex�o ao banco de dados
            services.AddDbContext<MeuDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            // adiciona arquivo customizado responsavel por configurar as Authorizations
            services.AddIdentityConfiguration(Configuration);

            services.AddControllers();

            // arquivo q contem configura��es do MVC
            services.WebApiConfig();

            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerConfig();

            // configura��o de Logger do Elmah.io
            services.AddLogginConfiguration();

            // Adiciona Helth Checks (o pachage j� vem instalado no asp net core)
            // Ref p uso de v�rias helth checkers: https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks
            services.AddHealthChecks()
                .AddSqlServer(Configuration.GetConnectionString("DefaultConnection"), name: "Banco SQL"); // add config check helt sql

            // Usar recurso de Interface UI de HelthChecks
            // precisa instalar o pacote 'aspnetcore.healthchecks.ui' (escrito minusculo assim mesmo)
            //services.AddHealthChecksUI();

            // ref tutorial : https://blog.zhaytam.com/2020/04/30/health-checks-aspnetcore/
            services.AddHealthChecksUI(s =>
            {
                s.AddHealthCheckEndpoint("URL Endpoint", "https://localhost:5001/health");
            })
            .AddInMemoryStorage();

            // adiciona arquivo responsavel pelas inje��es de independencias
            services.ResolveDependencies();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IApiVersionDescriptionProvider provider)
        {
            if (env.IsDevelopment())
            {
                app.UseCors("Desenvolvimento");
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseCors("Production");
                app.UseHsts();
            }

            // Configura autentica��o
            // Obs: tem q vir antes da config do MVC
            app.UseAuthentication();

            // Usa o Middleware criado com o prop�sito de capturar as exceptions e lan�ar no Elmah
            // * SEMPRE tem q ser chamado antes do 'UseMvc' (exemplo abaixo)
            app.UseMiddleware<ExceptionMiddleware>();

            // arquivo q contem configura��es customizadas
            // este recurso deve ser o �ltimo a ser chamado
            app.UseMvcConfiguration();

            // habilita o Swagger
            app.UseSwaggerConfig(provider);

            // Usa o loggin configuration (elmah.io)
            app.UseLogginConfiguration();

            // habilita e adiciona o caminho para ver se a aplica��o esta helph ou n�o
            app.UseHealthChecks("/api/hc");

            // habilita e adiciona o caminho para vers�o com interface dos HealthCheckers
            //app.UseHealthChecksUI(options =>
            //{
            //    options.UIPath = "/api/hc-ui";
            //});

            // ref tutorial : https://blog.zhaytam.com/2020/04/30/health-checks-aspnetcore/
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecksUI();

                endpoints.MapHealthChecks("/health", new HealthCheckOptions()
                {
                    Predicate = _ => true,
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });
            });

        }
    }
}
