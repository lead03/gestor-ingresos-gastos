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

    // Fechas mensuales
    Task<TarjetaFechaMensual?> GetFechaMensualAsync(int tarjetaId, int mes, int anio);
    Task<List<TarjetaFechaMensual>> GetFechasMensualesByMesAsync(int mes, int anio);
    Task UpsertFechaMensualAsync(TarjetaFechaMensual fecha);

    // Cuotas de una tarjeta cuyos gastos se compraron en un mes/año dado (para recalcular)
    Task<List<TarjetaCuota>> GetCuotasByTarjetaYCompraAsync(int tarjetaId, int mesFechaCompra, int anioFechaCompra);

    Task ActualizarGastoIdCuotasAsync(int tarjetaId, DateTime fechaCompra, decimal montoTotal, int totalCuotas, int gastoItemId);
    Task<List<TarjetaCuota>> GetCuotasParaRecalcularAsync(int tarjetaId, int mesDesde, int anioDesde);
}
