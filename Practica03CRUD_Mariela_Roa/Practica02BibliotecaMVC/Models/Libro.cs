using System.ComponentModel.DataAnnotations;

namespace Practica02BibliotecaMVC.Models;

public class Libro
{
    public int IdLibro { get; set; }

    [Required(ErrorMessage = "La categoría es obligatoria.")]
    [Display(Name = "Categoría")]
    public int IdCategoria { get; set; }

    [Required(ErrorMessage = "El autor es obligatorio.")]
    [Display(Name = "Autor")]
    public int IdAutor { get; set; }

    [Required(ErrorMessage = "La editorial es obligatoria.")]
    [Display(Name = "Editorial")]
    public int IdEditorial { get; set; }

    [Required(ErrorMessage = "El código ISBN es obligatorio.")]
    [StringLength(20)]
    [Display(Name = "Código ISBN")]
    public string Isbn { get; set; } = "";

    [Required(ErrorMessage = "El título es obligatorio.")]
    [StringLength(200)]
    [Display(Name = "Título del Libro")]
    public string Titulo { get; set; } = "";

    [Display(Name = "Año de Publicación")]
    public int? AnioPublicacion { get; set; }

    [Required(ErrorMessage = "El stock es obligatorio.")]
    [Range(0, 1000, ErrorMessage = "El stock debe ser un número válido.")]
    public int Stock { get; set; } = 0;

    public int Estado { get; set; } = 1;

    // Propiedades auxiliares para mostrar nombres en la tabla Index y en Details
    public string? CategoriaNombre { get; set; }
    public string? AutorNombreCompleto { get; set; }
    public string? EditorialNombre { get; set; }
}