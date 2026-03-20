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
