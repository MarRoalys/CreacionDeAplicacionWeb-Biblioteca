namespace BibliotecaNorte.Api.DTOs;

public class LibroDto
{
    public int IdLibro { get; set; }
    public string Titulo { get; set; } = "";
    public string Isbn { get; set; } = "";
    public int IdAutor { get; set; }
    public string Autor { get; set; } = ""; // Nombres + Apellidos
    public int IdCategoria { get; set; }
    public string Categoria { get; set; } = "";
    public int IdEditorial { get; set; }
    public string Editorial { get; set; } = "";
    public int? AnioPublicacion { get; set; }
    public int Stock { get; set; }
    public int Estado { get; set; }
}