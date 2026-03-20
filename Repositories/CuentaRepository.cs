using ControlGastos.Data;
using ControlGastos.Models;
using Microsoft.EntityFrameworkCore;

namespace ControlGastos.Repositories;

public class CuentaRepository(AppDbContext db) : ICuentaRepository
{
    public Task<List<Cuenta>> GetByMesAsync(int mes, int anio) =>
        db.Cuentas
          .Where(c => c.Mes == mes && c.Anio == anio)
          .ToListAsync();
}
