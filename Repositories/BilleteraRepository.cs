using ControlGastos.Data;
using ControlGastos.Models;
using ControlGastos.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace ControlGastos.Repositories;

public class BilleteraRepository(AppDbContext db) : IBilleteraRepository
{
    public Task<List<BilleteraVM>> GetAllWithCountAsync() =>
        db.Billeteras
            .OrderBy(e => e.Nombre)
            .Select(e => new BilleteraVM
            {
                Id           = e.Id,
                Nombre       = e.Nombre,
                CuentasCount = e.Cuentas.Count()
            })
            .ToListAsync();

    public Task<Billetera?> GetByIdAsync(int id) =>
        db.Billeteras.FirstOrDefaultAsync(e => e.Id == id);

    public async Task AddAsync(Billetera entidad)
    {
        db.Billeteras.Add(entidad);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Billetera entidad)
    {
        db.Billeteras.Update(entidad);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Billetera entidad)
    {
        db.Billeteras.Remove(entidad);
        await db.SaveChangesAsync();
    }

    public Task<bool> TieneVinculadosAsync(int id) =>
        db.Cuentas.AnyAsync(d => d.BilleteraId == id);
}
