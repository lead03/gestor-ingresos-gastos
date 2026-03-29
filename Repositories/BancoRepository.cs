using ControlGastos.Data;
using ControlGastos.Models;
using ControlGastos.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace ControlGastos.Repositories;

public class BancoRepository(AppDbContext db) : IBancoRepository
{
    public Task<List<BancoVM>> GetAllWithCountAsync() =>
        db.Bancos
            .OrderBy(e => e.Nombre)
            .Select(e => new BancoVM
            {
                Id           = e.Id,
                Nombre       = e.Nombre,
                CuentasCount = e.Cuentas.Count()
            })
            .ToListAsync();

    public Task<Banco?> GetByIdAsync(int id) =>
        db.Bancos.FirstOrDefaultAsync(e => e.Id == id);

    public async Task AddAsync(Banco entidad)
    {
        db.Bancos.Add(entidad);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Banco entidad)
    {
        db.Bancos.Update(entidad);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Banco entidad)
    {
        db.Bancos.Remove(entidad);
        await db.SaveChangesAsync();
    }

    public Task<bool> TieneVinculadosAsync(int id) =>
        db.Cuentas.AnyAsync(c => c.BancoId == id);
}
