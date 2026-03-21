using ControlGastos.Data;
using ControlGastos.Models;
using Microsoft.EntityFrameworkCore;

namespace ControlGastos.Repositories;

public class GastoRepository(AppDbContext db) : IGastoRepository
{
    public Task<List<GastoItem>> GetByMesAsync(int mes, int anio) =>
        db.Gastos
          .Include(g => g.Categoria)
          .Include(g => g.Cuenta)
          .Include(g => g.Tarjeta)
          .Include(g => g.TarjetaCuota)
          .Include(g => g.Participantes).ThenInclude(p => p.Persona)
          .Where(g => g.Mes == mes && g.Anio == anio)
          .OrderBy(g => g.Dia)
          .ToListAsync();

    public Task<GastoItem?> GetByIdAsync(int id) =>
        db.Gastos.FindAsync(id).AsTask();

    public Task<GastoItem?> GetByIdWithParticipantesAsync(int id) =>
        db.Gastos
          .Include(g => g.Participantes).ThenInclude(p => p.Persona)
          .FirstOrDefaultAsync(g => g.Id == id);

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

public class GastoParticipanteRepository(AppDbContext db) : IGastoParticipanteRepository
{
    public Task<List<GastoParticipante>> GetByGastoAsync(int gastoItemId) =>
        db.GastoParticipantes
          .Include(p => p.Persona)
          .Where(p => p.GastoItemId == gastoItemId)
          .ToListAsync();

    public Task<List<GastoParticipante>> GetByPersonaAsync(int personaId) =>
        db.GastoParticipantes
          .Include(p => p.GastoItem).ThenInclude(g => g.Categoria)
          .Where(p => p.PersonaId == personaId)
          .OrderByDescending(p => p.GastoItem.Anio)
          .ThenByDescending(p => p.GastoItem.Mes)
          .ThenByDescending(p => p.GastoItem.Dia)
          .ToListAsync();

    public async Task AddRangeAsync(IEnumerable<GastoParticipante> participantes)
    {
        db.GastoParticipantes.AddRange(participantes);
        await db.SaveChangesAsync();
    }

    public async Task DeleteByGastoAsync(int gastoItemId)
    {
        var items = await db.GastoParticipantes
                            .Where(p => p.GastoItemId == gastoItemId)
                            .ToListAsync();
        db.GastoParticipantes.RemoveRange(items);
        await db.SaveChangesAsync();
    }
}
