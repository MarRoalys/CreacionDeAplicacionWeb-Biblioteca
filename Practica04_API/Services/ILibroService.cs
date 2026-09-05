using BibliotecaNorte.Api.DTOs;

namespace BibliotecaNorte.Api.Services;

public interface ILibroService
{
    Task<List<LibroDto>> GetAllAsync();
    Task<LibroDto?> GetByIdAsync(int id);
    Task<int> CreateAsync(LibroCreateDto dto);
    Task<bool> UpdateAsync(int id, LibroUpdateDto dto);
    Task<bool> DeleteAsync(int id);
}