using System.ComponentModel.DataAnnotations;

namespace BibliotecaNorte.Api.DTOs;

public class LibroCreateDto
{
   [Required, StringLength(200)]
    public string Titulo { get; set; } = "";

    [Required, StringLength(20)]
    public string Isbn { get; set; } = "";

    [Range(1, int.MaxValue)]
    public int IdAutor { get; set; }

    [Range(1, int.MaxValue)]
    public int IdCategoria { get; set; }

    [Range(1, int.MaxValue)]
    public int IdEditorial { get; set; }

    public int? AnioPublicacion { get; set; }

    [Range(0, int.MaxValue)]
    public int Stock { get; set; } = 0;

    public int Estado { get; set; } = 1; // 1 = Activo, 0 = Inactivo
}