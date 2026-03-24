using System.ComponentModel.DataAnnotations;

namespace ControlGastos.Models;

public class TipoCategoriaGasto
{
    public int Id { get; set; }

    [Required, MaxLength(40)]
    public string Nombre { get; set; } = "";

    public ICollection<CategoriaGasto> Categorias { get; set; } = new List<CategoriaGasto>();
}
