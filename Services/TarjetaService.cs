using ControlGastos.Common;
using ControlGastos.Models;
using ControlGastos.Repositories;
using ControlGastos.ViewModels;

namespace ControlGastos.Services;

public class TarjetaService(ITarjetaRepository repo)
{
    public async Task<TarjetaResumenVM> GetResumenAsync(int mes, int anio, int? tarjetaId = null)
    {
        var tarjetas = await repo.GetAllAsync();

        // Fechas mensuales de todas las tarjetas para este mes
        var fechasMensualesList = await repo.GetFechasMensualesByMesAsync(mes, anio);
        var fechasMensuales = fechasMensualesList.ToDictionary(f => f.TarjetaId);

        // Cuotas que cierran este mes (con filtro opcional)
        var cuotasMes = await repo.GetCuotasByMesAsync(mes, anio);
        if (tarjetaId.HasValue)
            cuotasMes = cuotasMes.Where(tc => tc.TarjetaId == tarjetaId.Value).ToList();

        var agrupadas = cuotasMes
            .GroupBy(tc => tc.TarjetaId)
            .ToDictionary(g => g.Key, g => g.ToList());
        var totales = agrupadas.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.Sum(tc => tc.MontoCuota));

        // Todas las compras con cuotas pendientes (con filtro opcional)
        var todasActivas = await repo.GetCuotasActivasAsync();
        if (tarjetaId.HasValue)
            todasActivas = todasActivas.Where(tc => tc.TarjetaId == tarjetaId.Value).ToList();

        var comprasActivas = todasActivas
            .Select(tc => new CompraActivaVM
            {
                TarjetaCuotaId = tc.Id,
                TarjetaId      = tc.TarjetaId,
                TarjetaNombre  = tc.Tarjeta.Nombre,
                Comercio       = tc.Comercio,
                FechaCompra    = tc.FechaCompra,
                MontoTotal     = tc.MontoTotal,
                MontoCuota     = tc.MontoCuota,
                TotalCuotas    = tc.TotalCuotas,
                CuotasPagadas  = tc.CuotasPagadas,
                MesCierre      = tc.MesCierre,
                AnioCierre     = tc.AnioCierre
            })
            .OrderBy(c => c.AnioCierre).ThenBy(c => c.MesCierre)
            .ThenBy(c => c.TarjetaNombre)
            .ToList();

        return new TarjetaResumenVM
        {
            Mes              = mes,
            Anio             = anio,
            TarjetaIdFiltro  = tarjetaId,
            Tarjetas         = tarjetas,
            FechasMensuales  = fechasMensuales,
            CuotasPorTarjeta = agrupadas,
            TotalPorTarjeta  = totales,
            TotalGeneral     = totales.Values.Sum(),
            ComprasActivas   = comprasActivas
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

    // ── Gestión de tarjetas ───────────────────────────────────────────
    public Task<List<Tarjeta>> GetAllTarjetasAsync() => repo.GetAllAsync();

    public Task<Tarjeta?> GetTarjetaByIdAsync(int id) => repo.GetByIdAsync(id);

    public async Task<Result> SaveTarjetaAsync(TarjetaFormVM vm)
    {
        if (vm.Id == 0)
        {
            await repo.AddTarjetaAsync(new Tarjeta
            {
                Nombre         = vm.Nombre,
                Banco          = vm.Banco,
                Red            = vm.Red,
                DiaCierre      = vm.DiaCierre,
                DiaVencimiento = vm.DiaVencimiento,
                LimiteCredito  = vm.LimiteCredito
            });
        }
        else
        {
            var t = await repo.GetByIdAsync(vm.Id);
            if (t == null) return Result.Fail("Tarjeta no encontrada.");
            t.Nombre         = vm.Nombre;
            t.Banco          = vm.Banco;
            t.Red            = vm.Red;
            t.DiaCierre      = vm.DiaCierre;
            t.DiaVencimiento = vm.DiaVencimiento;
            t.LimiteCredito  = vm.LimiteCredito;
            await repo.UpdateTarjetaAsync(t);
        }
        return Result.Ok();
    }

    public async Task<Result> DeleteTarjetaAsync(int id)
    {
        if (await repo.TieneCuotasAsync(id))
            return Result.Fail("No se puede eliminar: la tarjeta tiene compras registradas.");
        await repo.DeleteTarjetaAsync(id);
        return Result.Ok();
    }

    // ── Fechas mensuales ──────────────────────────────────────────────
    public Task<TarjetaFechaMensual?> GetFechaMensualAsync(int tarjetaId, int mes, int anio) =>
        repo.GetFechaMensualAsync(tarjetaId, mes, anio);

    /// <summary>
    /// Guarda la fecha mensual (DiaCierre/DiaVencimiento) para una tarjeta en un mes/año
    /// y recalcula las cuotas cuyos gastos fueron comprados en ese mes.
    /// </summary>
    public async Task SaveFechaMensualAsync(FechaMensualFormVM vm)
    {
        await repo.UpsertFechaMensualAsync(new TarjetaFechaMensual
        {
            TarjetaId      = vm.TarjetaId,
            Mes            = vm.Mes,
            Anio           = vm.Anio,
            DiaCierre      = vm.DiaCierre,
            DiaVencimiento = vm.DiaVencimiento
        });

        // Recalcular cuotas cuyas fechas de compra caen en este mes
        await RecalcularCuotasGrupoAsync(vm.TarjetaId, vm.Mes, vm.Anio, vm.DiaCierre);
    }

    /// <summary>
    /// Recalcula MesCierre/AnioCierre de todas las cuotas de una tarjeta
    /// cuyos gastos fueron comprados en mesFechaCompra/anioFechaCompra,
    /// usando el nuevoDiaCierre.
    /// </summary>
    private async Task RecalcularCuotasGrupoAsync(int tarjetaId, int mesFechaCompra, int anioFechaCompra, int nuevoDiaCierre)
    {
        var cuotas = await repo.GetCuotasByTarjetaYCompraAsync(tarjetaId, mesFechaCompra, anioFechaCompra);

        // Agrupar por (FechaCompra, MontoTotal, TotalCuotas) → cada grupo es una compra
        var grupos = cuotas.GroupBy(c => new { c.FechaCompra, c.MontoTotal, c.TotalCuotas });

        foreach (var grupo in grupos)
        {
            var fechaCompra = grupo.Key.FechaCompra;

            // Primer mes de cierre con el nuevo DiaCierre
            bool cierraEsteMes  = fechaCompra.Day <= nuevoDiaCierre;
            int  nuevoPrimerMes = cierraEsteMes
                ? fechaCompra.Month
                : (fechaCompra.Month == 12 ? 1 : fechaCompra.Month + 1);
            int  nuevoPrimerAnio = cierraEsteMes
                ? fechaCompra.Year
                : (fechaCompra.Month == 12 ? fechaCompra.Year + 1 : fechaCompra.Year);

            foreach (var cuota in grupo)
            {
                int mes  = nuevoPrimerMes  + cuota.CuotasPagadas;
                int anio = nuevoPrimerAnio;
                while (mes > 12) { mes -= 12; anio++; }

                cuota.MesCierre  = mes;
                cuota.AnioCierre = anio;
                await repo.UpdateCuotaAsync(cuota);
            }
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
