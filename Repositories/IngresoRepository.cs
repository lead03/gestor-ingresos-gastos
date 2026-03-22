using ControlGastos.Data;
using ControlGastos.Models;
using Microsoft.EntityFrameworkCore;

namespace ControlGastos.Repositories;

public class IngresoRepository(AppDbContext db) : IIngresoRepository
{
    public Task<List<Ingreso>> GetByMesAsync(int mes, int anio) =>
        db.Ingresos
          .Include(i => i.TipoIngreso)
          .Include(i => i.Distribuciones).ThenInclude(d => d.Cuenta)
          .Where(i => i.Mes == mes && i.Anio == anio)
          .OrderBy(i => i.Dia)
          .ToListAsync();

    public Task<Ingreso?> GetByIdAsync(int id) =>
        db.Ingresos.FindAsync(id).AsTask();

    public Task<Ingreso?> GetByIdFullAsync(int id) =>
        db.Ingresos
          .Include(i => i.TipoIngreso)
          .Include(i => i.Distribuciones).ThenInclude(d => d.Cuenta)
          .FirstOrDefaultAsync(i => i.Id == id);

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
        if (i != null) { db.Ingresos.Remove(i); await db.SaveChangesAsync(); }
    }

    public async Task AddDistribucionesAsync(int ingresoId, IEnumerable<IngresoDistribucion> distribuciones)
    {
        foreach (var d in distribuciones) { d.IngresoId = ingresoId; db.IngresoDistribuciones.Add(d); }
        await db.SaveChangesAsync();
    }

    public async Task DeleteDistribucionesByIngresoAsync(int ingresoId)
    {
        var items = await db.IngresoDistribuciones.Where(d => d.IngresoId == ingresoId).ToListAsync();
        db.IngresoDistribuciones.RemoveRange(items);
        await db.SaveChangesAsync();
    }

    // TiposIngreso
    public Task<List<TipoIngreso>> GetTiposAsync() =>
        db.TiposIngreso.Where(t => t.Habilitada).OrderBy(t => t.Nombre).ToListAsync();

    public Task<List<TipoIngreso>> GetTodosTiposAsync() =>
        db.TiposIngreso.OrderBy(t => t.Nombre).ToListAsync();

    public Task<bool> TipoHasIngresosAsync(int id) =>
        db.Ingresos.AnyAsync(i => i.TipoIngresoId == id);

    public async Task AddTipoAsync(TipoIngreso tipo) { db.TiposIngreso.Add(tipo); await db.SaveChangesAsync(); }

    public async Task UpdateTipoAsync(TipoIngreso tipo) { db.TiposIngreso.Update(tipo); await db.SaveChangesAsync(); }

    public async Task SetTipoHabilitadoAsync(int id, bool habilitada)
    {
        var t = await db.TiposIngreso.FindAsync(id);
        if (t != null) { t.Habilitada = habilitada; await db.SaveChangesAsync(); }
    }

    public async Task DeleteTipoAsync(int id)
    {
        var t = await db.TiposIngreso.FindAsync(id);
        if (t != null) { db.TiposIngreso.Remove(t); await db.SaveChangesAsync(); }
    }
}
