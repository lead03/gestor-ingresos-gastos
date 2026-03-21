using ControlGastos.Data;
using ControlGastos.Models;
using Microsoft.EntityFrameworkCore;

namespace ControlGastos.Repositories;

public class TarjetaRepository(AppDbContext db) : ITarjetaRepository
{
    public Task<List<Tarjeta>> GetAllAsync() =>
        db.Tarjetas.ToListAsync();

    public Task<Tarjeta?> GetByIdAsync(int id) =>
        db.Tarjetas.FindAsync(id).AsTask();

    public Task<List<TarjetaCuota>> GetCuotasByMesAsync(int mes, int anio) =>
        db.TarjetaCuotas
          .Include(tc => tc.Tarjeta)
          .Where(tc => tc.MesCierre == mes && tc.AnioCierre == anio)
          .OrderBy(tc => tc.FechaCompra)
          .ToListAsync();

    public async Task<List<TarjetaCuota>> GetCuotasActivasAsync() =>
        (await db.TarjetaCuotas
                 .Include(tc => tc.Tarjeta)
                 .ToListAsync())
                 .Where(tc => tc.CuotasRestantes > 0)
                 .ToList();

    public Task<TarjetaCuota?> GetCuotaByIdAsync(int id) =>
        db.TarjetaCuotas.FindAsync(id).AsTask();

    public async Task AddCuotaAsync(TarjetaCuota cuota)
    {
        db.TarjetaCuotas.Add(cuota);
        await db.SaveChangesAsync();
    }

    public async Task UpdateCuotaAsync(TarjetaCuota cuota)
    {
        db.TarjetaCuotas.Update(cuota);
        await db.SaveChangesAsync();
    }

    public Task<List<TarjetaCuota>> GetCuotasParaAvanzarAsync(int tarjetaId, int mes, int anio) =>
        db.TarjetaCuotas
          .Where(tc => tc.TarjetaId == tarjetaId
                    && tc.MesCierre == mes
                    && tc.AnioCierre == anio)
          .ToListAsync();

    public Task<bool> ExisteCuotaAsync(int tarjetaId, string comercio, int mes, int anio) =>
        db.TarjetaCuotas.AnyAsync(x =>
            x.TarjetaId  == tarjetaId &&
            x.Comercio   == comercio  &&
            x.MesCierre  == mes       &&
            x.AnioCierre == anio);

    // ── CRUD Tarjetas ─────────────────────────────────────────────────
    public async Task AddTarjetaAsync(Tarjeta t)
    {
        db.Tarjetas.Add(t);
        await db.SaveChangesAsync();
    }

    public async Task UpdateTarjetaAsync(Tarjeta t)
    {
        db.Tarjetas.Update(t);
        await db.SaveChangesAsync();
    }

    public async Task DeleteTarjetaAsync(int id)
    {
        var t = await db.Tarjetas.FindAsync(id);
        if (t != null) { db.Tarjetas.Remove(t); await db.SaveChangesAsync(); }
    }

    public Task<bool> TieneCuotasAsync(int tarjetaId) =>
        db.TarjetaCuotas.AnyAsync(tc => tc.TarjetaId == tarjetaId);

    public async Task DeleteCuotasGrupoAsync(int tarjetaId, DateTime fechaCompra, decimal montoTotal, int totalCuotas)
    {
        var cuotas = await db.TarjetaCuotas
            .Where(tc => tc.TarjetaId   == tarjetaId
                      && tc.FechaCompra == fechaCompra
                      && tc.MontoTotal  == montoTotal
                      && tc.TotalCuotas == totalCuotas)
            .ToListAsync();
        db.TarjetaCuotas.RemoveRange(cuotas);
        await db.SaveChangesAsync();
    }
}
