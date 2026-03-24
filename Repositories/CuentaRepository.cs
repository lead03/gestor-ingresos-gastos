using ControlGastos.Data;
using ControlGastos.Models;
using Microsoft.EntityFrameworkCore;

namespace ControlGastos.Repositories;

public class CuentaRepository(AppDbContext db) : ICuentaRepository
{
    public Task<List<Cuenta>> GetAllActivasAsync() =>
        db.Cuentas
          .Include(c => c.TipoEntidad)
          .Where(c => c.Activa)
          .OrderBy(c => c.TipoId)
          .ThenBy(c => c.Nombre)
          .ToListAsync();

    public Task<Cuenta?> GetByIdAsync(int id) =>
        db.Cuentas
          .Include(c => c.TipoEntidad)
          .FirstOrDefaultAsync(c => c.Id == id);

    public async Task AddAsync(Cuenta cuenta)
    {
        db.Cuentas.Add(cuenta);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Cuenta cuenta)
    {
        db.Cuentas.Update(cuenta);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var c = await db.Cuentas.FindAsync(id);
        if (c != null)
        {
            // Soft delete — no borrar si tiene movimientos
            c.Activa = false;
            await db.SaveChangesAsync();
        }
    }

    public Task<List<GastoItem>> GetGastosByCuentaAsync(int cuentaId) =>
        db.Gastos
          .Include(g => g.Categoria)
          .Where(g => g.CuentaId == cuentaId)
          .OrderByDescending(g => g.Anio).ThenByDescending(g => g.Mes).ThenByDescending(g => g.Dia)
          .ToListAsync();

    public Task<List<Ingreso>> GetIngresosByCuentaAsync(int cuentaId) =>
        db.Ingresos
          .Include(i => i.TipoIngreso)
          .Include(i => i.Distribuciones).ThenInclude(d => d.Cuenta)
          .Where(i => i.Distribuciones.Any(d => d.CuentaId == cuentaId))
          .OrderByDescending(i => i.Anio).ThenByDescending(i => i.Mes).ThenByDescending(i => i.Dia)
          .ToListAsync();

    public Task<List<Ingreso>> GetPagosDeudaByCuentaAsync(int cuentaId) =>
        Task.FromResult(new List<Ingreso>());
}
