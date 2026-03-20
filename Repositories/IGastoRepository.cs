using ControlGastos.Models;

namespace ControlGastos.Repositories;

public interface IGastoRepository
{
    Task<List<GastoItem>> GetByMesAsync(int mes, int anio);
    Task<GastoItem?> GetByIdAsync(int id);
    Task<List<CategoriaGasto>> GetCategoriasAsync();
    Task AddAsync(GastoItem gasto);
    Task UpdateAsync(GastoItem gasto);
    Task DeleteAsync(int id);
}
