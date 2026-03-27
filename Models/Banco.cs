using System.ComponentModel.DataAnnotations;

namespace ControlGastos.Models;

public class Banco
{
    public int Id { get; set; }

    [Required, MaxLength(60)]
    public string Nombre { get; set; } = "";

    public int Orden { get; set; }

    public ICollection<Cuenta> Cuentas { get; set; } = new List<Cuenta>();
}
