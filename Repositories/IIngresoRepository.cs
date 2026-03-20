using ControlGastos.Models;

namespace ControlGastos.Repositories;

public interface IIngresoRepository
{
    Task<List<Ingreso>> GetByMesAsync(int mes, int anio);
    Task<Ingreso?> GetByIdAsync(int id);
    Task AddAsync(Ingreso ingreso);
    Task UpdateAsync(Ingreso ingreso);
    Task DeleteAsync(int id);
}
