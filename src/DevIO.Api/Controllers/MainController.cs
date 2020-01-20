using System;
using System.Linq;
using DevIO.Business.Intefaces;
using DevIO.Business.Notificacoes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace DevIO.Api.Controllers
{
    [ApiController]
    public abstract class MainController : ControllerBase
    {
        private readonly INotificador notificador;
        public MainController(INotificador _notificador)
        {
            notificador = _notificador;
        }

        protected bool OperacaoValida()
        {
            return !notificador.TemNotificacao();
        }

        protected ActionResult CustomResponse(Object result = null)
        {
            if (OperacaoValida())
            {
                return Ok(new { 
                    success = true,
                    data = result
                });
            }

            return BadRequest(new
            {
                success = false,
                errors = notificador.ObterNotificacoes().Select(n => n.Mensagem)
            });
        }

        // validação de notificação de erro
        protected ActionResult CustomResponse(ModelStateDictionary modelState)
        {
            if (!modelState.IsValid) NotificarErrorModelInvalida(modelState);

            return CustomResponse();
        }

        protected void NotificarErrorModelInvalida(ModelStateDictionary modelState)
        {
            var errors = modelState.Values.SelectMany(e => e.Errors);

            foreach(var error in errors)
            {
                var errorMessage = error.Exception == null ? error.ErrorMessage : error.Exception.Message;
                NotificarError(errorMessage);
            }
        }

        protected void NotificarError(string errorMessage)
        {
            notificador.Handle(new Notificacao(errorMessage));
        }

        // validação de modelstate

        // validação de operação de negócios
    }
}