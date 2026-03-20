using ControlGastos.Models;

namespace ControlGastos.ViewModels;

public class EfectivoVM
{
    public int Mes { get; set; }
    public int Anio { get; set; }
    public List<EfectivoDesglose> Desgloses { get; set; } = new();
    public decimal Total => Desgloses.Sum(d => d.Total);
    public static int[] Denominaciones = { 10, 20, 50, 100, 200, 500, 1000, 2000 };
}
