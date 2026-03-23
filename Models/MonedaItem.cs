namespace ControlGastos.Models;

/// <summary>Fila de la tabla Monedas. Actúa como lookup: el Id coincide con el valor del enum Moneda.</summary>
public class MonedaItem
{
    public int    Id      { get; set; }
    public string Codigo  { get; set; } = "";   // "ARS", "USD"
    public string Nombre  { get; set; } = "";   // "Peso Argentino", "Dólar"
    public string Simbolo { get; set; } = "";   // "$", "U$D"
}
