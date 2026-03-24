using System.ComponentModel.DataAnnotations;

namespace ControlGastos.Models;

public class TipoEntidad
{
    public int Id { get; set; }

    [Required, MaxLength(40)]
    public string Nombre { get; set; } = "";

    public ICollection<ConfigOpcion> Opciones { get; set; } = new List<ConfigOpcion>();
}
