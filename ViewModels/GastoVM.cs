using ControlGastos.Models;

namespace ControlGastos.ViewModels;

public class GastoFormVM
{
    public int Id { get; set; }
    public int Mes { get; set; } = DateTime.Today.Month;
    public int Anio { get; set; } = DateTime.Today.Year;
    public int Dia { get; set; } = DateTime.Today.Day;
    public int CategoriaId { get; set; }
    public decimal Monto { get; set; }
    public bool SeDivide { get; set; }
    public int? CantidadPersonas { get; set; }
    public decimal? MontoDividido { get; set; }
    public string? Descripcion { get; set; }
    public string? MedioPago { get; set; }
    public int? TarjetaCuotaId { get; set; }
    public List<CategoriaGasto> Categorias { get; set; } = new();
    public List<TarjetaCuota> Cuotas { get; set; } = new();
}

public class GastoListVM
{
    public int Mes { get; set; }
    public int Anio { get; set; }
    public List<GastoItem> Items { get; set; } = new();
    public decimal TotalFijos { get; set; }
    public decimal TotalVariables { get; set; }
    public decimal Total => TotalFijos + TotalVariables;

    // Agrupados por día
    public Dictionary<int, List<GastoItem>> PorDia { get; set; } = new();
}
