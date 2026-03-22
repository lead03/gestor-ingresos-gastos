using System.ComponentModel.DataAnnotations;

namespace ControlGastos.Models;

public class GastoItem
{
    public int Id { get; set; }
    public int Mes  { get; set; }
    public int Anio { get; set; }
    public int Dia  { get; set; }

    public int CategoriaId { get; set; }
    public CategoriaGasto Categoria { get; set; } = null!;

    [Required]
    public decimal Monto { get; set; }

    public bool     SeDivide { get; set; }
    public decimal? MiParte  { get; set; }

    [MaxLength(200)]
    public string? Descripcion { get; set; }

    public Moneda Moneda { get; set; } = Moneda.ARS;

    // Medio de pago: cuenta o tarjeta (mutuamente excluyentes)
    public int? CuentaId  { get; set; }
    public Cuenta?  Cuenta    { get; set; }

    public int? TarjetaId { get; set; }
    public Tarjeta? Tarjeta   { get; set; }

    public int? TarjetaCuotaId { get; set; }
    public TarjetaCuota? TarjetaCuota { get; set; }

    public ICollection<GastoParticipante> Participantes { get; set; } = new List<GastoParticipante>();

    // Nombre del medio de pago para mostrar
    public string MedioPagoNombre =>
        Cuenta  != null ? Cuenta.Nombre  :
        Tarjeta != null ? Tarjeta.Nombre :
        "—";

    /// <summary>Monto que impacta en el mes: cuota si es TC en cuotas, total si es contado.</summary>
    public decimal MontoMes => TarjetaCuota?.MontoCuota ?? Monto;

    /// <summary>Mi parte del monto del mes (proporciona cuota si hay TC dividida).</summary>
    public decimal MiParteMes
    {
        get
        {
            if (!SeDivide) return MontoMes;
            if (TarjetaCuota == null) return MiParte ?? Monto;
            if (Monto == 0) return 0;
            return (MiParte ?? Monto) / Monto * TarjetaCuota.MontoCuota;
        }
    }
}
