using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MySqlConnector;
using System.Text;
using BibliotecaNorte.Api.Services; // Asegúrate de que esto coincida con tus carpetas

var builder = WebApplication.CreateBuilder(args);

// 1. Agregar los controladores
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// 2. Configurar Swagger para que te aparezca el botón "Authorize" y puedas meter el token
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Pega aquí tu token JWT."
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// 3. Configurar tu conexión a la base de datos MySQL (Leyendo del appsettings.json)
builder.Services.AddTransient<MySqlConnection>(_ =>
    new MySqlConnection(builder.Configuration.GetConnectionString("BibliotecaConnection")));

// 4. Inyección de dependencias de tus servicios
builder.Services.AddScoped<ILibroService, LibroService>();
// Si tienes un servicio para el Login/Usuarios, agrégalo aquí también, por ejemplo:
// builder.Services.AddScoped<IAuthService, AuthService>();

// 5. Configurar la validación del Token JWT
var jwtKey = builder.Configuration["Jwt:Key"] ?? "TuSuperClaveSecretaQueDebeSerLarga12345!"; 
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "http://localhost:5007";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtIssuer,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

// Agregar Autorización
builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// ¡ESTE ORDEN ES VITAL PARA QUE NO DE ERROR!
app.UseAuthentication(); // Primero identifica quién es (El Token)
app.UseAuthorization();  // Luego revisa qué permisos tiene (Los Roles)

app.MapControllers();

app.Run();