using MySqlConnector;
using Practica02BibliotecaMVC.Models;

namespace Practica02BibliotecaMVC.Data;

public class UserRepository
{
    private readonly string _connectionString;

    public UserRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("BibliotecaConnection")
            ?? throw new InvalidOperationException("No se encontró la cadena BibliotecaConnection.");
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        const string sql = @"
            SELECT u.id_usuario, u.nombres, u.apellidos, u.correo, u.contrasena, r.nombre AS rol_nombre, u.estado 
            FROM usuario u
            INNER JOIN rol r ON u.id_rol = r.id_rol
            WHERE u.correo = @email 
            LIMIT 1;";

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@email", email.Trim());

        await using var reader = await command.ExecuteReaderAsync();
        
        if (!await reader.ReadAsync())
            return null;

        return new User
        {
            Id = reader.GetInt64("id_usuario"),
            Nombres = reader.GetString("nombres"),
            Apellidos = reader.GetString("apellidos"),
            Email = reader.GetString("correo"),
            Password = reader.GetString("contrasena"),
            Role = reader.GetString("rol_nombre"),
            Active = reader.GetBoolean("estado")
        };
    }
}