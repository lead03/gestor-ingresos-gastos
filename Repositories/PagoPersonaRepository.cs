using ControlGastos.Data;
using ControlGastos.Models;
using Microsoft.EntityFrameworkCore;

namespace ControlGastos.Repositories;

public class PagoPersonaRepository(AppDbContext db) : IPagoPersonaRepository
{
    public Task<List<PagoPersona>> GetByPersonaAsync(int personaId) =>
        db.PagosPersona
          .Where(p => p.PersonaId == personaId)
          .OrderByDescending(p => p.Fecha)
          .ToListAsync();

    public Task<PagoPersona?> GetByIdAsync(int id) =>
        db.PagosPersona.FindAsync(id).AsTask();

    public async Task AddAsync(PagoPersona pago)
    {
        db.PagosPersona.Add(pago);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(PagoPersona pago)
    {
        db.PagosPersona.Update(pago);
        await db.SaveChangesAsync();
    }
}
