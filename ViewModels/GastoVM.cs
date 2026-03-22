using System.ComponentModel.DataAnnotations;
using ControlGastos.Models;

namespace ControlGastos.ViewModels;

public class GastoFormVM : IValidatableObject
{
    public int Id   { get; set; }
    public int Mes  { get; set; } = DateTime.Today.Month;
    public int Anio { get; set; } = DateTime.Today.Year;
    public int Dia  { get; set; } = DateTime.Today.Day;

    [Required(ErrorMessage = "Debe seleccionar una categoría.")]
    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una categoría.")]
    public int CategoriaId { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a $0.")]
    public decimal Monto { get; set; }

    public bool    SeDivide   { get; set; }

    [MaxLength(100, ErrorMessage = "La descripción no puede superar los 100 caracteres.")]
    public string? Descripcion { get; set; }

    // Medio de pago
    public int? CuentaId  { get; set; }
    public int? TarjetaId { get; set; }

    // Validación: al menos uno debe tener valor
    public bool TieneMedioPago => CuentaId.HasValue || TarjetaId.HasValue;

    // Cuotas — solo aplica cuando TarjetaId tiene valor
    [Range(1, 48, ErrorMessage = "Las cuotas deben ser entre 1 y 48.")]
    public int CantidadCuotas { get; set; } = 1;

    [MaxLength(3)]
    public string Moneda { get; set; } = "ARS";

    public int? TarjetaCuotaId { get; set; }

    public List<ParticipanteFormVM> Participantes { get; set; } = new();

    // Listas para selects
    public List<CategoriaGasto>  Categorias { get; set; } = new();
    public List<TarjetaCuota>    Cuotas     { get; set; } = new();
    public List<Persona>         Personas   { get; set; } = new();
    public List<CuentaResumenVM> Cuentas    { get; set; } = new();
    public List<Tarjeta>         Tarjetas   { get; set; } = new();

    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (CuentaId.HasValue && TarjetaId.HasValue)
            yield return new ValidationResult(
                "No se puede seleccionar cuenta y tarjeta al mismo tiempo.",
                new[] { nameof(CuentaId) });

        if (TarjetaId.HasValue && CantidadCuotas < 1)
            yield return new ValidationResult(
                "La cantidad de cuotas debe ser al menos 1.",
                new[] { nameof(CantidadCuotas) });

        if (SeDivide)
        {
            if (!Participantes.Any())
                yield return new ValidationResult(
                    "Un gasto dividido debe tener al menos un participante.");

            if (Participantes.Any(p => p.Tipo == "Persona" && !p.PersonaId.HasValue))
                yield return new ValidationResult(
                    "Hay participantes tipo Persona sin asignar.");

            if (Math.Abs(Participantes.Sum(p => p.Monto) - Monto) > 0.01m)
                yield return new ValidationResult(
                    "La suma de participantes no coincide con el monto total.");
        }
    }
}

public class ParticipanteFormVM
{
    public int     Id          { get; set; }
    public string  Tipo        { get; set; } = "Yo";
    public string? Descripcion { get; set; }
    public decimal Monto       { get; set; }
    public int?    PersonaId   { get; set; }
}

public class GastoListVM
{
    public int  Mes  { get; set; }
    public int  Anio { get; set; }
    public List<GastoItem> Items          { get; set; } = new();
    public decimal         TotalFijos     { get; set; }   // ARS only
    public decimal         TotalVariables { get; set; }   // ARS only
    public decimal         TotalFijosUsd     { get; set; }
    public decimal         TotalVariablesUsd { get; set; }
    public decimal         Total             => TotalFijos + TotalVariables;
    public decimal         TotalUsd          => TotalFijosUsd + TotalVariablesUsd;
    public decimal?        CotizacionDolar   { get; set; }
    public string? FuenteCotizacion    { get; set; }
    public string? FuenteCotizacionTipo { get; set; }
    public Dictionary<int, List<GastoItem>> PorDia { get; set; } = new();
}
