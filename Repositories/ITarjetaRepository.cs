using ControlGastos.Models;

namespace ControlGastos.Repositories;

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
