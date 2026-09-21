using BibliotecaNorte.Api.DTOs;

namespace BibliotecaNorte.Api.Services;

public interface IAuthService
{
    Task<LoginResponseDto?> LoginAsync(LoginDto dto);
}