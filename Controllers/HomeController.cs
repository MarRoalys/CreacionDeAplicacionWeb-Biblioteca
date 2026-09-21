using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Practica02BibliotecaMVC.Controllers;

[Authorize] // Esto obliga a que NADIE pueda entrar sin iniciar sesión primero
public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    [Authorize(Roles = "Administrador")]
    public IActionResult Administracion()
    {
        return View();
    }

    [Authorize(Roles = "Administrador, Bibliotecario")]
    public IActionResult Prestamos()
    {
        return View();
    }

    [Authorize(Roles = "Administrador, Bibliotecario, Asistente, Lector")]
    public IActionResult Catalogo()
    {
        return View();
    }
}
