using System.ComponentModel.DataAnnotations;

namespace ControlGastos.Models;

// ── Catálogo de categorías de gastos ──────────────────────────────────────
public class CategoriaGasto
{
    public int Id { get; set; }
    [Required, MaxLength(80)]
    public string Nombre { get; set; } = "";
    // "Fijo" | "Variable"
    public string Tipo { get; set; } = "Variable";
    public ICollection<GastoItem> Gastos { get; set; } = new List<GastoItem>();
}

// ── Un gasto individual (puede ser parte de un día con varios ítems) ───────
public class GastoItem
{
    public int Id { get; set; }
    public int Mes { get; set; }
    public int Anio { get; set; }
    public int Dia { get; set; }

    public int CategoriaId { get; set; }
    public CategoriaGasto Categoria { get; set; } = null!;

    [Required]
    public decimal Monto { get; set; }

    // Si el monto se divide entre varias personas
    public bool SeDivide { get; set; }
    public decimal? MontoDividido { get; set; }   // monto que corresponde al usuario
    public int? CantidadPersonas { get; set; }

    [MaxLength(200)]
    public string? Descripcion { get; set; }

    // Medio de pago: "Efectivo" | "Galicia" | "Santander" | etc.
    [MaxLength(50)]
    public string? MedioPago { get; set; }

    // Si corresponde a una cuota de tarjeta
    public int? TarjetaCuotaId { get; set; }
    public TarjetaCuota? TarjetaCuota { get; set; }
}

// ── Ingreso ───────────────────────────────────────────────────────────────
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
    [MaxLength(50)]
    public string? Fuente { get; set; }   // banco/cuenta de origen
}

// ── Tarjeta de crédito ────────────────────────────────────────────────────
public class Tarjeta
{
    public int Id { get; set; }
    [Required, MaxLength(60)]
    public string Nombre { get; set; } = "";   // "Galicia - Visa", "Santander - Visa", etc.
    [MaxLength(30)]
    public string Banco { get; set; } = "";
    [MaxLength(20)]
    public string Red { get; set; } = "VISA";  // VISA | Mastercard | AMEX

    // Día del mes en que cierra/vence
    public int DiaCierre { get; set; }
    public int DiaVencimiento { get; set; }

    public ICollection<TarjetaCuota> Cuotas { get; set; } = new List<TarjetaCuota>();
}

// ── Compra en cuotas con tarjeta ──────────────────────────────────────────
public class TarjetaCuota
{
    public int Id { get; set; }
    public int TarjetaId { get; set; }
    public Tarjeta Tarjeta { get; set; } = null!;

    [Required, MaxLength(150)]
    public string Comercio { get; set; } = "";
    public DateTime FechaCompra { get; set; }
    public decimal MontoTotal { get; set; }
    public int TotalCuotas { get; set; }
    public decimal MontoCuota { get; set; }

    // Cuota de cierre actual (mes en que aparece en el resumen)
    public int MesCierre { get; set; }
    public int AnioCierre { get; set; }

    // Cuotas restantes (calculadas)
    public int CuotasRestantes => TotalCuotas - CuotasPagadas;
    public int CuotasPagadas { get; set; }

    // "NO" | "DEBE"
    [MaxLength(10)]
    public string PagaParte { get; set; } = "NO";
    public decimal? MontoPagoOtro { get; set; }

    public ICollection<GastoItem> GastosAsociados { get; set; } = new List<GastoItem>();
}

// ── Cuentas bancarias / billeteras ───────────────────────────────────────
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

// ── Deudas (me deben / le debo) ──────────────────────────────────────────
public class Deuda
{
    public int Id { get; set; }
    [Required, MaxLength(80)]
    public string Persona { get; set; } = "";
    public decimal Monto { get; set; }
    public DateTime Fecha { get; set; }
    [MaxLength(200)]
    public string? Descripcion { get; set; }
    // "MeDeben" | "LeDebo"
    public string Direccion { get; set; } = "MeDeben";
    // "Activa" | "Pagada" | "Parcial"
    public string Estado { get; set; } = "Activa";
    public decimal? MontoPagado { get; set; }
}

// ── Efectivo en mano (desglose de billetes) ───────────────────────────────
public class EfectivoDesglose
{
    public int Id { get; set; }
    public int Mes { get; set; }
    public int Anio { get; set; }
    public int Denominacion { get; set; }  // 10, 20, 50, 100, 200, 500, 1000, 2000
    public int Cantidad { get; set; }
    public decimal Total => Denominacion * Cantidad;
}
