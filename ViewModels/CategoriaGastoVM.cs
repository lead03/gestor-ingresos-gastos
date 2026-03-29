using System.ComponentModel.DataAnnotations;

namespace ControlGastos.ViewModels;

public class CategoriaGastoVM
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [MaxLength(80, ErrorMessage = "El nombre no puede superar los 80 caracteres.")]
    [MinLength(2,  ErrorMessage = "El nombre debe tener al menos 2 caracteres.")]
    public string Nombre { get; set; } = "";

    [Range(1, 2, ErrorMessage = "Debe seleccionar un tipo válido.")]
    public int TipoId { get; set; }

    // Solo lectura — para display en tabla
    public string TipoNombre { get; set; } = "";

    public bool Habilitada { get; set; } = true;

    // Solo lectura — cantidad de GastoItems vinculados
    public int GastoItemsCount { get; set; }
}
