using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductManagementAPI.Data;
using ProductManagementAPI.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

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
            // Aquí puedes enviar el error a un sistema de logs como Serilog, NLog, etc.
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
                .ToListAsync(); // Ejecutamos la consulta aquí

            var producto = productos.FirstOrDefault(); // Lo filtramos en memoria

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
    public async Task<IActionResult> PostProducto([FromBody] ProductoCreateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (dto.Stock < 0)
            return BadRequest(new { mensaje = "El stock no puede ser negativo." });

        try
        {
            int nuevoId;

            using (var connection = _context.Database.GetDbConnection())
            {
                await connection.OpenAsync();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "sp_InsertarProducto";
                    command.CommandType = System.Data.CommandType.StoredProcedure;

                    // Parámetros de entrada
                    var pNombre = command.CreateParameter();
                    pNombre.ParameterName = "@Nombre";
                    pNombre.Value = dto.Nombre;
                    command.Parameters.Add(pNombre);

                    var pDescripcion = command.CreateParameter();
                    pDescripcion.ParameterName = "@Descripcion";
                    pDescripcion.Value = dto.Descripcion;
                    command.Parameters.Add(pDescripcion);

                    var pPrecio = command.CreateParameter();
                    pPrecio.ParameterName = "@Precio";
                    pPrecio.Value = dto.Precio;
                    command.Parameters.Add(pPrecio);

                    var pStock = command.CreateParameter();
                    pStock.ParameterName = "@Stock";
                    pStock.Value = dto.Stock;
                    command.Parameters.Add(pStock);

                    // Parámetro de salida (ID generado)
                    var pId = command.CreateParameter();
                    pId.ParameterName = "@NuevoId";
                    pId.Direction = System.Data.ParameterDirection.Output;
                    pId.DbType = System.Data.DbType.Int32;
                    command.Parameters.Add(pId);

                    await command.ExecuteNonQueryAsync();
                    nuevoId = (int)pId.Value;
                }
            }

            return CreatedAtAction(nameof(GetProducto), new { id = nuevoId }, new
            {
                id = nuevoId,
                dto.Nombre,
                dto.Descripcion,
                dto.Precio,
                dto.Stock
            });
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[ERROR - PostProducto] {DateTime.Now}: {ex.Message}");
            return StatusCode(500, new
            {
                mensaje = "Ocurrió un error al insertar el producto.",
                detalle = ex.Message
            });
        }
    }



    // PUT: api/Producto/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutProducto(int id, [FromBody] ProductoUpdateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (dto.Stock < 0)
            return BadRequest(new { mensaje = "El stock no puede ser negativo." });

        var productoExistente = await _context.Productos.FindAsync(id);
        if (productoExistente == null)
            return NotFound(new { mensaje = "Producto no encontrado." });

        productoExistente.Nombre = dto.Nombre;
        productoExistente.Descripcion = dto.Descripcion;
        productoExistente.Precio = dto.Precio;
        productoExistente.Stock = dto.Stock;

        _context.Productos.Add(productoExistente);
        await _context.SaveChangesAsync();

        return Ok(new { mensaje = "Producto actualizado correctamente." });
    }



    // DELETE: api/Producto/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProducto(int id)
    {
        var producto = await _context.Productos.FindAsync(id);
        if (producto == null)
            return NotFound(new { mensaje = "Producto no encontrado." });

        _context.Productos.Remove(producto);
        await _context.SaveChangesAsync();

        return Ok(new { mensaje = "Producto eliminado correctamente." });
    }

    private bool ProductoExists(int id)
    {
        return _context.Productos.Any(e => e.Id == id);
    }
}
