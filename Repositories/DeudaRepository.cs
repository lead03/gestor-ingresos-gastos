using ControlGastos.Data;
using ControlGastos.Models;
using Microsoft.EntityFrameworkCore;

namespace ControlGastos.Repositories;

public class DeudaRepository(AppDbContext db) : IDeudaRepository
{
    // ── Deudas ───────────────────────────────────────────────────────

    /// Incluye Cuotas para que el cálculo de saldo sea correcto en cuentas de crédito.
    public Task<List<Deuda>> GetAllAsync() =>
        db.Deudas
          .Include(d => d.Persona)
          .Include(d => d.Cuotas)
          .OrderByDescending(d => d.Fecha)
          .ToListAsync();

    public Task<Deuda?> GetByIdAsync(int id) =>
        db.Deudas.FindAsync(id).AsTask();

    public async Task AddAsync(Deuda deuda)
    {
        db.Deudas.Add(deuda);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Deuda deuda)
    {
        db.Deudas.Update(deuda);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var d = await db.Deudas.FindAsync(id);
        if (d != null)
        {
            db.Deudas.Remove(d);
            await db.SaveChangesAsync();
        }
    }

    // ── Auto-generadas desde gastos ──────────────────────────────────

    public async Task DeleteByGastoItemIdAsync(int gastoItemId)
    {
        var d = await db.Deudas.FirstOrDefaultAsync(x => x.GastoItemId == gastoItemId);
        if (d != null) { db.Deudas.Remove(d); await db.SaveChangesAsync(); }
    }

    // ── Cuotas mensuales ─────────────────────────────────────────────

    public Task<List<DeudaCuota>> GetCuotasByMesAsync(int mes, int anio) =>
        db.DeudaCuotas
          .Include(c => c.Deuda)
          .Where(c => c.Mes == mes && c.Anio == anio)
          .OrderBy(c => c.Deuda.NombrePersona)
          .ToListAsync();

    public Task<DeudaCuota?> GetCuotaByIdAsync(int id) =>
        db.DeudaCuotas.FindAsync(id).AsTask();

    public async Task AddCuotaAsync(DeudaCuota cuota)
    {
        db.DeudaCuotas.Add(cuota);
        await db.SaveChangesAsync();
    }

    public async Task UpdateCuotaAsync(DeudaCuota cuota)
    {
        db.DeudaCuotas.Update(cuota);
        await db.SaveChangesAsync();
    }

    public async Task DeleteCuotaAsync(int id)
    {
        var c = await db.DeudaCuotas.FindAsync(id);
        if (c != null) { db.DeudaCuotas.Remove(c); await db.SaveChangesAsync(); }
    }

    public Task<bool> ExisteCuotaEnMesAsync(int deudaId, int mes, int anio) =>
        db.DeudaCuotas.AnyAsync(c => c.DeudaId == deudaId
                                  && c.Mes      == mes
                                  && c.Anio     == anio);
}
