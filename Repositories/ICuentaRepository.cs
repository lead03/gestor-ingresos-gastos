using ControlGastos.Models;

namespace ControlGastos.Repositories;

public interface ICuentaRepository
{
    Task<List<Cuenta>> GetAllActivasAsync();
    Task<Cuenta?> GetByIdAsync(int id);
    Task AddAsync(Cuenta cuenta);
    Task UpdateAsync(Cuenta cuenta);
    Task DeleteAsync(int id);
    Task<List<GastoItem>>  GetGastosByCuentaAsync(int cuentaId);
    Task<List<Ingreso>>    GetIngresosByCuentaAsync(int cuentaId);
    Task<List<Ingreso>>    GetPagosDeudaByCuentaAsync(int cuentaId);
}
