using Microsoft.AspNetCore.Mvc;
using SaborDeCasa.Api.DTOs;
using SaborDeCasa.Api.Services;

namespace SaborDeCasa.Api.Controllers;

[ApiController]
[Route("api/categorias")]
public class CategoriasController : ControllerBase
{
    private readonly ICategoriaService _categoriaService;

    public CategoriasController(ICategoriaService categoriaService)
    {
        _categoriaService = categoriaService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoriaResponseDto>>> ListarTodas()
    {
        var categorias = await _categoriaService.ListarTodasAsync();
        return Ok(categorias);
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<CategoriaResponseDto>> ObterPorId(long id)
    {
        var categoria = await _categoriaService.ObterPorIdAsync(id);
        return Ok(categoria);
    }

    [HttpPost]
    public async Task<ActionResult<CategoriaResponseDto>> Criar([FromBody] CategoriaRequestDto? dto)
    {
        var criada = await _categoriaService.CriarAsync(dto ?? new CategoriaRequestDto());
        return CreatedAtAction(nameof(ObterPorId), new { id = criada.Id }, criada);
    }

    [HttpPut("{id:long}")]
    public async Task<ActionResult<CategoriaResponseDto>> Atualizar(long id, [FromBody] CategoriaRequestDto? dto)
    {
        var atualizada = await _categoriaService.AtualizarAsync(id, dto ?? new CategoriaRequestDto());
        return Ok(atualizada);
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Excluir(long id)
    {
        await _categoriaService.ExcluirAsync(id);
        return Ok(new { mensagem = "Categoria excluída com sucesso." });
    }
}
