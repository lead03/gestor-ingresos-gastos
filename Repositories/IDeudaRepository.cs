using ControlGastos.Models;

namespace ControlGastos.Repositories;

public interface IDeudaRepository
{
    Task<List<Deuda>> GetAllAsync();
    Task<Deuda?> GetByIdAsync(int id);
    Task AddAsync(Deuda deuda);
    Task UpdateAsync(Deuda deuda);
    Task DeleteAsync(int id);

    // ── Cuotas mensuales ────────────────────────────────────────────
    Task<List<DeudaCuota>> GetCuotasByMesAsync(int mes, int anio);
    Task<DeudaCuota?> GetCuotaByIdAsync(int id);
    Task AddCuotaAsync(DeudaCuota cuota);
    Task UpdateCuotaAsync(DeudaCuota cuota);
    Task DeleteCuotaAsync(int id);
    Task<bool> ExisteCuotaEnMesAsync(int deudaId, int mes, int anio);
}
