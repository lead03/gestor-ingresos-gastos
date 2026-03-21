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
}
