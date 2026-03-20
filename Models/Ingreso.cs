using System.ComponentModel.DataAnnotations;

namespace ControlGastos.Models;

public class Ingreso
{
    public int Id { get; set; }
    public int Mes { get; set; }
    public int Anio { get; set; }
    public int Dia { get; set; }

    // "Propio" | "Distribuido" | "CuentaPropia" | "Ahorro" | "Dpto" | "USS" | "FIMA" | "Resto"
    [Required, MaxLength(50)]
    public string Tipo { get; set; } = "Propio";

    public decimal Monto { get; set; }

    [MaxLength(200)]
    public string? Descripcion { get; set; }

    // Banco/cuenta de origen
    [MaxLength(50)]
    public string? Fuente { get; set; }
}
