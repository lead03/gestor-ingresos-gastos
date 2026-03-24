using System.ComponentModel.DataAnnotations;

namespace ControlGastos.Models;

public class CategoriaGasto
{
    public int Id { get; set; }

    [Required, MaxLength(80)]
    public string Nombre { get; set; } = "";

    public int TipoId { get; set; }
    public TipoCategoriaGasto Tipo { get; set; } = null!;
    public bool Habilitada { get; set; } = true;

    public ICollection<GastoItem> Gastos { get; set; } = new List<GastoItem>();
}
