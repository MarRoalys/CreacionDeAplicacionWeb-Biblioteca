using Microsoft.AspNetCore.Mvc;
using MySqlConnector;

namespace BibliotecaNorte.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CatalogosController : ControllerBase
{
    private readonly string _connectionString;

    public CatalogosController(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("BibliotecaConnection")!;
    }

    [HttpGet("categorias")]
    public async Task<IActionResult> Categorias()
    {
        return Ok(await ConsultarAsync(
            "SELECT id_categoria AS id, nombre FROM categoria ORDER BY nombre;"));
    }

    [HttpGet("autores")]
    public async Task<IActionResult> Autores()
    {
        // Unimos el nombre y apellido para el select del frontend
        return Ok(await ConsultarAsync(
            "SELECT id_autor AS id, CONCAT(nombres, ' ', apellidos) AS nombre FROM autor ORDER BY nombres;"));
    }

    [HttpGet("editoriales")]
    public async Task<IActionResult> Editoriales()
    {
        return Ok(await ConsultarAsync(
            "SELECT id_editorial AS id, nombre FROM editorial ORDER BY nombre;"));
    }

    private async Task<List<object>> ConsultarAsync(string sql)
    {
        var lista = new List<object>();
        await using var cn = new MySqlConnection(_connectionString);
        await cn.OpenAsync();
        await using var cmd = new MySqlCommand(sql, cn);
        await using var rd = await cmd.ExecuteReaderAsync();

        while (await rd.ReadAsync())
        {
            lista.Add(new
            {
                id = rd.GetInt32("id"),
                nombre = rd.GetString("nombre")
            });
        }
        return lista;
    }
}