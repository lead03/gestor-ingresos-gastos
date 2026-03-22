using ControlGastos.Models;
using ControlGastos.Repositories;

namespace ControlGastos.Services;

public class ConfiguracionService(IConfiguracionRepository repo)
{
    public Task<List<ConfigOpcion>> GetRedesAsync()   => repo.GetByTipoAsync("Red");
    public Task<List<ConfigOpcion>> GetBancosAsync()  => repo.GetByTipoAsync("Banco");
    public Task<List<ConfigOpcion>> GetAllAsync()     => repo.GetAllAsync();

    public Task AddRedAsync(string valor)   => repo.AddAsync(new ConfigOpcion { Tipo = "Red",   Valor = valor.Trim() });
    public Task AddBancoAsync(string valor) => repo.AddAsync(new ConfigOpcion { Tipo = "Banco", Valor = valor.Trim() });
    public Task DeleteAsync(int id)         => repo.DeleteAsync(id);

    public async Task<string?> GetSettingAsync(string key)
    {
        var opt = await repo.GetSettingAsync(key);
        return opt?.Valor;
    }

    public Task UpsertSettingAsync(string key, string valor) =>
        repo.UpsertSettingAsync(key, valor);
}
