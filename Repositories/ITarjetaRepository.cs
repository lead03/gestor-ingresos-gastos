using ControlGastos.Models;

namespace ControlGastos.Repositories;

public interface ITarjetaRepository
{
    Task<List<Tarjeta>> GetAllAsync();
    Task<Tarjeta?> GetByIdAsync(int id);
    Task<List<TarjetaCuota>> GetCuotasByMesAsync(int mes, int anio);
    Task<List<TarjetaCuota>> GetCuotasActivasAsync();
    Task<TarjetaCuota?> GetCuotaByIdAsync(int id);
    Task AddCuotaAsync(TarjetaCuota cuota);
    Task UpdateCuotaAsync(TarjetaCuota cuota);
    Task<List<TarjetaCuota>> GetCuotasParaAvanzarAsync(int tarjetaId, int mes, int anio);
    Task<bool> ExisteCuotaAsync(int tarjetaId, string comercio, int mes, int anio);

    // CRUD tarjetas
    Task AddTarjetaAsync(Tarjeta t);
    Task UpdateTarjetaAsync(Tarjeta t);
    Task DeleteTarjetaAsync(int id);
    Task<bool> TieneCuotasAsync(int tarjetaId);
    Task DeleteCuotasGrupoAsync(int tarjetaId, DateTime fechaCompra, decimal montoTotal, int totalCuotas);
}
