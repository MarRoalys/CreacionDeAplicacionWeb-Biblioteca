using Microsoft.AspNetCore.Mvc;
using Practica01MVC.Models;
namespace Practica01MVC.Controllers;
public class ProductoController : Controller
{
 public IActionResult Index()
 {
 var producto = new Producto
 {
 Id = 1,
 Nombre = "Mouse inalambrico",
 Descripcion = "Mouse ergonómico avanzado que combina un diseño con base científica y el alto desempeño.",
 Precio = 102.89m,
 Existencia = 20
 };
 return View(producto);
 }
}
