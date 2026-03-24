using ControlGastos.Data;
using ControlGastos.Models;
using Microsoft.EntityFrameworkCore;

namespace ControlGastos.Repositories;

public class ConfiguracionRepository(AppDbContext db) : IConfiguracionRepository
{
    public Task<List<ConfigOpcion>> GetByTipoAsync(string tipo) =>
        db.ConfigOpciones
          .Where(c => c.Tipo == tipo)
          .OrderBy(c => c.Orden).ThenBy(c => c.Valor)
          .ToListAsync();

    public Task<List<ConfigOpcion>> GetByTipoEntidadAsync(int tipoEntidadId) =>
        db.ConfigOpciones
          .Where(c => c.TipoEntidadId == tipoEntidadId)
          .OrderBy(c => c.Orden).ThenBy(c => c.Valor)
          .ToListAsync();

    public Task<List<ConfigOpcion>> GetAllAsync() =>
        db.ConfigOpciones
          .OrderBy(c => c.Tipo).ThenBy(c => c.Orden).ThenBy(c => c.Valor)
          .ToListAsync();

    public async Task AddAsync(ConfigOpcion opcion)
    {
        var maxOrden = opcion.TipoEntidadId.HasValue
            ? await db.ConfigOpciones.Where(c => c.TipoEntidadId == opcion.TipoEntidadId).MaxAsync(c => (int?)c.Orden) ?? 0
            : await db.ConfigOpciones.Where(c => c.Tipo == opcion.Tipo).MaxAsync(c => (int?)c.Orden) ?? 0;
        opcion.Orden = maxOrden + 1;
        db.ConfigOpciones.Add(opcion);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var op = await db.ConfigOpciones.FindAsync(id);
        if (op != null) { db.ConfigOpciones.Remove(op); await db.SaveChangesAsync(); }
    }

    public Task<ConfigOpcion?> GetSettingAsync(string key) =>
        db.ConfigOpciones.FirstOrDefaultAsync(c => c.Tipo == "Setting:" + key);

    public async Task UpsertSettingAsync(string key, string valor)
    {
        var tipo = "Setting:" + key;
        var existing = await db.ConfigOpciones.FirstOrDefaultAsync(c => c.Tipo == tipo);
        if (existing != null)
        {
            existing.Valor = valor;
            db.ConfigOpciones.Update(existing);
        }
        else
        {
            db.ConfigOpciones.Add(new ConfigOpcion { Tipo = tipo, Valor = valor });
        }
        await db.SaveChangesAsync();
    }
}
