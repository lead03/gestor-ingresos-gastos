using System.ComponentModel.DataAnnotations;
using ControlGastos.Models;

namespace ControlGastos.ViewModels;

// ── Formulario alta/edición de Deuda ─────────────────────────────────────
public class DeudaFormVM
{
    public int    Id        { get; set; }
    public int?   PersonaId { get; set; }

    [Required(ErrorMessage = "El nombre de la persona es obligatorio.")]
    [MaxLength(80)]
    public string NombrePersona { get; set; } = "";

    // Permite 0 para cuentas de crédito auto-creadas con saldo inicial nulo.
    [Range(0, double.MaxValue, ErrorMessage = "El monto no puede ser negativo.")]
    public decimal Monto { get; set; }

    public DateTime Fecha { get; set; } = DateTime.Today;

    [MaxLength(200)]
    public string? Descripcion { get; set; }

    public DireccionDeuda Direccion    { get; set; } = DireccionDeuda.MeDeben;
    public EstadoDeuda    Estado       { get; set; } = EstadoDeuda.Activa;
    public decimal?       MontoPagado { get; set; }
    public bool           AceptaCuotas { get; set; }
}

// ── Formulario alta/edición de DeudaCuota ────────────────────────────────
public class DeudaCuotaFormVM
{
    public int Id { get; set; }

    [Required]
    public int DeudaId { get; set; }

    public int Mes  { get; set; } = DateTime.Today.Month;
    public int Anio { get; set; } = DateTime.Today.Year;

    [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a $0.")]
    public decimal Monto { get; set; }

    [MaxLength(200)]
    public string? Descripcion { get; set; }

    public decimal MontoPagado { get; set; }
}

// ── Lista (vista principal de Deudas) ────────────────────────────────────
public class DeudaListVM
{
    public List<Deuda> MeDeben { get; set; } = new();
    public List<Deuda> LeDebo  { get; set; } = new();

    public decimal TotalMeDeben { get; set; }
    public decimal TotalLeDebo  { get; set; }
    public decimal Neto         => TotalMeDeben - TotalLeDebo;

    /// Cuotas del mes/año seleccionado en el selector global.
    public List<DeudaCuota> CuotasMes      { get; set; } = new();
    public decimal          TotalCuotasMes { get; set; }

    /// Deudas con AceptaCuotas=true disponibles para agregar cuotas.
    public List<Deuda> DeudasConCuotas { get; set; } = new();

    public int Mes  { get; set; }
    public int Anio { get; set; }
}
