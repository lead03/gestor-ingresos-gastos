namespace ControlGastos.Helpers;

/// <summary>
/// Tipos de modal para _ModalConfirm.
/// Determina el color del header e ícono cuando se rediseñe visualmente.
/// </summary>
public static class TipoModal
{
    public const string Peligro     = "peligro";     // Acción irreversible (eliminar)
    public const string Alerta      = "alerta";      // Modificación con impacto
    public const string Informativo = "informativo"; // Solo informa, sin acción destructiva
    public const string Atencion    = "atencion";    // Advertencia no crítica
}
