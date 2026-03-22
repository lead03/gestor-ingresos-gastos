using System.ComponentModel.DataAnnotations;

namespace ControlGastos.Models;

public class TipoIngreso
{
    public int Id { get; set; }

    [Required, MaxLength(80)]
    public string Nombre { get; set; } = "";

    public bool Habilitada { get; set; } = true;

    public ICollection<Ingreso> Ingresos { get; set; } = new List<Ingreso>();
}
