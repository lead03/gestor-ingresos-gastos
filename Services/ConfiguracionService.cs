using ControlGastos.Common;
using ControlGastos.Data;
using ControlGastos.Models;
using ControlGastos.Repositories;
using ControlGastos.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace ControlGastos.Services;

public class ConfiguracionService(AppDbContext db, IBilleteraRepository billeteraRepo)
{
    // ── Mensajes de error de Banco ─────────────────────────────
    private const string ErrBancoYaExiste = "Ya existe un banco con el nombre '{0}'.";
    private const string ErrBancoNoEncontrado = "Banco no encontrado.";

    // ── Mensajes de error de Billetera ─────────────────────────
    private const string ErrBilleteraYaExiste = "Ya existe una billetera con el nombre '{0}'.";
    private const string ErrBilleteraNoEncontrada = "Billetera no encontrada.";

    // ══ Redes ══════════════════════════════════════════════════

    public Task<List<RedTarjeta>> GetRedesAsync() =>
        db.RedesTarjeta.OrderBy(r => r.Orden).ThenBy(r => r.Nombre).ToListAsync();

    public async Task AddRedAsync(string nombre)
    {
        var maxOrden = await db.RedesTarjeta.MaxAsync(r => (int?)r.Orden) ?? 0;
        db.RedesTarjeta.Add(new RedTarjeta { Nombre = nombre.Trim(), Orden = maxOrden + 1 });
        await db.SaveChangesAsync();
    }

    public async Task DeleteRedAsync(int id)
    {
        var r = await db.RedesTarjeta.FindAsync(id);
        if (r != null) { db.RedesTarjeta.Remove(r); await db.SaveChangesAsync(); }
    }

    // ══ Bancos ══════════════════════════════════════════════════

    public Task<List<BancoVM>> GetBancosAsync() =>
        db.Bancos
            .OrderBy(b => b.Orden).ThenBy(b => b.Nombre)
            .Select(b => new BancoVM
            {
                Id = b.Id,
                Nombre = b.Nombre,
                CuentasCount = b.Cuentas.Count()
            })
            .ToListAsync();

    public async Task<Result> AddBancoAsync(string nombre)
    {
        nombre = nombre.Trim();

        // CA1862: ToLower() es intencional — StringComparison no es traducible a SQL por EF Core
#pragma warning disable CA1862
        var existe = await db.Bancos.AnyAsync(b => b.Nombre.ToLower() == nombre.ToLower());
#pragma warning restore CA1862
        if (existe)
            return Result.Fail(string.Format(ErrBancoYaExiste, nombre));

        var maxOrden = await db.Bancos.MaxAsync(b => (int?)b.Orden) ?? 0;
        db.Bancos.Add(new Banco { Nombre = nombre, Orden = maxOrden + 1 });
        await db.SaveChangesAsync();
        return Result.Ok();
    }

    public async Task<Result> EditBancoAsync(int id, string nombre)
    {
        nombre = nombre.Trim();

        // CA1862: ToLower() es intencional — StringComparison no es traducible a SQL por EF Core
#pragma warning disable CA1862
        var existe = await db.Bancos.AnyAsync(b => b.Id != id && b.Nombre.ToLower() == nombre.ToLower());
#pragma warning restore CA1862
        if (existe)
            return Result.Fail(string.Format(ErrBancoYaExiste, nombre));

        var b = await db.Bancos.FindAsync(id);
        if (b == null) return Result.Fail(ErrBancoNoEncontrado);

        b.Nombre = nombre;
        await db.SaveChangesAsync();
        return Result.Ok();
    }

    public async Task<Result> DeleteBancoAsync(int id)
    {
        var b = await db.Bancos
            .Include(x => x.Cuentas)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (b == null) return Result.Fail(ErrBancoNoEncontrado);

        if (b.Cuentas.Count > 0)
            return Result.Fail($"No se puede eliminar '{b.Nombre}' porque tiene {b.Cuentas.Count} cuenta(s) asociada(s).");

        db.Bancos.Remove(b);
        await db.SaveChangesAsync();
        return Result.Ok();
    }

    // ══ Billeteras ══════════════════════════════════════════════

    public async Task<List<BilleteraVM>> GetBilleterasAsync() =>
        await billeteraRepo.GetAllWithCountAsync();

    public async Task<Result> AddBilleteraAsync(string nombre)
    {
        nombre = nombre.Trim();

        // CA1862: ToLower() es intencional — StringComparison no es traducible a SQL por EF Core
#pragma warning disable CA1862
        if (await db.Billeteras.AnyAsync(e => e.Nombre.ToLower() == nombre.ToLower()))
#pragma warning restore CA1862
            return Result.Fail(string.Format(ErrBilleteraYaExiste, nombre));

        var maxOrden = await db.Billeteras.MaxAsync(e => (int?)e.Orden) ?? 0;
        await billeteraRepo.AddAsync(new Billetera { Nombre = nombre, Orden = maxOrden + 1 });
        return Result.Ok();
    }

    public async Task<Result> EditBilleteraAsync(int id, string nombre)
    {
        nombre = nombre.Trim();

        // CA1862: ToLower() es intencional — StringComparison no es traducible a SQL por EF Core
#pragma warning disable CA1862
        if (await db.Billeteras.AnyAsync(e => e.Id != id && e.Nombre.ToLower() == nombre.ToLower()))
#pragma warning restore CA1862
            return Result.Fail(string.Format(ErrBilleteraYaExiste, nombre));

        var entidad = await billeteraRepo.GetByIdAsync(id);
        if (entidad is null) return Result.Fail(ErrBilleteraNoEncontrada);

        entidad.Nombre = nombre;
        await billeteraRepo.UpdateAsync(entidad);
        return Result.Ok();
    }

    public async Task<Result> DeleteBilleteraAsync(int id)
    {
        if (await billeteraRepo.TieneVinculadosAsync(id))
            return Result.Fail("No se puede eliminar porque tiene cuentas asociadas vinculadas.");

        var entidad = await billeteraRepo.GetByIdAsync(id);
        if (entidad is null) return Result.Fail(ErrBilleteraNoEncontrada);

        await billeteraRepo.DeleteAsync(entidad);
        return Result.Ok();
    }
}
