using System.ComponentModel.DataAnnotations;
using ControlGastos.Models;

namespace ControlGastos.ViewModels;

public class CuentaListVM
{
    public List<CuentaResumenVM> Cuentas { get; set; } = new();
    public decimal TotalSaldos => Cuentas.Sum(c => c.SaldoActual);
}

public class CuentaResumenVM
{
    public int        Id                 { get; set; }
    public string     Nombre             { get; set; } = "";
    public TipoCuenta Tipo               { get; set; }
    public decimal    SaldoInicial       { get; set; }
    public decimal    SaldoActual  { get; set; }
    public decimal?   AlertaSaldo  { get; set; }
    public bool       EnAlerta     => AlertaSaldo.HasValue && SaldoActual < AlertaSaldo.Value;
}

public class CuentaDetalleVM
{
    public Cuenta  Cuenta      { get; set; } = null!;
    public decimal SaldoActual { get; set; }
    public bool    EnAlerta    => Cuenta.AlertaSaldo.HasValue && SaldoActual < Cuenta.AlertaSaldo.Value;

    // Movimientos unificados ordenados por fecha desc
    public List<MovimientoCuentaVM> Movimientos { get; set; } = new();
}

public class MovimientoCuentaVM
{
    public DateTime Fecha       { get; set; }
    public string   Descripcion { get; set; } = "";
    public string   Categoria   { get; set; } = "";

    // "Gasto" | "Ingreso"
    public string   Tipo        { get; set; } = "";
    public decimal  Monto       { get; set; }

    // Positivo = ingreso, negativo = gasto
    public decimal  Impacto     => Tipo == "Ingreso" ? Monto : -Monto;
}

public class CuentaFormVM : IValidatableObject
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [MaxLength(60, ErrorMessage = "El nombre no puede superar 60 caracteres.")]
    public string Nombre { get; set; } = "";

    public TipoCuenta Tipo { get; set; } = TipoCuenta.Billetera;

    [Range(0, 9999999999.99, ErrorMessage = "El saldo inicial no puede ser negativo ni superar $ 9.999.999.999,99.")]
    public decimal SaldoInicial { get; set; }

    public decimal? AlertaSaldo { get; set; }
    public bool     Activa      { get; set; } = true;

    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        const decimal max = 9_999_999_999.99m;

        if (AlertaSaldo.HasValue)
        {
            if (AlertaSaldo.Value < 0)
                yield return new ValidationResult(
                    "El monto de alerta no puede ser negativo.", new[] { nameof(AlertaSaldo) });
            if (AlertaSaldo.Value > max)
                yield return new ValidationResult(
                    "El monto de alerta no puede superar $ 9.999.999.999,99.", new[] { nameof(AlertaSaldo) });
        }
    }
}
