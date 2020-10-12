using AutoMapper;
using DevIO.Api.Configuration;
using DevIO.Data.Context;
using Microsoft.AspNetCore.Builder;
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
            // adiciona configuração para automapper
            // Não esquecer de instalar o automapper antes c o comando: 'install-package AutoMapper.Extensions.Microsoft.DependencyInjection'
            services.AddAutoMapper(typeof(Startup));

            // adiciona configuração de conexão ao banco de dados
            services.AddDbContext<MeuDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            // adiciona arquivo customizado responsavel por configurar as Authorizations
            services.AddIdentityConfiguration(Configuration);

            services.AddControllers();

            // adiciona arquivo responsavel pelas injeções de independencias
            services.ResolveDependencies();

            // arquivo q contem configurações do MVC
            services.WebApiConfig();

            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerConfig();

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

            // Configura autenticação
            // Obs: tem q vir antes da config do MVC
            app.UseAuthentication();

            // arquivo q contem configurações customizadas
            // este recurso deve ser o último a ser chamado
            app.UseMvcConfiguration();

            // habilita o Swagger
            app.UseSwaggerConfig(provider);

        }
    }
}
