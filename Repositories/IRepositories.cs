using ControlGastos.Models;

namespace ControlGastos.Repositories;

public interface IGastoRepository
{
    Task<List<GastoItem>> GetByMesAsync(int mes, int anio);
    Task<GastoItem?> GetByIdAsync(int id);
    Task<List<CategoriaGasto>> GetCategoriasAsync();
    Task AddAsync(GastoItem gasto);
    Task UpdateAsync(GastoItem gasto);
    Task DeleteAsync(int id);
}

public interface IIngresoRepository
{
    Task<List<Ingreso>> GetByMesAsync(int mes, int anio);
    Task AddAsync(Ingreso ingreso);
    Task UpdateAsync(Ingreso ingreso);
    Task DeleteAsync(int id);
}

public interface ITarjetaRepository
{
    Task<List<Tarjeta>> GetAllAsync();
    Task<List<TarjetaCuota>> GetCuotasByMesAsync(int mes, int anio);
    Task<List<TarjetaCuota>> GetCuotasActivasAsync();
    Task<TarjetaCuota?> GetCuotaByIdAsync(int id);
    Task AddCuotaAsync(TarjetaCuota cuota);
    Task UpdateCuotaAsync(TarjetaCuota cuota);
    Task<List<TarjetaCuota>> GetCuotasParaAvanzarAsync(int tarjetaId, int mes, int anio);
    Task<bool> ExisteCuotaAsync(int tarjetaId, string comercio, int mes, int anio);
}

public interface IDeudaRepository
{
    Task<List<Deuda>> GetAllAsync();
    Task<Deuda?> GetByIdAsync(int id);
    Task AddAsync(Deuda deuda);
    Task UpdateAsync(Deuda deuda);
    Task DeleteAsync(int id);
}

public interface ICuentaRepository
{
    Task<List<Cuenta>> GetByMesAsync(int mes, int anio);
}
