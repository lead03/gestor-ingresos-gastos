using ControlGastos.Models;

namespace ControlGastos.Repositories;

public interface ICuentaRepository
{
    Task<List<Cuenta>> GetByMesAsync(int mes, int anio);
}
