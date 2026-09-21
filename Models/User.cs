namespace Practica02BibliotecaMVC.Models;

public class User
{
    public long Id { get; set; }
    public string Nombres { get; set; } = "";
    public string Apellidos { get; set; } = "";
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    public string Role { get; set; } = "";
    public bool Active { get; set; }
}