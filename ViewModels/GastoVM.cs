using ControlGastos.Models;
using System.ComponentModel.DataAnnotations;

namespace ControlGastos.ViewModels;

public class GastoFormVM : IValidatableObject
{
    public int Id { get; set; }
    public int Mes  { get; set; } = DateTime.Today.Month;
    public int Anio { get; set; } = DateTime.Today.Year;
    public int Dia  { get; set; } = DateTime.Today.Day;
    [Required(ErrorMessage = "Debe seleccionar una categoría.")]
    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una categoría.")]
    public int CategoriaId { get; set; }

    [Required(ErrorMessage = "El monto es obligatorio.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a $0.")]
    public decimal Monto { get; set; }

    public bool SeDivide { get; set; }
    public string? Descripcion { get; set; }
    public string? MedioPago { get; set; }
    public int? TarjetaCuotaId { get; set; }
    public List<ParticipanteFormVM> Participantes { get; set; } = new();
    public List<CategoriaGasto> Categorias { get; set; } = new();
    public List<TarjetaCuota> Cuotas { get; set; } = new();
    public List<Persona> Personas { get; set; } = new();

    // Validaciones cruzadas (las que dependen de otros campos)
    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (SeDivide)
        {
            if (!Participantes.Any())
                yield return new ValidationResult("Un gasto dividido debe tener al menos un participante.");

            if (Participantes.Any(p => p.Tipo == "Persona" && !p.PersonaId.HasValue))
                yield return new ValidationResult("Hay participantes tipo Persona sin asignar.");

            var totalPartes = Participantes.Sum(p => p.Monto);
            if (Math.Abs(totalPartes - Monto) > 0.01m)
                yield return new ValidationResult("La suma de participantes no coincide con el monto total.");
        }
    }
}

public class ParticipanteFormVM
{
    public int Id { get; set; }

    // "Yo" | "Persona" | "Pagado"
    public string Tipo { get; set; } = "Yo";

    public string  Descripcion { get; set; } = "";
    public decimal Monto       { get; set; }

    // Solo si Tipo == "Persona"
    public int? PersonaId { get; set; }
}

public class GastoListVM
{
    public int Mes  { get; set; }
    public int Anio { get; set; }
    public List<GastoItem> Items          { get; set; } = new();
    public decimal         TotalFijos     { get; set; }
    public decimal         TotalVariables { get; set; }
    public decimal         Total          => TotalFijos + TotalVariables;
    public Dictionary<int, List<GastoItem>> PorDia { get; set; } = new();
}
