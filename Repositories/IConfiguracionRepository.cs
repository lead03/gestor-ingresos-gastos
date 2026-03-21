using ControlGastos.Models;

namespace ControlGastos.Repositories;

public interface IConfiguracionRepository
{
    Task<List<ConfigOpcion>> GetByTipoAsync(string tipo);
    Task<List<ConfigOpcion>> GetAllAsync();
    Task AddAsync(ConfigOpcion opcion);
    Task DeleteAsync(int id);
}
