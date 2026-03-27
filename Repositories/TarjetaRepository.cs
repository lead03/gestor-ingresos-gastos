using ControlGastos.Data;
using ControlGastos.Models;
using Microsoft.EntityFrameworkCore;

namespace ControlGastos.Repositories;

public class TarjetaRepository(AppDbContext db) : ITarjetaRepository
{
    public Task<List<Tarjeta>> GetAllAsync() =>
        db.Tarjetas.Include(t => t.RedTarjeta).ToListAsync();

    public Task<Tarjeta?> GetByIdAsync(int id) =>
        db.Tarjetas.Include(t => t.RedTarjeta).FirstOrDefaultAsync(t => t.Id == id);

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

    // ── Fechas mensuales ───────────────────────────────────────────────
    public Task<TarjetaFechaMensual?> GetFechaMensualAsync(int tarjetaId, int mes, int anio) =>
        db.TarjetaFechasMensuales
          .FirstOrDefaultAsync(f => f.TarjetaId == tarjetaId && f.Mes == mes && f.Anio == anio);

    public Task<List<TarjetaFechaMensual>> GetFechasMensualesByMesAsync(int mes, int anio) =>
        db.TarjetaFechasMensuales
          .Where(f => f.Mes == mes && f.Anio == anio)
          .ToListAsync();

    public async Task UpsertFechaMensualAsync(TarjetaFechaMensual fecha)
    {
        var existente = await db.TarjetaFechasMensuales
            .FirstOrDefaultAsync(f => f.TarjetaId == fecha.TarjetaId
                                   && f.Mes       == fecha.Mes
                                   && f.Anio      == fecha.Anio);
        if (existente == null)
        {
            db.TarjetaFechasMensuales.Add(fecha);
        }
        else
        {
            existente.DiaCierre      = fecha.DiaCierre;
            existente.DiaVencimiento = fecha.DiaVencimiento;
        }
        await db.SaveChangesAsync();
    }

    public Task<List<TarjetaCuota>> GetCuotasByTarjetaYCompraAsync(int tarjetaId, int mesFechaCompra, int anioFechaCompra) =>
        db.TarjetaCuotas
          .Where(tc => tc.TarjetaId == tarjetaId
                    && tc.FechaCompra.Month == mesFechaCompra
                    && tc.FechaCompra.Year  == anioFechaCompra)
          .ToListAsync();

    public async Task ActualizarGastoIdCuotasAsync(int tarjetaId, DateTime fechaCompra, decimal montoTotal, int totalCuotas, int gastoItemId)
    {
        var cuotas = await db.TarjetaCuotas
            .Where(tc => tc.TarjetaId   == tarjetaId
                      && tc.FechaCompra == fechaCompra
                      && tc.MontoTotal  == montoTotal
                      && tc.TotalCuotas == totalCuotas)
            .ToListAsync();
        foreach (var c in cuotas)
            c.GastoItemId = gastoItemId;
        await db.SaveChangesAsync();
    }

    public Task<List<TarjetaCuota>> GetCuotasParaRecalcularAsync(int tarjetaId, int mesDesde, int anioDesde) =>
        db.TarjetaCuotas
          .Where(tc => tc.TarjetaId == tarjetaId
                    && (tc.FechaCompra.Year > anioDesde
                        || (tc.FechaCompra.Year == anioDesde && tc.FechaCompra.Month >= mesDesde)))
          .ToListAsync();
}
