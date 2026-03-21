namespace ControlGastos.Models;

public class GastoParticipante
{
    public int Id { get; set; }

    public int      GastoItemId { get; set; }
    public GastoItem GastoItem  { get; set; } = null!;

    public int?     PersonaId   { get; set; }
    public Persona? Persona     { get; set; }

    public TipoParticipante Tipo { get; set; } = TipoParticipante.Yo;

    public string? Descripcion  { get; set; }
    public string  NombreMostrar => Persona?.Nombre ?? Descripcion ?? Tipo.ToString();

    public decimal Monto { get; set; }

    public bool GeneraDeuda => Tipo == TipoParticipante.Persona && PersonaId.HasValue;
}
