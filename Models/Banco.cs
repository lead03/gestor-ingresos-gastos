using System.ComponentModel.DataAnnotations;

namespace ControlGastos.Models;

public class Banco
{
    public int Id { get; set; }

    [MaxLength(60)]
    public string Nombre { get; set; } = "";

    public ICollection<Cuenta> Cuentas { get; set; } = [];
}
