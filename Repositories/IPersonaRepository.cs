using ControlGastos.Models;

namespace ControlGastos.Repositories;

public interface IPersonaRepository
{
    Task<List<Persona>> GetAllAsync();
    Task<Persona?> GetByIdAsync(int id);
    Task<Persona?> GetByIdWithDetalleAsync(int id);
    Task AddAsync(Persona persona);
    Task UpdateAsync(Persona persona);
    Task DeleteAsync(int id);
}
