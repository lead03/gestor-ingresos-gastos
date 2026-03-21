namespace ControlGastos.Models;

/// <summary>
/// Fechas de cierre y vencimiento específicas para un mes/año de una tarjeta.
/// Si no existe registro, se usan los valores por defecto de Tarjeta.DiaCierre / DiaVencimiento.
/// </summary>
public class TarjetaFechaMensual
{
    public int Id { get; set; }

    public int TarjetaId { get; set; }
    public Tarjeta Tarjeta { get; set; } = null!;

    public int Mes  { get; set; }
    public int Anio { get; set; }

    public int DiaCierre      { get; set; }
    public int DiaVencimiento { get; set; }
}
