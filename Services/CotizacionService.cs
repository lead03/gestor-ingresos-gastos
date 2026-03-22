using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;

namespace ControlGastos.Services;

/// <summary>Resultado de la cotización con su fuente de origen.</summary>
public record CotizacionResultado(
    decimal Valor,
    string  Fuente,      // texto para mostrar al usuario
    string  FuenteTipo   // "api" | "ultima_api" | "manual"
);

public class CotizacionService(
    IHttpClientFactory   httpFactory,
    IMemoryCache         cache,
    ConfiguracionService configSvc)
{
    public static readonly Dictionary<string, string> TiposDisponibles = new()
    {
        { "blue",    "Dólar Blue"    },
        { "oficial", "Dólar Oficial" },
        { "tarjeta", "Dólar Tarjeta" },
        { "bolsa",   "Dólar MEP"     },
    };

    private const string CacheKey = "cotizacion_usd";

    /// <summary>
    /// Cadena de fallback:
    ///   1. Memoria caché (último llamado exitoso a la API, válido 30 min)
    ///   2. API online  → guarda en memoria + persiste en DB
    ///   3. Último valor API persistido en DB (sobrevive reinicios y cortes largos)
    ///   4. Valor manual configurado por el usuario
    /// </summary>
    public async Task<CotizacionResultado?> GetCotizacionConFuenteAsync()
    {
        var tipo      = await configSvc.GetSettingAsync("TipoDolar") ?? "blue";
        var tipNombre = TiposDisponibles.GetValueOrDefault(tipo, tipo);

        // 1 — Memoria caché (valor vigente de la API)
        if (cache.TryGetValue(CacheKey, out decimal cachedVal))
            return new(cachedVal, $"API · {tipNombre}", "api");

        // 2 — Intentar API
        try
        {
            var client = httpFactory.CreateClient("dolarapi");
            client.Timeout = TimeSpan.FromSeconds(4);
            var json = await client.GetStringAsync($"https://dolarapi.com/v1/dolares/{tipo}");
            using var doc   = JsonDocument.Parse(json);
            var venta = doc.RootElement.GetProperty("venta").GetDecimal();

            // Guardar en caché y persistir como "último valor conocido de API"
            cache.Set(CacheKey, venta, TimeSpan.FromMinutes(30));
            await configSvc.UpsertSettingAsync("UltimaCotizacionAPI",  venta.ToString(CultureInfo.InvariantCulture));
            await configSvc.UpsertSettingAsync("UltimaCotizacionTipo", tipNombre);

            return new(venta, $"API · {tipNombre}", "api");
        }
        catch
        {
            // 3 — Último valor de API persistido en DB
            var ultimaStr  = await configSvc.GetSettingAsync("UltimaCotizacionAPI");
            var ultimaTipo = await configSvc.GetSettingAsync("UltimaCotizacionTipo") ?? tipNombre;
            if (ultimaStr is not null &&
                decimal.TryParse(ultimaStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var ultimaVal))
                return new(ultimaVal, $"Último valor API · {ultimaTipo} (sin conexión)", "ultima_api");

            // 4 — Valor manual
            var manualStr = await configSvc.GetSettingAsync("CotizacionManual");
            if (manualStr is not null &&
                decimal.TryParse(manualStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var manualVal))
                return new(manualVal, "Manual", "manual");

            return null;
        }
    }

    /// <summary>Solo el valor decimal (para cálculos internos).</summary>
    public async Task<decimal?> GetCotizacionAsync() =>
        (await GetCotizacionConFuenteAsync())?.Valor;

    public async Task SaveTipoDolarAsync(string tipo)
    {
        cache.Remove(CacheKey);
        await configSvc.UpsertSettingAsync("TipoDolar", tipo);
    }

    public async Task SaveCotizacionManualAsync(decimal valor) =>
        await configSvc.UpsertSettingAsync("CotizacionManual",
            valor.ToString(CultureInfo.InvariantCulture));

    public async Task<string> GetTipoDolarAsync() =>
        await configSvc.GetSettingAsync("TipoDolar") ?? "blue";

    public async Task<decimal?> GetCotizacionManualAsync()
    {
        var s = await configSvc.GetSettingAsync("CotizacionManual");
        return s is not null &&
               decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var v)
               ? v : null;
    }
}
