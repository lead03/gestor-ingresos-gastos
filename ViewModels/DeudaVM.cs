using ControlGastos.Models;

namespace ControlGastos.ViewModels;

public class DeudaFormVM
{
    public int Id { get; set; }
    public string Persona { get; set; } = "";
    public decimal Monto { get; set; }
    public DateTime Fecha { get; set; } = DateTime.Today;
    public string? Descripcion { get; set; }
    public string Direccion { get; set; } = "MeDeben";
    public string Estado { get; set; } = "Activa";
    public decimal? MontoPagado { get; set; }
}

public class DeudaListVM
{
    public List<Deuda> MeDeben { get; set; } = new();
    public List<Deuda> LeDebo { get; set; } = new();
    public decimal TotalMeDeben { get; set; }
    public decimal TotalLeDebo { get; set; }
    public decimal Neto => TotalMeDeben - TotalLeDebo;
}
