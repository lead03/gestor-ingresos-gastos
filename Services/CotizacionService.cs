using System.Globalization;
using System.Text.Json;
using ControlGastos.Repositories;
using Microsoft.Extensions.Caching.Memory;

namespace ControlGastos.Services;

/// <summary>Resultado de la cotización con su fuente de origen.</summary>
public record CotizacionResultado(
    decimal  Valor,
    string   Fuente,        // texto para mostrar al usuario
    string   FuenteTipo,    // "api" | "ultima_api" | "manual"
    DateTime? FechaUltimaApi = null
);

public class CotizacionService(
    IHttpClientFactory          httpFactory,
    IMemoryCache                cache,
    ICotizacionConfigRepository cotizacionRepo)
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
        var config    = await cotizacionRepo.GetConfigAsync();
        var tipo      = config.TipoDolar;
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
            var ahora = DateTime.Now;
            cache.Set(CacheKey, venta, TimeSpan.FromMinutes(30));
            config.UltimoValor         = venta;
            config.UltimoTipo          = tipNombre;
            config.UltimaActualizacion = ahora;
            await cotizacionRepo.SaveConfigAsync(config);

            return new(venta, $"API · {tipNombre}", "api", ahora);
        }
        catch
        {
            // 3 — Último valor de API persistido en DB
            if (config.UltimoValor.HasValue)
            {
                var ultimaTipo  = config.UltimoTipo ?? tipNombre;
                var ultimaFecha = config.UltimaActualizacion;
                return new(config.UltimoValor.Value, $"Último valor API · {ultimaTipo} (sin conexión)", "ultima_api", ultimaFecha);
            }

            // 4 — Valor manual
            if (config.CotizacionManual > 0)
                return new(config.CotizacionManual, "Manual", "manual");

            return null;
        }
    }

    /// <summary>Solo el valor decimal (para cálculos internos).</summary>
    public async Task<decimal?> GetCotizacionAsync() =>
        (await GetCotizacionConFuenteAsync())?.Valor;

    /// <summary>Fuerza una nueva consulta a la API, ignorando caché.</summary>
    public async Task<CotizacionResultado?> ReintentarAsync()
    {
        cache.Remove(CacheKey);
        return await GetCotizacionConFuenteAsync();
    }

    public async Task SaveTipoDolarAsync(string tipo)
    {
        cache.Remove(CacheKey);
        var config = await cotizacionRepo.GetConfigAsync();
        config.TipoDolar = tipo;
        await cotizacionRepo.SaveConfigAsync(config);
    }

    public async Task SaveCotizacionManualAsync(decimal valor)
    {
        var config = await cotizacionRepo.GetConfigAsync();
        config.CotizacionManual = valor;
        await cotizacionRepo.SaveConfigAsync(config);
    }

    public async Task<string> GetTipoDolarAsync()
    {
        var config = await cotizacionRepo.GetConfigAsync();
        return config.TipoDolar;
    }

    public async Task<decimal?> GetCotizacionManualAsync()
    {
        var config = await cotizacionRepo.GetConfigAsync();
        return config.CotizacionManual > 0 ? config.CotizacionManual : null;
    }
}
