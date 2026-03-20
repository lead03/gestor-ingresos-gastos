using ControlGastos.Data;
using ControlGastos.Models;
using Microsoft.EntityFrameworkCore;

namespace ControlGastos.Repositories;

public class TarjetaRepository(AppDbContext db) : ITarjetaRepository
{
    public Task<List<Tarjeta>> GetAllAsync() =>
        db.Tarjetas.ToListAsync();

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
}
