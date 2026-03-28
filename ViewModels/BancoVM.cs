using System.ComponentModel.DataAnnotations;

namespace ControlGastos.ViewModels;

public class BancoVM
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre del banco es obligatorio.")]
    [MaxLength(60, ErrorMessage = "El nombre no puede superar los 60 caracteres.")]
    [MinLength(2, ErrorMessage = "El nombre debe tener al menos 2 caracteres.")]
    public string Nombre { get; set; } = "";

    public int CuentasCount { get; set; }
}
