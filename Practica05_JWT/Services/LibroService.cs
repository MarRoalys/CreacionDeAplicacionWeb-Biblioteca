using BibliotecaNorte.Api.DTOs;
using MySqlConnector;

namespace BibliotecaNorte.Api.Services;

public class LibroService : ILibroService
{
    private readonly string _connectionString;

    public LibroService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("BibliotecaConnection")
            ?? throw new InvalidOperationException("No existe la cadena BibliotecaConnection.");
    }

    public async Task<List<LibroDto>> GetAllAsync()
    {
        var lista = new List<LibroDto>();
        const string sql = """
            SELECT l.id_libro, l.titulo, l.isbn, 
                   l.id_autor, CONCAT(a.nombres, ' ', a.apellidos) AS autor, 
                   l.id_categoria, c.nombre AS categoria, 
                   l.id_editorial, e.nombre AS editorial,
                   l.anio_publicacion, l.stock, l.estado
            FROM libro l
            INNER JOIN autor a ON a.id_autor = l.id_autor
            INNER JOIN categoria c ON c.id_categoria = l.id_categoria
            INNER JOIN editorial e ON e.id_editorial = l.id_editorial
            ORDER BY l.id_libro;
            """;

        await using var cn = new MySqlConnection(_connectionString);
        await cn.OpenAsync();
        await using var cmd = new MySqlCommand(sql, cn);
        await using var rd = await cmd.ExecuteReaderAsync();

        while (await rd.ReadAsync())
        {
            lista.Add(MapLibro(rd));
        }
        return lista;
    }

    public async Task<LibroDto?> GetByIdAsync(int id)
    {
        const string sql = """
            SELECT l.id_libro, l.titulo, l.isbn, 
                   l.id_autor, CONCAT(a.nombres, ' ', a.apellidos) AS autor, 
                   l.id_categoria, c.nombre AS categoria, 
                   l.id_editorial, e.nombre AS editorial,
                   l.anio_publicacion, l.stock, l.estado
            FROM libro l
            INNER JOIN autor a ON a.id_autor = l.id_autor
            INNER JOIN categoria c ON c.id_categoria = l.id_categoria
            INNER JOIN editorial e ON e.id_editorial = l.id_editorial
            WHERE l.id_libro = @id;
            """;

        await using var cn = new MySqlConnection(_connectionString);
        await cn.OpenAsync();
        await using var cmd = new MySqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@id", id);
        await using var rd = await cmd.ExecuteReaderAsync();

        return await rd.ReadAsync() ? MapLibro(rd) : null;
    }

    public async Task<int> CreateAsync(LibroCreateDto dto)
    {
        const string sql = """
            INSERT INTO libro 
            (id_categoria, id_autor, id_editorial, isbn, titulo, anio_publicacion, stock, estado)
            VALUES 
            (@categoria, @autor, @editorial, @isbn, @titulo, @anio, @stock, @estado);
            SELECT LAST_INSERT_ID();
            """;

        await using var cn = new MySqlConnection(_connectionString);
        await cn.OpenAsync();
        await using var cmd = new MySqlCommand(sql, cn);
        AddParameters(cmd, dto);

        return Convert.ToInt32(await cmd.ExecuteScalarAsync());
    }

    public async Task<bool> UpdateAsync(int id, LibroUpdateDto dto)
    {
        const string sql = """
            UPDATE libro 
            SET id_categoria = @categoria,
                id_autor = @autor,
                id_editorial = @editorial,
                isbn = @isbn, 
                titulo = @titulo, 
                anio_publicacion = @anio, 
                stock = @stock,
                estado = @estado
            WHERE id_libro = @id;
            """;

        await using var cn = new MySqlConnection(_connectionString);
        await cn.OpenAsync();
        await using var cmd = new MySqlCommand(sql, cn);
        AddParameters(cmd, dto);
        cmd.Parameters.AddWithValue("@id", id);

        return await cmd.ExecuteNonQueryAsync() > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        const string sql = "DELETE FROM libro WHERE id_libro = @id;";
        await using var cn = new MySqlConnection(_connectionString);
        await cn.OpenAsync();
        await using var cmd = new MySqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@id", id);

        return await cmd.ExecuteNonQueryAsync() > 0;
    }

    // --- MÉTODOS DE AYUDA ---

    private static void AddParameters(MySqlCommand cmd, LibroCreateDto dto)
    {
        cmd.Parameters.AddWithValue("@titulo", dto.Titulo);
        cmd.Parameters.AddWithValue("@isbn", dto.Isbn);
        cmd.Parameters.AddWithValue("@autor", dto.IdAutor);
        cmd.Parameters.AddWithValue("@categoria", dto.IdCategoria);
        cmd.Parameters.AddWithValue("@editorial", dto.IdEditorial);
        cmd.Parameters.AddWithValue("@anio", (object?)dto.AnioPublicacion ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@stock", dto.Stock);
        cmd.Parameters.AddWithValue("@estado", dto.Estado);
    }

    private static LibroDto MapLibro(MySqlDataReader rd)
    {
        return new LibroDto
        {
            IdLibro = rd.GetInt32("id_libro"),
            Titulo = rd.GetString("titulo"),
            Isbn = rd.GetString("isbn"),
            IdAutor = rd.GetInt32("id_autor"),
            Autor = rd.GetString("autor"),
            IdCategoria = rd.GetInt32("id_categoria"),
            Categoria = rd.GetString("categoria"),
            IdEditorial = rd.GetInt32("id_editorial"),
            Editorial = rd.GetString("editorial"),
            AnioPublicacion = rd.IsDBNull(rd.GetOrdinal("anio_publicacion")) ? null : rd.GetInt32("anio_publicacion"),
            Stock = rd.GetInt32("stock"),
            Estado = Convert.ToInt32(rd["estado"]) // Convertimos el TINYINT a int estándar
        };
    }
}