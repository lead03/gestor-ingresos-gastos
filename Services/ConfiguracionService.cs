using ControlGastos.Common;
using ControlGastos.Data;
using ControlGastos.Models;
using ControlGastos.Repositories;
using ControlGastos.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace ControlGastos.Services;

public class ConfiguracionService(AppDbContext db, ICategoriaGastoRepository categoriaRepo, IBancoRepository bancoRepo, IBilleteraRepository billeteraRepo)
{
    // ── Mensajes de error de Banco ─────────────────────────────
    private const string ErrBancoYaExiste = "Ya existe un banco con el nombre '{0}'.";
    private const string ErrBancoNoEncontrado = "Banco no encontrado.";

    // ── Mensajes de error de Billetera ─────────────────────────
    private const string ErrBilleteraYaExiste = "Ya existe una billetera con el nombre '{0}'.";
    private const string ErrBilleteraNoEncontrada = "Billetera no encontrada.";

    // ── Mensajes de error de CategoriaGasto ────────────────────
    private const string ErrCategoriaNoEncontrada = "Categoría no encontrada.";

    // ══ Categorías de gasto ═════════════════════════════════════

    public async Task<List<CategoriaGastoVM>> GetCategoriasGastoAsync() =>
        await categoriaRepo.GetAllWithCountAsync();

    public async Task<Result> AddCategoriaGastoAsync(string nombre, int tipoId)
    {
        nombre = nombre.Trim();

        // CA1862: ToLower() es intencional — StringComparison no es traducible a SQL por EF Core
#pragma warning disable CA1862
        if (await db.CategoriasGasto.AnyAsync(e => e.TipoId == tipoId && e.Nombre.ToLower() == nombre.ToLower()))
#pragma warning restore CA1862
            return Result.Fail($"Ya existe una categoría con ese nombre en el mismo tipo.");

        await categoriaRepo.AddAsync(new CategoriaGasto { Nombre = nombre, TipoId = tipoId, Habilitada = true });
        return Result.Ok();
    }

    public async Task<Result> EditCategoriaGastoAsync(int id, string nombre, int tipoId)
    {
        nombre = nombre.Trim();

        // CA1862: ToLower() es intencional — StringComparison no es traducible a SQL por EF Core
#pragma warning disable CA1862
        if (await db.CategoriasGasto.AnyAsync(e => e.Id != id && e.TipoId == tipoId && e.Nombre.ToLower() == nombre.ToLower()))
#pragma warning restore CA1862
            return Result.Fail($"Ya existe una categoría con ese nombre en el mismo tipo.");

        var entidad = await categoriaRepo.GetByIdAsync(id);
        if (entidad is null) return Result.Fail(ErrCategoriaNoEncontrada);

        entidad.Nombre = nombre;
        entidad.TipoId = tipoId;
        await categoriaRepo.UpdateAsync(entidad);
        return Result.Ok();
    }

    /// <summary>
    /// Si tiene gastos → deshabilita (soft delete). Si no → elimina permanentemente.
    /// </summary>
    public async Task<Result<string>> DeleteCategoriaGastoAsync(int id)
    {
        var entidad = await categoriaRepo.GetByIdAsync(id);
        if (entidad is null) return Result.Fail<string>(ErrCategoriaNoEncontrada);

        if (await categoriaRepo.TieneVinculadosAsync(id))
        {
            entidad.Habilitada = false;
            await categoriaRepo.UpdateAsync(entidad);
            return Result.Ok<string>("deshabilitada");
        }

        await categoriaRepo.DeleteAsync(entidad);
        return Result.Ok<string>("eliminada");
    }

    public async Task<Result> HabilitarCategoriaGastoAsync(int id)
    {
        var entidad = await categoriaRepo.GetByIdAsync(id);
        if (entidad is null) return Result.Fail(ErrCategoriaNoEncontrada);

        entidad.Habilitada = true;
        await categoriaRepo.UpdateAsync(entidad);
        return Result.Ok();
    }

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

    public async Task<List<BancoVM>> GetBancosAsync() =>
        await bancoRepo.GetAllWithCountAsync();

    public async Task<Result> AddBancoAsync(string nombre)
    {
        nombre = nombre.Trim();

        // CA1862: ToLower() es intencional — StringComparison no es traducible a SQL por EF Core
#pragma warning disable CA1862
        if (await db.Bancos.AnyAsync(b => b.Nombre.ToLower() == nombre.ToLower()))
#pragma warning restore CA1862
            return Result.Fail(string.Format(ErrBancoYaExiste, nombre));

        await bancoRepo.AddAsync(new Banco { Nombre = nombre });
        return Result.Ok();
    }

    public async Task<Result> EditBancoAsync(int id, string nombre)
    {
        nombre = nombre.Trim();

        // CA1862: ToLower() es intencional — StringComparison no es traducible a SQL por EF Core
#pragma warning disable CA1862
        if (await db.Bancos.AnyAsync(b => b.Id != id && b.Nombre.ToLower() == nombre.ToLower()))
#pragma warning restore CA1862
            return Result.Fail(string.Format(ErrBancoYaExiste, nombre));

        var entidad = await bancoRepo.GetByIdAsync(id);
        if (entidad is null) return Result.Fail(ErrBancoNoEncontrado);

        entidad.Nombre = nombre;
        await bancoRepo.UpdateAsync(entidad);
        return Result.Ok();
    }

    public async Task<Result> DeleteBancoAsync(int id)
    {
        if (await bancoRepo.TieneVinculadosAsync(id))
            return Result.Fail("No se puede eliminar porque tiene cuentas asociadas vinculadas.");

        var entidad = await bancoRepo.GetByIdAsync(id);
        if (entidad is null) return Result.Fail(ErrBancoNoEncontrado);

        await bancoRepo.DeleteAsync(entidad);
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

        await billeteraRepo.AddAsync(new Billetera { Nombre = nombre });
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
