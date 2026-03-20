namespace ControlGastos.Models;

public class EfectivoDesglose
{
    public int Id { get; set; }
    public int Mes { get; set; }
    public int Anio { get; set; }

    // 10 | 20 | 50 | 100 | 200 | 500 | 1000 | 2000
    public int Denominacion { get; set; }

    public int Cantidad { get; set; }

    // Propiedad calculada — no se persiste en la DB
    public decimal Total => Denominacion * Cantidad;
}
