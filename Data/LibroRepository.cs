using MySqlConnector;
using Practica02BibliotecaMVC.Models;

namespace Practica02BibliotecaMVC.Data;

public class LibroRepository
{
    private readonly string _connectionString;

    public LibroRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("BibliotecaConnection")
            ?? throw new InvalidOperationException("No se encontró la cadena BibliotecaConnection.");
    }

    public async Task<List<Libro>> GetAllAsync()
    {
        const string sql = @"
            SELECT l.id_libro, l.id_categoria, l.id_autor, l.id_editorial, l.isbn, l.titulo, l.anio_publicacion, l.stock, l.estado,
                   c.nombre AS categoria_nombre, 
                   CONCAT(a.nombres, ' ', a.apellidos) AS autor_nombre, 
                   e.nombre AS editorial_nombre
            FROM libro l
            INNER JOIN categoria c ON l.id_categoria = c.id_categoria
            INNER JOIN autor a ON l.id_autor = a.id_autor
            INNER JOIN editorial e ON l.id_editorial = e.id_editorial
            ORDER BY l.id_libro DESC;";

        var lista = new List<Libro>();
        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var command = new MySqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            lista.Add(MapLibro(reader));
        }
        return lista;
    }

    public async Task<Libro?> GetByIdAsync(int id)
    {
        const string sql = @"
            SELECT l.id_libro, l.id_categoria, l.id_autor, l.id_editorial, l.isbn, l.titulo, l.anio_publicacion, l.stock, l.estado,
                   c.nombre AS categoria_nombre, 
                   CONCAT(a.nombres, ' ', a.apellidos) AS autor_nombre, 
                   e.nombre AS editorial_nombre
            FROM libro l
            INNER JOIN categoria c ON l.id_categoria = c.id_categoria
            INNER JOIN autor a ON l.id_autor = a.id_autor
            INNER JOIN editorial e ON l.id_editorial = e.id_editorial
            WHERE l.id_libro = @id LIMIT 1;";

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id);
        
        await using var reader = await command.ExecuteReaderAsync();
        return await reader.ReadAsync() ? MapLibro(reader) : null;
    }

    public async Task<int> CreateAsync(Libro libro)
    {
        const string sql = @"
            INSERT INTO libro (id_categoria, id_autor, id_editorial, isbn, titulo, anio_publicacion, stock, estado)
            VALUES (@idCategoria, @idAutor, @idEditorial, @isbn, @titulo, @anio, @stock, @estado);";

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var command = new MySqlCommand(sql, connection);
        AddParameters(command, libro);
        
        await command.ExecuteNonQueryAsync();
        return (int)command.LastInsertedId;
    }

    public async Task<bool> UpdateAsync(Libro libro)
    {
        const string sql = @"
            UPDATE libro 
            SET id_categoria = @idCategoria, id_autor = @idAutor, id_editorial = @idEditorial, 
                isbn = @isbn, titulo = @titulo, anio_publicacion = @anio, stock = @stock, estado = @estado
            WHERE id_libro = @id;";

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var command = new MySqlCommand(sql, connection);
        AddParameters(command, libro);
        command.Parameters.AddWithValue("@id", libro.IdLibro);
        
        return await command.ExecuteNonQueryAsync() > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        const string sql = "DELETE FROM libro WHERE id_libro = @id;";
        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id);
        return await command.ExecuteNonQueryAsync() > 0;
    }

    // Métodos para cargar las listas desplegables
    public async Task<List<CatalogoItem>> GetCategoriasAsync() => 
        await GetCatalogAsync("SELECT id_categoria AS id, nombre FROM categoria ORDER BY nombre;");

    public async Task<List<CatalogoItem>> GetAutoresAsync() => 
        await GetCatalogAsync("SELECT id_autor AS id, CONCAT(nombres, ' ', apellidos) AS nombre FROM autor ORDER BY nombres;");

    public async Task<List<CatalogoItem>> GetEditorialesAsync() => 
        await GetCatalogAsync("SELECT id_editorial AS id, nombre FROM editorial ORDER BY nombre;");

    private async Task<List<CatalogoItem>> GetCatalogAsync(string sql)
    {
        var lista = new List<CatalogoItem>();
        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var command = new MySqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            lista.Add(new CatalogoItem { Id = reader.GetInt32("id"), Nombre = reader.GetString("nombre") });
        }
        return lista;
    }

    private static void AddParameters(MySqlCommand command, Libro libro)
    {
        command.Parameters.AddWithValue("@idCategoria", libro.IdCategoria);
        command.Parameters.AddWithValue("@idAutor", libro.IdAutor);
        command.Parameters.AddWithValue("@idEditorial", libro.IdEditorial);
        command.Parameters.AddWithValue("@isbn", libro.Isbn.Trim());
        command.Parameters.AddWithValue("@titulo", libro.Titulo.Trim());
        command.Parameters.AddWithValue("@anio", (object?)libro.AnioPublicacion ?? DBNull.Value);
        command.Parameters.AddWithValue("@stock", libro.Stock);
        command.Parameters.AddWithValue("@estado", libro.Estado);
    }

    private static Libro MapLibro(MySqlDataReader reader)
    {
        return new Libro
        {
            IdLibro = reader.GetInt32("id_libro"),
            IdCategoria = reader.GetInt32("id_categoria"),
            IdAutor = reader.GetInt32("id_autor"),
            IdEditorial = reader.GetInt32("id_editorial"),
            Isbn = reader.GetString("isbn"),
            Titulo = reader.GetString("titulo"),
            AnioPublicacion = reader.IsDBNull(reader.GetOrdinal("anio_publicacion")) ? null : reader.GetInt32("anio_publicacion"),
            Stock = reader.GetInt32("stock"),
            Estado = reader.GetInt32("estado"),
            CategoriaNombre = reader.GetString("categoria_nombre"),
            AutorNombreCompleto = reader.GetString("autor_nombre"),
            EditorialNombre = reader.GetString("editorial_nombre")
        };
    }
}