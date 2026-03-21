using System.ComponentModel.DataAnnotations;
using ControlGastos.Models;

namespace ControlGastos.ViewModels;

public class DeudaFormVM
{
    public int            Id            { get; set; }
    public int?           PersonaId     { get; set; }

    [Required(ErrorMessage = "El nombre de la persona es obligatorio.")]
    [MaxLength(80)]
    public string         NombrePersona { get; set; } = "";

    [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a $0.")]
    public decimal        Monto         { get; set; }

    public DateTime       Fecha         { get; set; } = DateTime.Today;

    [MaxLength(200)]
    public string?        Descripcion   { get; set; }

    public DireccionDeuda Direccion     { get; set; } = DireccionDeuda.MeDeben;
    public EstadoDeuda    Estado        { get; set; } = EstadoDeuda.Activa;
    public decimal?       MontoPagado  { get; set; }
}

public class DeudaListVM
{
    public List<Deuda> MeDeben      { get; set; } = new();
    public List<Deuda> LeDebo       { get; set; } = new();
    public decimal     TotalMeDeben { get; set; }
    public decimal     TotalLeDebo  { get; set; }
    public decimal     Neto         => TotalMeDeben - TotalLeDebo;
}
