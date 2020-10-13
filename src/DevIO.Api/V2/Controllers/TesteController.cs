using DevIO.Api.Controllers;
using DevIO.Business.Intefaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DevIO.Api.V2.Controllers
{
    [ApiVersion("2.0", Deprecated = true)]
    [Route("Api/v{version:apiVersion}/teste")]
    public class TesteController : MainController
    {
        private readonly ILogger _logger;

        public TesteController(INotificador notificador,
            IUser user,
            ILogger<TesteController> logger
        ) : base(notificador, user)
        {
            _logger = logger;
        }

        [HttpGet]
        public string Valor()
        {
            // logs mais utilizado em desenvolvimento
            _logger.LogTrace("Log de Trace");
            _logger.LogDebug("Log de Debug");

            // logs mais utilizados em produção
            _logger.LogInformation("Log de informação");
            _logger.LogWarning("Log de Aviso");
            _logger.LogError("Log de Erro");
            _logger.LogCritical("Log de Problema Crítico");

            return "Sou a V2";
        }
    }
}
