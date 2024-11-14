using Microsoft.EntityFrameworkCore;
using WebApplication2;
using WebApplication2.Controllers;
using WebApplication2.Models;

var builder = WebApplication.CreateBuilder(args);

// Añadir servicios al contenedor.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configurar el DbContext para usar SQL Server
builder.Services.AddDbContext<StarBankContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("cadenaSQL")));

// Registrar EmailService como singleton
builder.Services.AddSingleton<EmailService>();

// Configurar CORS
builder.Services.AddCors(options => options.AddPolicy("AllowWebapp",
    builder => builder.AllowAnyOrigin()
                      .AllowAnyHeader()
                      .AllowAnyMethod()));

var app = builder.Build();

// Configurar el pipeline de solicitudes HTTP.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage(); // Página de excepciones en desarrollo
}

app.UseAuthorization();
app.MapControllers();
app.UseCors("AllowWebapp");

app.Run();
