using ControlGastos.Models;

namespace ControlGastos.Repositories;

public interface IDeudaRepository
{
    Task<List<Deuda>> GetAllAsync();
    Task<Deuda?> GetByIdAsync(int id);
    Task AddAsync(Deuda deuda);
    Task UpdateAsync(Deuda deuda);
    Task DeleteAsync(int id);
}
