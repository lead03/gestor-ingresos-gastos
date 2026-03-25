using ControlGastos.Data;
using ControlGastos.Models;
using Microsoft.EntityFrameworkCore;

namespace ControlGastos.Repositories;

public class GastoRepository(AppDbContext db) : IGastoRepository
{
    public async Task<List<GastoItem>> GetByMesAsync(int mes, int anio)
    {
        // 1. Gastos cuya fecha de registro cae en este mes
        //    (incluye la 1.ª cuota TC, cuyo GastoItem vive en el mes de compra)
        var gastosDirecto = await db.Gastos
            .AsNoTracking()
            .Include(g => g.Categoria).ThenInclude(c => c.Tipo)
            .Include(g => g.Cuenta)
            .Include(g => g.Tarjeta)
            .Include(g => g.TarjetaCuota)
            .Include(g => g.Participantes).ThenInclude(p => p.Persona)
            .Where(g => g.Mes == mes && g.Anio == anio)
            .OrderBy(g => g.Dia)
            .ToListAsync();

        // 2. Cuotas TC (2.ª, 3.ª, …) que cierran este mes y tienen GastoItem asociado
        var cuotasEsteMes = await db.TarjetaCuotas
            .AsNoTracking()
            .Include(tc => tc.Tarjeta)
            .Where(tc => tc.MesCierre == mes && tc.AnioCierre == anio
                      && tc.GastoItemId != null)
            .ToListAsync();

        // IDs de GastoItems ya incluidos (para no duplicar la 1.ª cuota)
        var gastoIdsDirecto = new HashSet<int>(gastosDirecto.Select(g => g.Id));

        var gastoIdsParaIncluir = cuotasEsteMes
            .Where(tc => tc.GastoItemId.HasValue
                      && !gastoIdsDirecto.Contains(tc.GastoItemId.Value))
            .Select(tc => tc.GastoItemId!.Value)
            .Distinct()
            .ToList();

        if (gastoIdsParaIncluir.Count == 0)
            return gastosDirecto;

        // 3. Cargar los GastoItems de esas cuotas anteriores
        var gastosCuotas = await db.Gastos
            .AsNoTracking()
            .Include(g => g.Categoria).ThenInclude(c => c.Tipo)
            .Include(g => g.Cuenta)
            .Include(g => g.Tarjeta)
            .Include(g => g.Participantes).ThenInclude(p => p.Persona)
            .Where(g => gastoIdsParaIncluir.Contains(g.Id))
            .ToListAsync();

        // 4. Inyectar la cuota correcta del mes en cada GastoItem
        //    para que MontoMes / MiParteMes devuelvan el importe de ESA cuota
        foreach (var gasto in gastosCuotas)
        {
            var cuota = cuotasEsteMes.FirstOrDefault(tc => tc.GastoItemId == gasto.Id);
            if (cuota != null)
            {
                gasto.TarjetaCuotaId = cuota.Id;
                gasto.TarjetaCuota   = cuota;
            }
        }

        return [.. gastosDirecto, .. gastosCuotas.OrderBy(g => g.Dia)];
    }

    public Task<GastoItem?> GetByIdAsync(int id) =>
        db.Gastos.FindAsync(id).AsTask();

    public Task<GastoItem?> GetByIdWithParticipantesAsync(int id) =>
        db.Gastos
          .Include(g => g.Participantes).ThenInclude(p => p.Persona)
          .Include(g => g.TarjetaCuota)
          .FirstOrDefaultAsync(g => g.Id == id);

    public Task<List<CategoriaGasto>> GetCategoriasAsync() =>
        db.CategoriasGasto
          .Include(c => c.Tipo)
          .Where(c => c.Habilitada)
          .OrderBy(c => c.TipoId)
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

    public Task<List<CategoriaGasto>> GetTodasCategoriasAsync() =>
        db.CategoriasGasto
          .Include(c => c.Tipo)
          .OrderBy(c => c.TipoId)
          .ThenBy(c => c.Nombre)
          .ToListAsync();

    public Task<bool> CategoriaHasGastosAsync(int id) =>
        db.Gastos.AnyAsync(g => g.CategoriaId == id);

    public async Task AddCategoriaAsync(CategoriaGasto cat)
    {
        db.CategoriasGasto.Add(cat);
        await db.SaveChangesAsync();
    }

    public async Task UpdateCategoriaAsync(CategoriaGasto cat)
    {
        db.CategoriasGasto.Update(cat);
        await db.SaveChangesAsync();
    }

    public async Task SetHabilitadaAsync(int id, bool habilitada)
    {
        var cat = await db.CategoriasGasto.FindAsync(id);
        if (cat != null) { cat.Habilitada = habilitada; await db.SaveChangesAsync(); }
    }

    public async Task DeleteCategoriaAsync(int id)
    {
        var cat = await db.CategoriasGasto.FindAsync(id);
        if (cat != null) { db.CategoriasGasto.Remove(cat); await db.SaveChangesAsync(); }
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
