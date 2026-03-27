using ControlGastos.Data;
using ControlGastos.Models;
using Microsoft.EntityFrameworkCore;

namespace ControlGastos.Services;

public class ConfiguracionService(AppDbContext db)
{
    public Task<List<RedTarjeta>> GetRedesAsync() =>
        db.RedesTarjeta.OrderBy(r => r.Orden).ThenBy(r => r.Nombre).ToListAsync();
    public Task<List<Banco>> GetBancosAsync() =>
        db.Bancos.OrderBy(b => b.Orden).ThenBy(b => b.Nombre).ToListAsync();
    public Task<List<Billetera>> GetBilleterasAsync() =>
        db.Billeteras.OrderBy(b => b.Orden).ThenBy(b => b.Nombre).ToListAsync();

    public async Task AddRedAsync(string nombre)
    {
        var maxOrden = await db.RedesTarjeta.MaxAsync(r => (int?)r.Orden) ?? 0;
        db.RedesTarjeta.Add(new RedTarjeta { Nombre = nombre.Trim(), Orden = maxOrden + 1 });
        await db.SaveChangesAsync();
    }

    public async Task AddBancoAsync(string nombre)
    {
        var maxOrden = await db.Bancos.MaxAsync(b => (int?)b.Orden) ?? 0;
        db.Bancos.Add(new Banco { Nombre = nombre.Trim(), Orden = maxOrden + 1 });
        await db.SaveChangesAsync();
    }

    public async Task DeleteBancoAsync(int id)
    {
        var b = await db.Bancos.FindAsync(id);
        if (b != null) { db.Bancos.Remove(b); await db.SaveChangesAsync(); }
    }

    public async Task AddBilleteraAsync(string nombre)
    {
        var maxOrden = await db.Billeteras.MaxAsync(b => (int?)b.Orden) ?? 0;
        db.Billeteras.Add(new Billetera { Nombre = nombre.Trim(), Orden = maxOrden + 1 });
        await db.SaveChangesAsync();
    }

    public async Task DeleteBilleteraAsync(int id)
    {
        var b = await db.Billeteras.FindAsync(id);
        if (b != null) { db.Billeteras.Remove(b); await db.SaveChangesAsync(); }
    }

    public async Task DeleteRedAsync(int id)
    {
        var r = await db.RedesTarjeta.FindAsync(id);
        if (r != null) { db.RedesTarjeta.Remove(r); await db.SaveChangesAsync(); }
    }
}
