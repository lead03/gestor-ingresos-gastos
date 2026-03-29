using ControlGastos.Models;
using ControlGastos.ViewModels;

namespace ControlGastos.Repositories;

public interface ICategoriaGastoRepository
{
    Task<List<CategoriaGastoVM>> GetAllWithCountAsync();
    Task<CategoriaGasto?> GetByIdAsync(int id);
    Task AddAsync(CategoriaGasto entidad);
    Task UpdateAsync(CategoriaGasto entidad);
    Task DeleteAsync(CategoriaGasto entidad);
    Task<bool> TieneVinculadosAsync(int id);
}
