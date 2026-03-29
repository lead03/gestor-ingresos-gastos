using ControlGastos.Data;
using ControlGastos.Models;
using ControlGastos.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace ControlGastos.Repositories;

public class CategoriaGastoRepository(AppDbContext db) : ICategoriaGastoRepository
{
    public Task<List<CategoriaGastoVM>> GetAllWithCountAsync() =>
        db.CategoriasGasto
            .OrderBy(e => e.TipoId)
            .ThenBy(e => e.Nombre)
            .Select(e => new CategoriaGastoVM
            {
                Id             = e.Id,
                Nombre         = e.Nombre,
                TipoId         = e.TipoId,
                TipoNombre     = e.Tipo.Nombre,
                Habilitada     = e.Habilitada,
                GastoItemsCount = e.GastoItems.Count()
            })
            .ToListAsync();

    public Task<CategoriaGasto?> GetByIdAsync(int id) =>
        db.CategoriasGasto.FirstOrDefaultAsync(e => e.Id == id);

    public async Task AddAsync(CategoriaGasto entidad)
    {
        db.CategoriasGasto.Add(entidad);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(CategoriaGasto entidad)
    {
        db.CategoriasGasto.Update(entidad);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(CategoriaGasto entidad)
    {
        db.CategoriasGasto.Remove(entidad);
        await db.SaveChangesAsync();
    }

    public Task<bool> TieneVinculadosAsync(int id) =>
        db.Gastos.AnyAsync(d => d.CategoriaId == id);
}
