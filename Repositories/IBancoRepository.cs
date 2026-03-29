using ControlGastos.Models;
using ControlGastos.ViewModels;

namespace ControlGastos.Repositories;

public interface IBancoRepository
{
    Task<List<BancoVM>> GetAllWithCountAsync();
    Task<Banco?> GetByIdAsync(int id);
    Task AddAsync(Banco entidad);
    Task UpdateAsync(Banco entidad);
    Task DeleteAsync(Banco entidad);
    Task<bool> TieneVinculadosAsync(int id);
}
