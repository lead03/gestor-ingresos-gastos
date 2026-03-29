using System.ComponentModel.DataAnnotations;

namespace ControlGastos.ViewModels;

public class BilleteraVM
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [MaxLength(60, ErrorMessage = "El nombre no puede superar los 60 caracteres.")]
    [MinLength(2,  ErrorMessage = "El nombre debe tener al menos 2 caracteres.")]
    public string Nombre { get; set; } = "";

    // Solo lectura — cantidad de Cuentas vinculadas
    public int CuentasCount { get; set; }
}
