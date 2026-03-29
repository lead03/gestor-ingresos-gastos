using ControlGastos.Models;
using ControlGastos.ViewModels;

namespace ControlGastos.Repositories;

public interface IBilleteraRepository
{
    Task<List<BilleteraVM>> GetAllWithCountAsync();
    Task<Billetera?> GetByIdAsync(int id);
    Task AddAsync(Billetera entidad);
    Task UpdateAsync(Billetera entidad);
    Task DeleteAsync(Billetera entidad);
    Task<bool> TieneVinculadosAsync(int id);
}
