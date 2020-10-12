using DevIO.Api.Data;
using DevIO.Api.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace DevIO.Api.Configuration
{
    public static class IdentityConfig
    {
        public static IServiceCollection AddIdentityConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options => 
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            services.AddDefaultIdentity<IdentityUser>()
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddErrorDescriber<IdentityMensagensPortugues>() // tradução dos erros do identity
                .AddDefaultTokenProviders();

            // JWT
            var appSettingsSection = configuration.GetSection("AppSettings");
            services.Configure<AppSettings>(appSettingsSection);

            var appSettings = appSettingsSection.Get<AppSettings>();
            var key = Encoding.ASCII.GetBytes(appSettings.Secret);

            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme; // autentica
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme; // valida 
            }).AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false; // deixar true caso vc esteja trabalhando SOMENTE com HTTPS
                x.SaveToken = true; // guardar ou não o Token após a autenticação
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true, // 
                    IssuerSigningKey = new SymmetricSecurityKey(key), // configura a chave em uma chave criptografada
                    ValidateIssuer = true, // configura para validar o Emissor
                    ValidateAudience = true, // configura para validar em base da URL
                    ValidAudience = appSettings.ValidoEm, // configura a URL permitida em base do valor definido no AppSettings.json
                    ValidIssuer = appSettings.Emissor // configura o Emissor em base do valor definido no AppSettings.json
                };
            });

            return services;
        }
    }
}
