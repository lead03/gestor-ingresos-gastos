using ControlGastos.Data;
using ControlGastos.Models;
using Microsoft.EntityFrameworkCore;

namespace ControlGastos.Repositories;

public class PersonaRepository(AppDbContext db) : IPersonaRepository
{
    public Task<List<Persona>> GetAllAsync() =>
        db.Personas.OrderBy(p => p.Nombre).ToListAsync();

    public Task<Persona?> GetByIdAsync(int id) =>
        db.Personas.FindAsync(id).AsTask();

    public Task<Persona?> GetByIdWithDetalleAsync(int id) =>
        db.Personas
          .Include(p => p.Deudas)
          .Include(p => p.Participaciones)
              .ThenInclude(par => par.GastoItem)
                  .ThenInclude(g => g.Categoria)
          .FirstOrDefaultAsync(p => p.Id == id);

    public async Task AddAsync(Persona persona)
    {
        db.Personas.Add(persona);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Persona persona)
    {
        db.Personas.Update(persona);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var p = await db.Personas.FindAsync(id);
        if (p != null)
        {
            db.Personas.Remove(p);
            await db.SaveChangesAsync();
        }
    }
}
