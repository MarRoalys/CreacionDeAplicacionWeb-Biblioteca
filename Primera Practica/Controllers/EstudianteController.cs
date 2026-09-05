using Microsoft.AspNetCore.Mvc;
using Practica01MVC.Models;
namespace Practica01MVC.Controllers;
public class EstudianteController : Controller
{
 public IActionResult Index()
 {
 return View();
 }
 public IActionResult Informacion()
 {
 var estudiante = new Estudiante
 {
 Carnet = "23-00741-0",
 Nombres = "Mariela Belen",
 Apellidos = "Roa Garcia",
 Carrera = "Ingenieria en Sistemas de Informacion",
 Edad = 20
 };
 return View(estudiante);
 }
 public IActionResult Saludo(string? nombre)
 {
 if (string.IsNullOrWhiteSpace(nombre))
 {
 return Content("Debe escribir un nombre.");
 }
 return Content($"Hola, {nombre}. Bienvenido a ASP.NET Core MVC.");
 }
 public IActionResult CalcularEdad(int anioNacimiento)
 {
 int anioActual = DateTime.Now.Year;
 if (anioNacimiento < 1900 || anioNacimiento > anioActual)
 {
 return BadRequest("El año de nacimiento no es válido.");
 }
 int edadAproximada = anioActual - anioNacimiento;
 return Content($"Su edad aproximada es: {edadAproximada} años.");
 }
}