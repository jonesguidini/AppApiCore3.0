using AutoMapper;
using Dev.IO.API.Extensions;
using Dev.IO.API.ViewModels;
using DevIO.Business.Intefaces;
using DevIO.Business.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Dev.IO.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class ProdutosController : MainController
    {
        private readonly IProdutoRepository produtoRepository;
        private readonly IProdutoService produtoService;
        private readonly IMapper mapper;

        public ProdutosController(INotificador notificador, IProdutoRepository produtoRepository, IProdutoService produtoService, IMapper mapper, IUser user) 
            : base(notificador, user)
        {
            this.produtoRepository = produtoRepository;
            this.produtoService = produtoService;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<ProdutosViewModel>> ObterTodos() =>
            Ok(mapper.Map<IEnumerable<ProdutosViewModel>>(await produtoRepository.ObterProdutosFornecedores()));

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ProdutosViewModel>> ObterPorId([FromRoute] Guid id)
        {
            var produtoViewModel = await ObterProduto(id);
            if (produtoViewModel == null) return NotFound();

            return Ok(produtoViewModel);
        }

        [HttpPost]
        [ClaimsAuthorize("Produtos", "Adicionar")]
        public async Task<ActionResult<ProdutosViewModel>> Adicionar(ProdutosViewModel produtosViewModel)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            var imagemNome = Guid.NewGuid() + "_" + produtosViewModel.Imagem;

            if (!UploadArquivo(produtosViewModel.ImagemUpload, imagemNome))
                return CustomResponse(produtosViewModel);

            produtosViewModel.Imagem = imagemNome;

            await produtoService.Adicionar(mapper.Map<Produto>(produtosViewModel));
            return CustomResponse(produtosViewModel);
        }

        [ClaimsAuthorize("Produtos", "Atualizar")]
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Atualizar(Guid id, ProdutosViewModel produtoViewModel)
        {
            if (id != produtoViewModel.Id)
            {
                NotificarErro("Os ids informados não são iguais!");
                return CustomResponse();
            }

            var produtoAtualizacao = await ObterProduto(id);
            produtoViewModel.Imagem = produtoAtualizacao.Imagem;
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            if (produtoViewModel.ImagemUpload != null)
            {
                var imagemNome = Guid.NewGuid() + "_" + produtoViewModel.Imagem;
                if (!UploadArquivo(produtoViewModel.ImagemUpload, imagemNome))
                {
                    return CustomResponse(ModelState);
                }

                produtoAtualizacao.Imagem = imagemNome;
            }

            produtoAtualizacao.Nome = produtoViewModel.Nome;
            produtoAtualizacao.Descricao = produtoViewModel.Descricao;
            produtoAtualizacao.Valor = produtoViewModel.Valor;
            produtoAtualizacao.Ativo = produtoViewModel.Ativo;

            await produtoService.Atualizar(mapper.Map<Produto>(produtoAtualizacao));

            return CustomResponse(produtoViewModel);
        }

        [ClaimsAuthorize("Produtos", "Adicionar")]
        [HttpPost("Adicionar")]
        public async Task<ActionResult<ProdutosViewModel>> AdicionarAlternativo(ProdutosImagemViewModel produtosViewModel)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            var imgPrefix = Guid.NewGuid() + "_";

            if (!await UploadArquivoAlternativo(produtosViewModel.ImagemUpload, imgPrefix))
                return CustomResponse(produtosViewModel);

            produtosViewModel.Imagem = imgPrefix + produtosViewModel.ImagemUpload;

            await produtoService.Adicionar(mapper.Map<Produto>(produtosViewModel));
            return CustomResponse(produtosViewModel);
        }

        [ClaimsAuthorize("Produtos", "Remover")]
        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<ProdutosViewModel>> Excluir([FromRoute] Guid id)
        {
            var produtoViewModel = await ObterProduto(id);
            if (produtoViewModel == null) return NotFound();

            await produtoService.Remover(id);
            return CustomResponse(produtoViewModel);
        }

        private async Task<ProdutosViewModel> ObterProduto(Guid id) =>
            mapper.Map<ProdutosViewModel>(await produtoRepository.ObterPorId(id));

        private bool UploadArquivo(string arquivo, string imgNome)
        {
            var imageDataByteArray = Convert.FromBase64String(arquivo);

            if(string.IsNullOrEmpty(arquivo))
            {
                //Outra possibilidade
                //ModelState.AddModelError(string.Empty, "Forneça uma imagem para este produto");
                NotificarErro("Forneça uma imagem para este produto");
                return false;
            }

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/imagens", imgNome);

            if (System.IO.File.Exists(filePath))
            {
                //ModelState.AddModelError(string.Empty, "Já existe um arquivo com este nome");
                NotificarErro("Já existe um arquivo com este nome");
                return false;
            }

            System.IO.File.WriteAllBytes(filePath, imageDataByteArray);
            return true;
        }

        private async Task<bool> UploadArquivoAlternativo(IFormFile arquivo, string imgPrefixo)
        {
            if (arquivo == null || arquivo.Length == 0)
            {
                NotificarErro("Forneça uma imagem para este produto!");
                return false;
            }

            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/app/demo-webapi/src/assets", imgPrefixo + arquivo.FileName);

            if (System.IO.File.Exists(path))
            {
                NotificarErro("Já existe um arquivo com este nome!");
                return false;
            }

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await arquivo.CopyToAsync(stream);
            }

            return true;
        }
    }
}
