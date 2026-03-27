using ControlGastos.Models;

namespace ControlGastos.Repositories;

public interface ICotizacionConfigRepository
{
    Task<CotizacionConfig> GetConfigAsync();
    Task SaveConfigAsync(CotizacionConfig config);
}
