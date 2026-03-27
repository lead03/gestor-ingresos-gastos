using System.ComponentModel.DataAnnotations;

namespace ControlGastos.Models;

public class CategoriaGasto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [MaxLength(80, ErrorMessage = "El nombre no puede superar los 80 caracteres.")]
    [MinLength(2, ErrorMessage = "El nombre debe tener al menos 2 caracteres.")]
    public string Nombre { get; set; } = "";

    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un tipo.")]
    public int TipoId { get; set; }
    public TipoCategoriaGasto Tipo { get; set; } = null!;

    public bool Habilitada { get; set; } = true;
}