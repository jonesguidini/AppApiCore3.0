using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using DevIO.Api.Configuration;
using DevIO.Data.Context;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Microsoft.Extensions.Logging;

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
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseCors("Desenvolvimento");
                app.UseDeveloperExceptionPage();
            }else
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
        }
    }
}
