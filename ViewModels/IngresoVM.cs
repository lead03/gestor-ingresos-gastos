using System.ComponentModel.DataAnnotations;
using ControlGastos.Models;

namespace ControlGastos.ViewModels;

public class DistribucionFormVM
{
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione una cuenta válida.")]
    public int CuentaId { get; set; }

    [Range(0.01, 9999999999.99, ErrorMessage = "El monto de cada cuenta debe ser mayor a $0.")]
    public decimal Monto { get; set; }
}

public class IngresoFormVM : IValidatableObject
{
    public int Id { get; set; }

    [Range(1, 12, ErrorMessage = "Mes inválido.")]
    public int Mes  { get; set; } = DateTime.Today.Month;

    [Range(2000, 2100, ErrorMessage = "Año inválido.")]
    public int Anio { get; set; } = DateTime.Today.Year;

    [Range(1, 31, ErrorMessage = "Día inválido.")]
    public int Dia  { get; set; } = DateTime.Today.Day;

    [Required(ErrorMessage = "Debe seleccionar un tipo de ingreso.")]
    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un tipo de ingreso.")]
    public int TipoIngresoId { get; set; }

    public Moneda Moneda { get; set; } = Moneda.ARS;

    [Range(0.01, 9999999999.99,
        ErrorMessage = "El monto debe ser mayor a $0 y menor a $9.999.999.999,99.")]
    public decimal Monto { get; set; }

    [MaxLength(100, ErrorMessage = "La descripción no puede superar los 100 caracteres.")]
    public string? Descripcion { get; set; }

    public List<DistribucionFormVM> Distribuciones { get; set; } = new();

    // Listas para selects
    public List<TipoIngreso>     Tipos   { get; set; } = new();
    public List<CuentaResumenVM> Cuentas { get; set; } = new();

    public bool HayCuentas => Cuentas.Any();

    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        // Fecha válida
        bool fechaValida = true;
        try { _ = new DateTime(Anio, Mes, Dia); }
        catch { fechaValida = false; }

        if (!fechaValida)
        {
            yield return new ValidationResult("La fecha ingresada no es válida.", new[] { nameof(Dia) });
            yield break;
        }

        if (new DateTime(Anio, Mes, Dia) > DateTime.Today.AddDays(1))
            yield return new ValidationResult("La fecha no puede ser futura.", new[] { nameof(Dia) });

        // Distribuciones
        if (!Distribuciones.Any())
        {
            yield return new ValidationResult("Debe asignar al menos una cuenta donde acreditar el ingreso.");
            yield break;
        }

        var ids = Distribuciones.Select(d => d.CuentaId).ToList();
        if (ids.Distinct().Count() != ids.Count)
            yield return new ValidationResult("No puede asignar la misma cuenta más de una vez.");

        if (Distribuciones.Any(d => d.Monto <= 0))
            yield return new ValidationResult("Cada cuenta debe tener un monto mayor a $0.");

        var suma = Distribuciones.Sum(d => d.Monto);
        if (Math.Abs(suma - Monto) > 0.01m)
            yield return new ValidationResult(
                $"La suma de las cuentas (${suma:N2}) no coincide con el monto total (${Monto:N2}).");
    }
}

public class IngresoListVM
{
    public int  Mes  { get; set; }
    public int  Anio { get; set; }
    public List<Ingreso> Items { get; set; } = new();
    public List<(string NombreTipo, decimal Total)> TotalesPorTipo { get; set; } = new();
    public decimal Total => TotalesPorTipo.Sum(x => x.Total);
}
