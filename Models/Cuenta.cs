using System.ComponentModel.DataAnnotations;

namespace ControlGastos.Models;

public class Cuenta
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [MaxLength(60)]
    public string Nombre { get; set; } = "";

    public int         TipoId     { get; set; } = 2; // 0=Efectivo, 1=Banco, 2=Billetera
    public TipoEntidad? TipoEntidad { get; set; }
    public Moneda     Moneda { get; set; } = Moneda.ARS;

    [Range(0, double.MaxValue, ErrorMessage = "El saldo inicial no puede ser negativo.")]
    public decimal SaldoInicial { get; set; }

    // Alerta cuando el saldo cae por debajo de este valor
    public decimal? AlertaSaldo { get; set; }

    public bool Activa { get; set; } = true;

    // Saldo calculado (no persistido) — se calcula en el servicio
    public decimal SaldoActual { get; set; }
}
