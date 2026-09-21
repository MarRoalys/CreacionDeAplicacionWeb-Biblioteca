using BibliotecaNorte.Api.DTOs;
using BibliotecaNorte.Api.Services;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using Microsoft.AspNetCore.Authorization;

namespace BibliotecaNorte.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class LibrosController : ControllerBase
{
    private readonly ILibroService _service;

    public LibrosController(ILibroService service)
    {
        _service = service;
    }

    // GET es público para todos los que hayan iniciado sesión (Administrador, Bibliotecario, Asistente)
    [HttpGet]
    public async Task<ActionResult<List<LibroDto>>> GetAll()
    {
        return Ok(await _service.GetAllAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<LibroDto>> GetById(int id)
    {
        var libro = await _service.GetByIdAsync(id);
        if (libro is null)
            return NotFound(new { mensaje = "Libro no encontrado." });

        return Ok(libro);
    }

    // Solo el Administrador y el Bibliotecario pueden CREAR
    [Authorize(Roles = "Administrador,Bibliotecario")]
    [HttpPost]
    public async Task<ActionResult<LibroDto>> Create(LibroCreateDto dto)
    {
        try
        {
            int id = await _service.CreateAsync(dto);
            var creado = await _service.GetByIdAsync(id);
            
            return CreatedAtAction(nameof(GetById), new { id }, creado);
        }
        catch (MySqlException ex) when (ex.Number == 1062)
        {
            return Conflict(new { mensaje = "El ISBN ya existe registrado en otro libro." });
        }
        catch (MySqlException ex) when (ex.Number == 1452)
        {
            return BadRequest(new { mensaje = "Autor, categoría o editorial no válida." });
        }
    }

    // Solo el Administrador y el Bibliotecario pueden EDITAR
    [Authorize(Roles = "Administrador,Bibliotecario")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, LibroUpdateDto dto)
    {
        try
        {
            bool actualizado = await _service.UpdateAsync(id, dto);
            if (!actualizado)
                return NotFound();

            return NoContent();
        }
        catch (MySqlException ex) when (ex.Number == 1062)
        {
            return Conflict(new { mensaje = "El ISBN ya existe registrado en otro libro." });
        }
        catch (MySqlException ex) when (ex.Number == 1452)
        {
            return BadRequest(new { mensaje = "Autor, categoría o editorial no válida." });
        }
    }

    // SOLO el Administrador puede ELIMINAR (Aquí bloqueamos a Ana)
    [Authorize(Roles = "Administrador")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            bool eliminado = await _service.DeleteAsync(id);
            // Cambiamos NoContent() por Ok() con tu mensaje
            return eliminado ? Ok(new { mensaje = "El libro fue eliminado con éxito." }) : NotFound();
        }
        catch (MySqlException ex) when (ex.Number == 1451)
        {
            return Conflict(new { mensaje = "No se puede eliminar porque este libro ya tiene préstamos asociados." });
        }
    }
}