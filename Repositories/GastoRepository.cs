using ControlGastos.Data;
using ControlGastos.Models;
using ControlGastos.ViewModels;
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
            .Where(g => g.Mes == mes && g.Anio == anio && g.TarjetaCuotaId == null)
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
          .Include(p => p.GastoItem).ThenInclude(g => g.TarjetaCuota)
          .Where(p => p.PersonaId == personaId)
          .OrderByDescending(p => p.GastoItem.Anio)
          .ThenByDescending(p => p.GastoItem.Mes)
          .ThenByDescending(p => p.GastoItem.Dia)
          .ToListAsync();

    public async Task<List<PersonaParticipacionVM>> GetExpandedByPersonaAsync(int personaId)
    {
        // 1. Participaciones con info del gasto y primera cuota (si aplica)
        var participaciones = await db.GastoParticipantes
            .AsNoTracking()
            .Include(p => p.GastoItem).ThenInclude(g => g.Categoria)
            .Include(p => p.GastoItem).ThenInclude(g => g.TarjetaCuota)
            .Where(p => p.PersonaId == personaId && p.Tipo == TipoParticipante.Persona)
            .ToListAsync();

        // 2. Para los gastos TC, traer TODAS sus cuotas (por GastoItemId)
        var gastoIdsTc = participaciones
            .Where(p => p.GastoItem.TarjetaCuotaId != null)
            .Select(p => p.GastoItemId)
            .Distinct()
            .ToList();

        var todasCuotas = gastoIdsTc.Count > 0
            ? await db.TarjetaCuotas
                .AsNoTracking()
                .Where(tc => tc.GastoItemId != null && gastoIdsTc.Contains(tc.GastoItemId!.Value))
                .OrderBy(tc => tc.AnioCierre).ThenBy(tc => tc.MesCierre)
                .ToListAsync()
            : new List<TarjetaCuota>();

        // 3. Expandir
        var resultado = new List<PersonaParticipacionVM>();

        foreach (var par in participaciones)
        {
            var g = par.GastoItem;

            if (g.TarjetaCuotaId != null)
            {
                // Gastos TC: una entrada por cada cuota
                var cuotasGasto = todasCuotas
                    .Where(tc => tc.GastoItemId == par.GastoItemId)
                    .ToList();

                if (cuotasGasto.Count == 0)
                {
                    // Fallback: sin cuotas encontradas, mostrar en mes de compra
                    resultado.Add(MapDirect(par, g.Mes, g.Anio));
                    continue;
                }

                int n = cuotasGasto.Count;
                decimal montoPorCuota = Math.Round(par.Monto / n, 2);
                decimal montoUltima   = par.Monto - montoPorCuota * (n - 1); // ajuste redondeo

                for (int i = 0; i < n; i++)
                {
                    var cuota = cuotasGasto[i];
                    resultado.Add(new PersonaParticipacionVM
                    {
                        Dia          = g.Dia,
                        MesCompra    = g.Mes,
                        AnioCompra   = g.Anio,
                        Mes          = cuota.MesCierre,
                        Anio         = cuota.AnioCierre,
                        Categoria    = g.Categoria.Nombre,
                        Descripcion  = g.Descripcion,
                        Monto        = i < n - 1 ? montoPorCuota : montoUltima,
                        NumeroCuota  = i + 1,
                        TotalCuotas  = n
                    });
                }
            }
            else
            {
                // Gasto directo: una sola entrada
                resultado.Add(MapDirect(par, g.Mes, g.Anio));
            }
        }

        return resultado;

        static PersonaParticipacionVM MapDirect(GastoParticipante par, int mes, int anio) =>
            new()
            {
                Dia         = par.GastoItem.Dia,
                MesCompra   = par.GastoItem.Mes,
                AnioCompra  = par.GastoItem.Anio,
                Mes         = mes,
                Anio        = anio,
                Categoria   = par.GastoItem.Categoria.Nombre,
                Descripcion = par.GastoItem.Descripcion,
                Monto       = par.Monto
            };
    }

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
