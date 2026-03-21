using ControlGastos.Data;
using ControlGastos.Models;
using Microsoft.EntityFrameworkCore;

namespace ControlGastos.Repositories;

public class IngresoRepository(AppDbContext db) : IIngresoRepository
{
    public Task<List<Ingreso>> GetByMesAsync(int mes, int anio) =>
        db.Ingresos
          .Include(i => i.Cuenta)
          .Where(i => i.Mes == mes && i.Anio == anio)
          .OrderBy(i => i.Dia)
          .ToListAsync();

    public Task<Ingreso?> GetByIdAsync(int id) =>
        db.Ingresos.FindAsync(id).AsTask();

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
}
