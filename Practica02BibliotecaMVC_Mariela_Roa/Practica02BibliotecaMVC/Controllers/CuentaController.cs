using System.Security.Claims;
using BCrypt.Net;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Practica02BibliotecaMVC.Data;
using Practica02BibliotecaMVC.Models.ViewModels;

namespace Practica02BibliotecaMVC.Controllers;

public class CuentaController : Controller
{
    private readonly UserRepository _userRepository;

    public CuentaController(UserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        return View(new LoginViewModel());
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await _userRepository.GetByEmailAsync(model.Email);
        
        // Verifica si existe y si está activo (estado = 1)
        if (user is null || !user.Active)
        {
            ModelState.AddModelError("", "Correo o contraseña incorrectos.");
            return View(model);
        }

        bool passwordOk;
        try
        {
            passwordOk = BCrypt.Net.BCrypt.Verify(model.Password, user.Password);
        }
        catch
        {
            passwordOk = false;
        }

        if (!passwordOk)
        {
            ModelState.AddModelError("", "Correo o contraseña incorrectos.");
            return View(model);
        }

        // Asignación de Roles y Datos
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, $"{user.Nombres} {user.Apellidos}"),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role) // Aquí va Administrador, Bibliotecario, etc.
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        var properties = new AuthenticationProperties
        {
            IsPersistent = model.RememberMe,
            AllowRefresh = true
        };

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, properties);

        return RedirectToAction("Index", "Home");
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        TempData["MensajeLogout"] = "Su sesión se ha cerrado correctamente.";
        return RedirectToAction(nameof(Login));
    }

    [Authorize]
    [HttpGet]
    public IActionResult Ping()
    {
        return NoContent();
    }

    [AllowAnonymous]
    public IActionResult AccesoDenegado()
    {
        return View();
    }
}