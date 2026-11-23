
using Microsoft.AspNetCore.Mvc;
using SaborDeCasa.Api.DTOs;
using SaborDeCasa.Api.Services;

namespace SaborDeCasa.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PratosController : ControllerBase
{
    private readonly IPratoService _pratoService;

    public PratosController(IPratoService pratoService)
    {
        _pratoService = pratoService;
    }

    // GET: api/pratos
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PratoResponseDto>>> ListarTodos()
    {
        var pratos = await _pratoService.ListarTodosAsync();

        return Ok(pratos);
    }

    // GET: api/pratos/5
    [HttpGet("{id:long}")]
    public async Task<ActionResult<PratoResponseDto>> ObterPorId(long id)
    {
        var prato = await _pratoService.ObterPorIdAsync(id);

        return Ok(prato);
    }

    // GET: api/pratos/categoria/2
    [HttpGet("categoria/{categoriaId:long}")]
    public async Task<ActionResult<IEnumerable<PratoResponseDto>>>
        ListarPorCategoria(long categoriaId)
    {
        var pratos = await _pratoService.ListarPorCategoriaAsync(categoriaId);

        return Ok(pratos);
    }

    // POST: api/pratos
    [HttpPost]
    public async Task<ActionResult<PratoResponseDto>> Criar(
        [FromBody] CriarPratoDto dto)
    {
        var prato = await _pratoService.CriarAsync(dto);

        return CreatedAtAction(
            nameof(ObterPorId),
            new { id = prato.Id },
            prato
        );
    }

    // PUT: api/pratos/5
    [HttpPut("{id:long}")]
    public async Task<ActionResult<PratoResponseDto>> Atualizar(
        long id,
        [FromBody] AtualizarPratoDto dto)
    {
        var prato = await _pratoService.AtualizarAsync(id, dto);

        return Ok(prato);
    }

    // DELETE: api/pratos/5
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Excluir(long id)
    {
        await _pratoService.ExcluirAsync(id);

        return NoContent();
    }
}
