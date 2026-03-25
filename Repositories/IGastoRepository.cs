using ControlGastos.Models;
using ControlGastos.ViewModels;

namespace ControlGastos.Repositories;

public interface IGastoRepository
{
    Task<List<GastoItem>> GetByMesAsync(int mes, int anio);
    Task<GastoItem?> GetByIdAsync(int id);
    Task<GastoItem?> GetByIdWithParticipantesAsync(int id);
    Task<List<CategoriaGasto>> GetCategoriasAsync();
    Task<List<CategoriaGasto>> GetTodasCategoriasAsync();   // incluye deshabilitadas
    Task<bool> CategoriaHasGastosAsync(int id);
    Task AddCategoriaAsync(CategoriaGasto cat);
    Task UpdateCategoriaAsync(CategoriaGasto cat);
    Task SetHabilitadaAsync(int id, bool habilitada);
    Task DeleteCategoriaAsync(int id);
    Task AddAsync(GastoItem gasto);
    Task UpdateAsync(GastoItem gasto);
    Task DeleteAsync(int id);
}

public interface IGastoParticipanteRepository
{
    Task<List<GastoParticipante>> GetByGastoAsync(int gastoItemId);
    Task<List<GastoParticipante>> GetByPersonaAsync(int personaId);
    /// <summary>
    /// Igual que GetByPersonaAsync pero con los gastos de TC expandidos por cuota:
    /// cada cuota genera su propia entrada con el mes de cierre y el monto proporcional.
    /// </summary>
    Task<List<PersonaParticipacionVM>> GetExpandedByPersonaAsync(int personaId);
    Task AddRangeAsync(IEnumerable<GastoParticipante> participantes);
    Task DeleteByGastoAsync(int gastoItemId);
}
