using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ProductManagementAPI.Data;
using ProductManagementAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProductManagementAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductoController : ControllerBase
{
    private readonly ProductosDbContext _context;

    public ProductoController(ProductosDbContext context)
    {
        _context = context;
    }

    // GET: api/Producto
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Productos>>> GetProductos()
    {
        try
        {
            var productos = await _context.Productos
                .FromSqlRaw("EXEC sp_ObtenerProductos")
                .ToListAsync();

            return Ok(productos);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[ERROR] {DateTime.Now}: {ex.Message}");

            return StatusCode(500, new
            {
                mensaje = "Ocurrió un error al obtener los productos.",
                detalle = ex.Message
            });
        }
    }


    // GET: api/Producto/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Productos>> GetProducto(int id)
    {
        try
        {
            var productos = await _context.Productos
                .FromSqlRaw("EXEC sp_ObtenerProductoPorId @Id = {0}", id)
                .AsNoTracking()
                .ToListAsync();

            var producto = productos.FirstOrDefault();

            if (producto == null)
                return NotFound(new { mensaje = "Producto no encontrado." });

            return Ok(producto);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[ERROR] {DateTime.Now}: {ex.Message}");

            return StatusCode(500, new
            {
                mensaje = "Ocurrió un error al obtener el producto.",
                detalle = ex.Message
            });
        }
    }



    // POST: api/Producto
    [HttpPost]
    public async Task<ActionResult> CrearProducto([FromBody] ProductoCreateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var parametros = new[]
            {
            new SqlParameter("@Nombre", dto.Nombre),
            new SqlParameter("@Descripcion", (object?)dto.Descripcion ?? DBNull.Value),
            new SqlParameter("@Precio", dto.Precio),
            new SqlParameter("@Stock", dto.Stock)
        };

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC sp_InsertarProducto @Nombre, @Descripcion, @Precio, @Stock",
                parametros
            );

            return Ok(new { mensaje = "Producto creado exitosamente." });
        }
        catch (SqlException ex)
        {
            return StatusCode(500, new { mensaje = "Error en la base de datos.", detalle = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensaje = "Ocurrió un error inesperado.", detalle = ex.Message });
        }
    }




    // PUT: api/Producto/5
    [HttpPut("{id:int}")]
    public async Task<ActionResult> ActualizarProducto(int id, [FromBody] ProductoUpdateDto dto)
    {
        try
        {
            var productoActual = await _context.Productos.FirstOrDefaultAsync(p => p.Id == id);
            if (productoActual == null)
                return NotFound(new { mensaje = "Producto no encontrado." });

            // Aplicar los cambios solo en el campo que se recib
            var nombre = dto.Nombre ?? productoActual.Nombre;
            var descripcion = dto.Descripcion ?? productoActual.Descripcion;
            var precio = dto.Precio ?? productoActual.Precio;
            var stock = dto.Stock ?? productoActual.Stock;

            var parametros = new[]
            {
            new SqlParameter("@Id", id),
            new SqlParameter("@Nombre", nombre),
            new SqlParameter("@Descripcion", (object?)descripcion ?? DBNull.Value),
            new SqlParameter("@Precio", precio),
            new SqlParameter("@Stock", stock)
        };

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC sp_ActualizarProducto @Id, @Nombre, @Descripcion, @Precio, @Stock",
                parametros
            );

            return Ok(new { mensaje = "Producto actualizado exitosamente." });
        }
        catch (SqlException ex)
        {
            return StatusCode(500, new { mensaje = "Error en la base de datos.", detalle = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensaje = "Ocurrió un error inesperado.", detalle = ex.Message });
        }
    }




    // DELETE: api/Producto/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteProducto(int id)
    {
        try
        {
            var existe = await _context.Productos.AnyAsync(p => p.Id == id);
            if (!existe)
                return NotFound(new { mensaje = "Producto no encontrado." });

            var parametros = new[]
            {
            new SqlParameter("@Id", id)
        };

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC sp_EliminarProducto @Id",
                parametros
            );

            return Ok(new { mensaje = "Producto eliminado correctamente." });
        }
        catch (SqlException ex)
        {
            return StatusCode(500, new { mensaje = "Error en la base de datos.", detalle = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensaje = "Ocurrió un error inesperado.", detalle = ex.Message });
        }
    }
}
