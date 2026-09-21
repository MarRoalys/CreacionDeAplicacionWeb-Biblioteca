using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MySqlConnector;
using Practica02BibliotecaMVC.Data;
using Practica02BibliotecaMVC.Models;

namespace Practica02BibliotecaMVC.Controllers;

[Authorize] // Exige que TODOS estén autenticados para entrar aquí
public class LibrosController : Controller
{
    private readonly LibroRepository _repository;

    public LibrosController(LibroRepository repository)
    {
        _repository = repository;
    }

    // READ: Todos los usuarios autenticados pueden ver la lista
    public async Task<IActionResult> Index()
    {
        var libros = await _repository.GetAllAsync();
        return View(libros);
    }

    public async Task<IActionResult> Details(int id)
    {
        var libro = await _repository.GetByIdAsync(id);
        if (libro is null) return NotFound();
        return View(libro);
    }

    // CREATE: Administrador y Bibliotecario
    [Authorize(Roles = "Administrador, Bibliotecario")]
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await CargarCatalogosAsync();
        return View(new Libro { Estado = 1 });
    }

    [Authorize(Roles = "Administrador, Bibliotecario")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Libro libro)
    {
        if (!ModelState.IsValid)
        {
            await CargarCatalogosAsync(libro);
            return View(libro);
        }

        try
        {
            int id = await _repository.CreateAsync(libro);
            TempData["Mensaje"] = "Libro registrado correctamente en el catálogo.";
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (MySqlException ex) when (ex.Number == 1062) // Error 1062: Registro duplicado
        {
            ModelState.AddModelError(nameof(libro.Isbn), "Ya existe un libro con ese código ISBN.");
            await CargarCatalogosAsync(libro);
            return View(libro);
        }
    }

    // UPDATE: Administrador y Bibliotecario
    [Authorize(Roles = "Administrador, Bibliotecario")]
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var libro = await _repository.GetByIdAsync(id);
        if (libro is null) return NotFound();
        
        await CargarCatalogosAsync(libro);
        return View(libro);
    }

    [Authorize(Roles = "Administrador, Bibliotecario")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Libro libro)
    {
        if (id != libro.IdLibro) return BadRequest();

        if (!ModelState.IsValid)
        {
            await CargarCatalogosAsync(libro);
            return View(libro);
        }

        try
        {
            bool actualizado = await _repository.UpdateAsync(libro);
            if (!actualizado) return NotFound();
            
            TempData["Mensaje"] = "Datos del libro actualizados correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (MySqlException ex) when (ex.Number == 1062)
        {
            ModelState.AddModelError(nameof(libro.Isbn), "Ya existe otro libro registrado con ese código ISBN.");
            await CargarCatalogosAsync(libro);
            return View(libro);
        }
    }

    // DELETE: Solamente Administrador
    [Authorize(Roles = "Administrador")]
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var libro = await _repository.GetByIdAsync(id);
        if (libro is null) return NotFound();
        return View(libro);
    }

    [Authorize(Roles = "Administrador")]
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            bool eliminado = await _repository.DeleteAsync(id);
            if (!eliminado) return NotFound();
            
            TempData["Mensaje"] = "Libro eliminado correctamente del catálogo.";
        }
        catch (MySqlException ex) when (ex.Number == 1451) // Error 1451: Restricción de llave foránea
        {
            TempData["Error"] = "No se puede eliminar este libro porque tiene préstamos u otros registros asociados en la base de datos.";
        }
        
        return RedirectToAction(nameof(Index));
    }

    // Función auxiliar para cargar las listas desplegables (Categorías, Autores, Editoriales)
    private async Task CargarCatalogosAsync(Libro? libro = null)
    {
        ViewBag.Categorias = new SelectList(await _repository.GetCategoriasAsync(), "Id", "Nombre", libro?.IdCategoria);
        ViewBag.Autores = new SelectList(await _repository.GetAutoresAsync(), "Id", "Nombre", libro?.IdAutor);
        ViewBag.Editoriales = new SelectList(await _repository.GetEditorialesAsync(), "Id", "Nombre", libro?.IdEditorial);
    }
}