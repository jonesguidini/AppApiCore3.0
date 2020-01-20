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
            services.AddAutoMapper(typeof(Startup));

            // adiciona configuração de conexão ao banco de dados
            services.AddDbContext<MeuDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("Default")));

            services.AddControllers();

            // adiciona arquivo responsavel pelas injeções de independencias
            services.ResolveDependencies();

            // adiciona compatibilidade da versão
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

            // configura o api para retornar apenas mensagens de validações customizadas
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });


            services.AddMvc(option => option.EnableEndpointRouting = false)
              .SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
              .AddNewtonsoftJson(opt => {
                  opt.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                  opt.SerializerSettings.ContractResolver = new DefaultContractResolver();
              });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

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
