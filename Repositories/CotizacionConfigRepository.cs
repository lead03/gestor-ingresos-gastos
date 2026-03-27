using ControlGastos.Data;
using ControlGastos.Models;
using Microsoft.EntityFrameworkCore;

namespace ControlGastos.Repositories;

public class CotizacionConfigRepository(AppDbContext db) : ICotizacionConfigRepository
{
    public async Task<CotizacionConfig> GetConfigAsync()
    {
        var config = await db.CotizacionConfigs.FirstOrDefaultAsync();
        if (config is null)
        {
            config = new CotizacionConfig { Id = 1 };
            db.CotizacionConfigs.Add(config);
            await db.SaveChangesAsync();
        }
        return config;
    }

    public async Task SaveConfigAsync(CotizacionConfig config)
    {
        db.CotizacionConfigs.Update(config);
        await db.SaveChangesAsync();
    }
}
