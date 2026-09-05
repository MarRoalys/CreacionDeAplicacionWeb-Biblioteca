namespace Practica01MVC.Models;
public class Producto
{
 public int Id { get; set; }
 public string Nombre { get; set; } = "";
 public string Descripcion { get; set; } = "";
 public decimal Precio { get; set; }
 public int Existencia { get; set; }
}