namespace ControlGastos.Models;

public class PagoPersona
{
    public int      Id          { get; set; }
    public int      PersonaId   { get; set; }
    public Persona? Persona     { get; set; }
    public int?     CuentaId    { get; set; }
    public Cuenta?  Cuenta      { get; set; }
    public decimal  Monto       { get; set; }
    public Moneda   Moneda      { get; set; } = Moneda.ARS;
    public DateTime Fecha       { get; set; }
    public string?  Descripcion { get; set; }
}
