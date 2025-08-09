using System.ComponentModel.DataAnnotations;

public class ProductoUpdateDto
{
    [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres.")]
    public string? Nombre { get; set; }

    [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres.")]
    public string? Descripcion { get; set; }

    [Range(0.01, 9999999.99, ErrorMessage = "El precio debe ser mayor a 0.")]
    public decimal? Precio { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo.")]
    public int? Stock { get; set; }
}
