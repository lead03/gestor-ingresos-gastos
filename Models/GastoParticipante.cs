namespace ControlGastos.Models;

public class GastoParticipante
{
    public int Id { get; set; }

    public int GastoItemId { get; set; }
    public GastoItem GastoItem { get; set; } = null!;

    // Persona declarada (nullable — puede ser libre sin asignar)
    public int? PersonaId { get; set; }
    public Persona? Persona { get; set; }

    // "Yo" | "Persona" | "Pagado"
    public string Tipo { get; set; } = "Yo";

    // Nombre a mostrar (usa Persona.Nombre si está vinculada)
    public string? Descripcion { get; set; }
    public string NombreMostrar => Persona?.Nombre ?? Descripcion;

    public decimal Monto { get; set; }

    public bool GeneraDeuda => Tipo == "Persona" && PersonaId.HasValue;
}
