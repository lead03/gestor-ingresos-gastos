using System.ComponentModel.DataAnnotations;

namespace ControlGastos.Models;

public class Tarjeta
{
    public int Id { get; set; }

    [Required, MaxLength(60)]
    public string Nombre { get; set; } = "";   // "Galicia - Visa", "Santander - Visa", etc.

    [MaxLength(30)]
    public string Banco { get; set; } = "";

    public int? RedTarjetaId { get; set; }
    public RedTarjeta? RedTarjeta { get; set; }

    public int DiaCierre { get; set; }
    public int DiaVencimiento { get; set; }

    public decimal? LimiteCredito { get; set; }

    public ICollection<TarjetaCuota>        Cuotas          { get; set; } = new List<TarjetaCuota>();
    public ICollection<TarjetaFechaMensual> FechasMensuales { get; set; } = new List<TarjetaFechaMensual>();
}
