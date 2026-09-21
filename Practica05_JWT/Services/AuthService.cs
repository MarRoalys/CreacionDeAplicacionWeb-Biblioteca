using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BibliotecaNorte.Api.DTOs;
using BibliotecaNorte.Api.Models;
using Microsoft.IdentityModel.Tokens;
using MySqlConnector;

namespace BibliotecaNorte.Api.Services;

public class AuthService : IAuthService
{
    private readonly IConfiguration _configuration;
    private readonly string _connectionString;

    public AuthService(IConfiguration configuration)
    {
        _configuration = configuration;
        _connectionString = configuration.GetConnectionString("BibliotecaConnection")
            ?? throw new InvalidOperationException("No existe la cadena BibliotecaConnection.");
    }

    public async Task<LoginResponseDto?> LoginAsync(LoginDto dto)
    {
        var user = await FindByEmailAsync(dto.Email);
        
        if (user is null || !user.Active)
            return null;
        bool passwordOk = BCrypt.Net.BCrypt.Verify(dto.Password, user.Password);
        
        if (!passwordOk)
            return null;

        return CreateToken(user);
    }

    private async Task<AuthUser?> FindByEmailAsync(string email)
    {
        const string sql = """
            SELECT u.id_usuario, u.nombres, u.apellidos, u.correo, u.contrasena, r.nombre AS rol, u.estado 
            FROM usuario u
            INNER JOIN rol r ON u.id_rol = r.id_rol
            WHERE u.correo = @email
            LIMIT 1;
            """;

        await using var cn = new MySqlConnection(_connectionString);
        await cn.OpenAsync();
        await using var cmd = new MySqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@email", email); // El parámetro de búsqueda
        
        await using var rd = await cmd.ExecuteReaderAsync();
        
        if (!await rd.ReadAsync())
            return null;

        // Mapeo exacto a las columnas de tu script SQL
        return new AuthUser
        {
            Id = rd.GetInt32("id_usuario"),
            Nombres = rd.GetString("nombres"),
            Apellidos = rd.GetString("apellidos"),
            Email = rd.GetString("correo"),
            Password = rd.GetString("contrasena"),
            Role = rd.GetString("rol"),
            Active = rd.GetBoolean("estado")
        };
    }
    private LoginResponseDto CreateToken(AuthUser user)
    {
        string key = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key no configurado.");
        string issuer = _configuration["Jwt:Issuer"] ?? "BibliotecaNorteApi";
        string audience = _configuration["Jwt:Audience"] ?? "BibliotecaNorteUsuarios";
        
        DateTime expires = DateTime.UtcNow.AddMinutes(60); // 60 minutos de validez

        // Empacamos la información del usuario en el Token 
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, $"{user.Nombres} {user.Apellidos}"),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role)
        };

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var jwt = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expires,
            signingCredentials: credentials);

        return new LoginResponseDto
        {
            Token = new JwtSecurityTokenHandler().WriteToken(jwt),
            ExpiresAt = expires,
            Name = $"{user.Nombres} {user.Apellidos}",
            Email = user.Email,
            Role = user.Role
        };
    }
}