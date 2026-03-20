using ControlGastos.Models;
using ControlGastos.Repositories;
using ControlGastos.ViewModels;

namespace ControlGastos.Services;

public class TarjetaService(ITarjetaRepository repo)
{
    public async Task<TarjetaResumenVM> GetResumenAsync(int mes, int anio)
    {
        var tarjetas = await repo.GetAllAsync();
        var cuotas   = await repo.GetCuotasByMesAsync(mes, anio);

        var agrupadas = cuotas.GroupBy(tc => tc.TarjetaId)
                              .ToDictionary(g => g.Key, g => g.ToList());
        var totales   = agrupadas.ToDictionary(
                              kvp => kvp.Key,
                              kvp => kvp.Value.Sum(tc => tc.MontoCuota));

        return new TarjetaResumenVM
        {
            Mes = mes, Anio = anio, Tarjetas = tarjetas,
            CuotasPorTarjeta = agrupadas,
            TotalPorTarjeta  = totales,
            TotalGeneral     = totales.Values.Sum()
        };
    }

    public Task<List<TarjetaCuota>> GetCuotasActivasAsync() =>
        repo.GetCuotasActivasAsync();

    public async Task SaveCuotaAsync(TarjetaCuotaFormVM vm)
    {
        if (vm.TotalCuotas > 0 && vm.MontoTotal > 0)
            vm.MontoCuota = Math.Round(vm.MontoTotal / vm.TotalCuotas, 2);

        if (vm.Id == 0)
        {
            await repo.AddCuotaAsync(new TarjetaCuota
            {
                TarjetaId = vm.TarjetaId, Comercio = vm.Comercio,
                FechaCompra = vm.FechaCompra, MontoTotal = vm.MontoTotal,
                TotalCuotas = vm.TotalCuotas, MontoCuota = vm.MontoCuota,
                MesCierre = vm.MesCierre, AnioCierre = vm.AnioCierre,
                CuotasPagadas = vm.CuotasPagadas,
                PagaParte = vm.PagaParte, MontoPagoOtro = vm.MontoPagoOtro
            });
        }
        else
        {
            var tc = await repo.GetCuotaByIdAsync(vm.Id)
                     ?? throw new KeyNotFoundException($"Cuota {vm.Id} no encontrada");

            tc.TarjetaId = vm.TarjetaId; tc.Comercio = vm.Comercio;
            tc.FechaCompra = vm.FechaCompra; tc.MontoTotal = vm.MontoTotal;
            tc.TotalCuotas = vm.TotalCuotas; tc.MontoCuota = vm.MontoCuota;
            tc.MesCierre = vm.MesCierre; tc.AnioCierre = vm.AnioCierre;
            tc.CuotasPagadas = vm.CuotasPagadas;
            tc.PagaParte = vm.PagaParte; tc.MontoPagoOtro = vm.MontoPagoOtro;

            await repo.UpdateCuotaAsync(tc);
        }
    }

    public async Task AvanzarMesAsync(int tarjetaId, int mesActual, int anioActual)
    {
        var cuotas  = await repo.GetCuotasParaAvanzarAsync(tarjetaId, mesActual, anioActual);
        int mesSig  = mesActual == 12 ? 1 : mesActual + 1;
        int anioSig = mesActual == 12 ? anioActual + 1 : anioActual;

        foreach (var tc in cuotas.Where(c => c.CuotasRestantes > 1))
        {
            var existe = await repo.ExisteCuotaAsync(tarjetaId, tc.Comercio, mesSig, anioSig);
            if (!existe)
            {
                await repo.AddCuotaAsync(new TarjetaCuota
                {
                    TarjetaId     = tc.TarjetaId,   Comercio      = tc.Comercio,
                    FechaCompra   = tc.FechaCompra, MontoTotal    = tc.MontoTotal,
                    TotalCuotas   = tc.TotalCuotas, MontoCuota    = tc.MontoCuota,
                    MesCierre     = mesSig,         AnioCierre    = anioSig,
                    CuotasPagadas = tc.CuotasPagadas + 1,
                    PagaParte     = tc.PagaParte,   MontoPagoOtro = tc.MontoPagoOtro
                });
            }
        }
    }
}
