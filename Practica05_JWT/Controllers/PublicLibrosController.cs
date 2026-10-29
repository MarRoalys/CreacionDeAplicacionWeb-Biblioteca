using BibliotecaNorte.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace Practica05_JWT.Controllers
{
    [ApiController]
    [Route("api/public/libros")]
    public class PublicLibrosController : ControllerBase
    {
        private readonly ILibroService _service;

        public PublicLibrosController(ILibroService service)
        {
            _service = service;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var libros = await _service.GetAllAsync(); 
            return Ok(libros);
        }
    }
}