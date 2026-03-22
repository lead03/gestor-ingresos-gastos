using System.Text.Json;
using ControlGastos.Services;
using Microsoft.Extensions.Caching.Memory;

namespace ControlGastos.Services;

public class CotizacionService(
    IHttpClientFactory    httpFactory,
    IMemoryCache          cache,
    ConfiguracionService  configSvc)
{
    // Tipos disponibles en dolarapi.com
    public static readonly Dictionary<string, string> TiposDisponibles = new()
    {
        { "blue",     "Dólar Blue"    },
        { "oficial",  "Dólar Oficial" },
        { "tarjeta",  "Dólar Tarjeta" },
        { "bolsa",    "Dólar MEP"     },
    };

    /// <summary>Devuelve el valor de venta del USD en ARS. Puede ser null si no hay dato.</summary>
    public async Task<decimal?> GetCotizacionAsync()
    {
        const string cacheKey = "cotizacion_usd";
        if (cache.TryGetValue(cacheKey, out decimal cached)) return cached;

        var tipo = await configSvc.GetSettingAsync("TipoDolar") ?? "blue";
        try
        {
            var client = httpFactory.CreateClient("dolarapi");
            var json   = await client.GetStringAsync($"https://dolarapi.com/v1/dolares/{tipo}");
            using var doc = JsonDocument.Parse(json);
            var venta = doc.RootElement.GetProperty("venta").GetDecimal();
            cache.Set(cacheKey, venta, TimeSpan.FromMinutes(30));
            return venta;
        }
        catch
        {
            // Fallback al valor manual
            var manual = await configSvc.GetSettingAsync("CotizacionManual");
            if (manual != null && decimal.TryParse(manual, System.Globalization.NumberStyles.Any,
                                                   System.Globalization.CultureInfo.InvariantCulture, out var v))
                return v;
            return null;
        }
    }

    public async Task SaveTipoDolarAsync(string tipo)
    {
        cache.Remove("cotizacion_usd");
        await configSvc.UpsertSettingAsync("TipoDolar", tipo);
    }

    public async Task SaveCotizacionManualAsync(decimal valor)
    {
        await configSvc.UpsertSettingAsync("CotizacionManual",
            valor.ToString(System.Globalization.CultureInfo.InvariantCulture));
    }

    public async Task<string> GetTipoDolarAsync() =>
        await configSvc.GetSettingAsync("TipoDolar") ?? "blue";

    public async Task<decimal?> GetCotizacionManualAsync()
    {
        var s = await configSvc.GetSettingAsync("CotizacionManual");
        if (s != null && decimal.TryParse(s, System.Globalization.NumberStyles.Any,
                                          System.Globalization.CultureInfo.InvariantCulture, out var v))
            return v;
        return null;
    }
}
