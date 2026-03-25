using ControlGastos.Common;
using ControlGastos.Models;
using ControlGastos.Repositories;
using ControlGastos.ViewModels;

namespace ControlGastos.Services;

public class DeudaService(IDeudaRepository repo, IGastoParticipanteRepository participanteRepo)
{
    // ── Lista principal ───────────────────────────────────────────────

    public async Task<DeudaListVM> GetListAsync(int mes = 0, int anio = 0)
    {
        mes  = mes  == 0 ? DateTime.Today.Month : mes;
        anio = anio == 0 ? DateTime.Today.Year  : anio;

        var deudas    = await repo.GetAllAsync();
        var cuotasMes = await repo.GetCuotasByMesAsync(mes, anio);

        // Para cuentas de crédito (AceptaCuotas) vinculadas a una persona,
        // el saldo del mes se calcula desde los GastoParticipantes expandidos por cuota.
        var saldosVirtuales = new Dictionary<int, decimal>();
        foreach (var d in deudas.Where(d => d.AceptaCuotas && d.PersonaId.HasValue))
        {
            var expandidas = await participanteRepo.GetExpandedByPersonaAsync(d.PersonaId!.Value);
            saldosVirtuales[d.Id] = expandidas
                .Where(p => p.Mes == mes && p.Anio == anio)
                .Sum(p => p.Monto);
        }

        decimal SaldoEfectivo(Deuda d) =>
            saldosVirtuales.TryGetValue(d.Id, out var sv) ? sv : CalcSaldo(d);

        var meDeben = deudas.Where(d => d.Direccion == DireccionDeuda.MeDeben).ToList();
        var leDebo  = deudas.Where(d => d.Direccion == DireccionDeuda.LeDebo).ToList();

        return new DeudaListVM
        {
            MeDeben = meDeben,
            LeDebo  = leDebo,
            TotalMeDeben = meDeben.Where(d => d.Estado != EstadoDeuda.Pagada)
                                  .Sum(d => SaldoEfectivo(d)),
            TotalLeDebo  = leDebo.Where(d => d.Estado != EstadoDeuda.Pagada)
                                 .Sum(d => SaldoEfectivo(d)),
            CuotasMes      = cuotasMes,
            TotalCuotasMes = cuotasMes.Where(c => c.Estado != EstadoDeuda.Pagada)
                                      .Sum(c => c.Saldo),
            DeudasConCuotas = deudas.Where(d => d.AceptaCuotas).ToList(),
            SaldosVirtuales = saldosVirtuales,
            Mes  = mes,
            Anio = anio
        };
    }

    /// <summary>
    /// Saldo efectivo de una deuda sin cuenta virtual.
    /// - Si acepta cuotas: suma los saldos de cuotas activas/parciales.
    /// - Si no: Monto − MontoPagado.
    /// </summary>
    private static decimal CalcSaldo(Deuda d) =>
        d.AceptaCuotas
            ? d.Cuotas.Where(c => c.Estado != EstadoDeuda.Pagada).Sum(c => c.Saldo)
            : d.Monto - (d.MontoPagado ?? 0);

    // ── CRUD Deuda ───────────────────────────────────────────────────

    public async Task<Result> SaveAsync(DeudaFormVM vm)
    {
        if (vm.Id == 0)
        {
            await repo.AddAsync(new Deuda
            {
                PersonaId     = vm.PersonaId,
                NombrePersona = vm.NombrePersona,
                Monto         = vm.Monto,
                Fecha         = vm.Fecha,
                Descripcion   = vm.Descripcion,
                Direccion     = vm.Direccion,
                Estado        = vm.Estado,
                MontoPagado   = vm.MontoPagado,
                AceptaCuotas  = vm.AceptaCuotas
            });
        }
        else
        {
            var d = await repo.GetByIdAsync(vm.Id);
            if (d == null) return Result.Fail($"Deuda {vm.Id} no encontrada.");

            d.PersonaId     = vm.PersonaId;
            d.NombrePersona = vm.NombrePersona;
            d.Monto         = vm.Monto;
            d.Fecha         = vm.Fecha;
            d.Descripcion   = vm.Descripcion;
            d.Direccion     = vm.Direccion;
            d.Estado        = vm.Estado;
            d.MontoPagado   = vm.MontoPagado;
            d.AceptaCuotas  = vm.AceptaCuotas;

            await repo.UpdateAsync(d);
        }
        return Result.Ok();
    }

    public Task DeleteAsync(int id) => repo.DeleteAsync(id);

    // ── CRUD DeudaCuota ──────────────────────────────────────────────

    public async Task<Result> SaveCuotaAsync(DeudaCuotaFormVM vm)
    {
        if (vm.Id == 0)
        {
            if (await repo.ExisteCuotaEnMesAsync(vm.DeudaId, vm.Mes, vm.Anio))
                return Result.Fail("Ya existe una cuota registrada para ese mes.");

            await repo.AddCuotaAsync(new DeudaCuota
            {
                DeudaId    = vm.DeudaId,
                Mes        = vm.Mes,
                Anio       = vm.Anio,
                Monto      = vm.Monto,
                MontoPagado = vm.MontoPagado,
                Estado     = DeterminarEstado(vm.Monto, vm.MontoPagado),
                Descripcion = vm.Descripcion
            });
        }
        else
        {
            var c = await repo.GetCuotaByIdAsync(vm.Id);
            if (c == null) return Result.Fail("Cuota no encontrada.");

            c.Mes         = vm.Mes;
            c.Anio        = vm.Anio;
            c.Monto       = vm.Monto;
            c.MontoPagado = vm.MontoPagado;
            c.Estado      = DeterminarEstado(vm.Monto, vm.MontoPagado);
            c.Descripcion = vm.Descripcion;

            await repo.UpdateCuotaAsync(c);
        }
        return Result.Ok();
    }

    public async Task<Result> MarcarCuotaPagadaAsync(int id)
    {
        var c = await repo.GetCuotaByIdAsync(id);
        if (c == null) return Result.Fail("Cuota no encontrada.");

        c.MontoPagado = c.Monto;
        c.Estado      = EstadoDeuda.Pagada;
        await repo.UpdateCuotaAsync(c);
        return Result.Ok();
    }

    public Task DeleteCuotaAsync(int id) => repo.DeleteCuotaAsync(id);

    // ── Helpers ──────────────────────────────────────────────────────

    private static EstadoDeuda DeterminarEstado(decimal monto, decimal pagado) =>
        pagado >= monto ? EstadoDeuda.Pagada  :
        pagado >  0     ? EstadoDeuda.Parcial :
                          EstadoDeuda.Activa;
}
