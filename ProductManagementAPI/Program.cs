using Microsoft.EntityFrameworkCore;
using ProductManagementAPI.Data;

var builder = WebApplication.CreateBuilder(args);

// Configuraci�n del DbContext para SQL Server, con cadena de conexi�n desde appsettings.json
builder.Services.AddDbContext<ProductosDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Agregar servicios para controladores y Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Middleware para documentaci�n Swagger solo en desarrollo
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
