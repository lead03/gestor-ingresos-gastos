using System.ComponentModel.DataAnnotations;

namespace ControlGastos.Models;

public class Cuenta
{
    public int Id { get; set; }

    [Required, MaxLength(60)]
    public string Nombre { get; set; } = "";   // "Galicia", "PersonalPay", "Binance", etc.

    public decimal SaldoInicio { get; set; }
    public decimal SaldoFinal { get; set; }

    public int Mes { get; set; }
    public int Anio { get; set; }
}
