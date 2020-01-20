using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Dev.IO.API.Extensions;
using Dev.IO.API.ViewModels;
using DevIO.Business.Intefaces;
using DevIO.Business.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Dev.IO.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class FornecedoresController : MainController
    {
        private readonly IFornecedorRepository fornecedorRepository;
        private readonly IMapper mapper;
        private readonly IFornecedorService fornecedorService;
        private readonly IEnderecoRepository enderecoRepository;

        public FornecedoresController(IFornecedorRepository fornecedorRepository, IMapper mapper,
            IFornecedorService fornecedorService, INotificador notificador, IEnderecoRepository enderecoRepository, IUser user) 
            : base(notificador, user)
        {
            this.fornecedorRepository = fornecedorRepository;
            this.mapper = mapper;
            this.fornecedorService = fornecedorService;
            this.enderecoRepository = enderecoRepository;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FornecedorViewModel>>> ObterTodos()
        {
            var fornecedor = mapper.Map<IEnumerable<FornecedorViewModel>>
                (await fornecedorRepository.ObterTodos());
            return Ok(fornecedor);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<FornecedorViewModel>> ObterPorId([FromRoute] Guid id)
        {
            var fornecedor = await ObterFornecedorProdutosEndereco(id);

            if (fornecedor == null)
                return NotFound();

            return Ok(fornecedor);
        }

        [ClaimsAuthorize("Fornecedor", "Adicionar")]
        [HttpPost]
        public async Task<ActionResult<FornecedorViewModel>> Adicionar([FromBody] FornecedorViewModel fornecedorViewModel)
        {
            if (!ModelState.IsValid)
                return CustomResponse(ModelState);

            await fornecedorService.Adicionar(mapper.Map<Fornecedor>(fornecedorViewModel));

            return CustomResponse(fornecedorViewModel);
        }

        [ClaimsAuthorize("Fornecedor", "Atualizar")]
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<FornecedorViewModel>> Atualizar([FromRoute] Guid id, [FromBody] FornecedorViewModel fornecedorViewModel)
        {
            if (id != fornecedorViewModel.Id)
            {
                NotificarErro("Id informado incorreto");
                return CustomResponse(fornecedorViewModel);
            }

            if (!ModelState.IsValid)
                return CustomResponse(ModelState);

            await fornecedorService.Atualizar(mapper.Map<Fornecedor>(fornecedorViewModel));
            return CustomResponse(fornecedorViewModel);
        }

        [ClaimsAuthorize("Fornecedor", "Remover")]
        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<FornecedorViewModel>> Excluir([FromRoute] Guid id)
        {
            var fornecedorViewModel = await ObterFornecedorEndereco(id);
            if (fornecedorViewModel == null)
                return NotFound();

            await fornecedorService.Remover(id);
            return CustomResponse(fornecedorViewModel);
        }

        [HttpGet("obter-endereco/{id:guid}")]
        public async Task<ActionResult<EnderecoViewModel>> ObterEnderecoPorId([FromRoute] Guid id) =>
            mapper.Map<EnderecoViewModel>(await enderecoRepository.ObterPorId(id));

        [ClaimsAuthorize("Fornecedor", "Atualizar")]
        [HttpPut("atualizar-endereco/{id:guid}")]
        public async Task<IActionResult> AtualizarEndereco([FromRoute] Guid id, [FromBody] EnderecoViewModel enderecoViewModel)
        {
            if (id != enderecoViewModel.Id)
            {
                NotificarErro("Id informado incorreto");
                return CustomResponse(enderecoViewModel);
            }

            if (!ModelState.IsValid) return CustomResponse(ModelState);
            await fornecedorService.AtualizarEndereco(mapper.Map<Endereco>(enderecoViewModel));

            return CustomResponse(enderecoViewModel);
        }

        private async Task<FornecedorViewModel> ObterFornecedorProdutosEndereco(Guid id)
        {
            return mapper.Map<FornecedorViewModel>
                (await fornecedorRepository.ObterFornecedorProdutosEndereco(id));
        }

        private async Task<FornecedorViewModel> ObterFornecedorEndereco(Guid id)
        {
            return mapper.Map<FornecedorViewModel>
                (await fornecedorRepository.ObterFornecedorEndereco(id));
        }
    }
}