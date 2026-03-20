using ControlGastos.Data;
using ControlGastos.Models;
using Microsoft.EntityFrameworkCore;

namespace ControlGastos.Repositories;

public class GastoRepository(AppDbContext db) : IGastoRepository
{
    public Task<List<GastoItem>> GetByMesAsync(int mes, int anio) =>
        db.Gastos
          .Include(g => g.Categoria)
          .Include(g => g.TarjetaCuota)
          .Where(g => g.Mes == mes && g.Anio == anio)
          .OrderBy(g => g.Dia)
          .ToListAsync();

    public Task<GastoItem?> GetByIdAsync(int id) =>
        db.Gastos.FindAsync(id).AsTask();

    public Task<List<CategoriaGasto>> GetCategoriasAsync() =>
        db.CategoriasGasto
          .OrderBy(c => c.Tipo)
          .ThenBy(c => c.Nombre)
          .ToListAsync();

    public async Task AddAsync(GastoItem gasto)
    {
        db.Gastos.Add(gasto);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(GastoItem gasto)
    {
        db.Gastos.Update(gasto);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var g = await db.Gastos.FindAsync(id);
        if (g != null)
        {
            db.Gastos.Remove(g);
            await db.SaveChangesAsync();
        }
    }
}

public class IngresoRepository(AppDbContext db) : IIngresoRepository
{
    public Task<List<Ingreso>> GetByMesAsync(int mes, int anio) =>
        db.Ingresos
          .Where(i => i.Mes == mes && i.Anio == anio)
          .OrderBy(i => i.Dia)
          .ToListAsync();

    public async Task AddAsync(Ingreso ingreso)
    {
        db.Ingresos.Add(ingreso);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Ingreso ingreso)
    {
        db.Ingresos.Update(ingreso);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var i = await db.Ingresos.FindAsync(id);
        if (i != null)
        {
            db.Ingresos.Remove(i);
            await db.SaveChangesAsync();
        }
    }
}

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
            x.TarjetaId == tarjetaId &&
            x.Comercio  == comercio  &&
            x.MesCierre == mes       &&
            x.AnioCierre == anio);
}

public class DeudaRepository(AppDbContext db) : IDeudaRepository
{
    public Task<List<Deuda>> GetAllAsync() =>
        db.Deudas.OrderByDescending(d => d.Fecha).ToListAsync();

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
}

public class CuentaRepository(AppDbContext db) : ICuentaRepository
{
    public Task<List<Cuenta>> GetByMesAsync(int mes, int anio) =>
        db.Cuentas
          .Where(c => c.Mes == mes && c.Anio == anio)
          .ToListAsync();
}
