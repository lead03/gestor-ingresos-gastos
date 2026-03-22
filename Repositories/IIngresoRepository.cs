using ControlGastos.Models;

namespace ControlGastos.Repositories;

public interface IIngresoRepository
{
    Task<List<Ingreso>> GetByMesAsync(int mes, int anio);
    Task<Ingreso?> GetByIdAsync(int id);
    Task<Ingreso?> GetByIdFullAsync(int id);
    Task AddAsync(Ingreso ingreso);
    Task UpdateAsync(Ingreso ingreso);
    Task DeleteAsync(int id);

    // Distribuciones
    Task AddDistribucionesAsync(int ingresoId, IEnumerable<IngresoDistribucion> distribuciones);
    Task DeleteDistribucionesByIngresoAsync(int ingresoId);

    // TiposIngreso
    Task<List<TipoIngreso>> GetTiposAsync();           // habilitados
    Task<List<TipoIngreso>> GetTodosTiposAsync();      // incluye deshabilitados
    Task<bool> TipoHasIngresosAsync(int id);
    Task AddTipoAsync(TipoIngreso tipo);
    Task UpdateTipoAsync(TipoIngreso tipo);
    Task SetTipoHabilitadoAsync(int id, bool habilitada);
    Task DeleteTipoAsync(int id);
}
