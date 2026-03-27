using ControlGastos.Common;
using ControlGastos.Models;
using ControlGastos.Repositories;
using ControlGastos.ViewModels;

namespace ControlGastos.Services;

public class PersonaService(
    IPersonaRepository           personaRepo,
    IGastoParticipanteRepository participanteRepo,
    IDeudaRepository             deudaRepo,
    IPagoPersonaRepository       pagoRepo)
{
    public async Task<PersonaListVM> GetListAsync()
    {
        var personas    = await personaRepo.GetAllAsync();
        var todasDeudas = await deudaRepo.GetAllAsync();

        var resumen = new List<PersonaResumenVM>();

        foreach (var p in personas)
        {
            var participaciones = await participanteRepo.GetByPersonaAsync(p.Id);
            var deudas = todasDeudas.Where(d => d.PersonaId == p.Id).ToList();

            // Balance: suma de gastos compartidos + deudas directas
            // Excluye participaciones donde la persona fue quien pagó (no generan deuda hacia el usuario)
            decimal desdGastos = participaciones
                .Where(par => par.Tipo == TipoParticipante.Persona)
                .Where(par => par.GastoItem.PagadorPersonaId == null
                           || par.GastoItem.PagadorPersonaId != p.Id)
                .Sum(par => par.Monto);

            decimal desdDeudas = deudas
                .Where(d => d.Estado != EstadoDeuda.Pagada)
                .Sum(d =>
                {
                    decimal saldo = d.AceptaCuotas
                        ? d.Cuotas.Where(c => c.Estado != EstadoDeuda.Pagada).Sum(c => c.Saldo)
                        : d.Monto - (d.MontoPagado ?? 0);
                    return d.Direccion == DireccionDeuda.MeDeben ? saldo : -saldo;
                });

            var pagos = await pagoRepo.GetByPersonaAsync(p.Id);
            decimal desdPagos = pagos.Sum(pg => pg.Monto);

            resumen.Add(new PersonaResumenVM
            {
                Id               = p.Id,
                Nombre           = p.Nombre,
                Notas            = p.Notas,
                TotalMovimientos = participaciones.Count,
                Balance          = desdGastos + desdDeudas - desdPagos
            });
        }

        return new PersonaListVM { Personas = resumen };
    }

    public async Task<PersonaDetalleVM> GetDetalleAsync(int id, int mes = 0, int anio = 0)
    {
        var persona = await personaRepo.GetByIdWithDetalleAsync(id)
                      ?? throw new KeyNotFoundException($"Persona {id} no encontrada");

        // Carga expandida: gastos TC se dividen en una entrada por cuota
        var todas = await participanteRepo.GetExpandedByPersonaAsync(id);

        // Deudas directas: excluir las cuentas de crédito (AceptaCuotas=true)
        // porque esas se gestionan desde MeDeben/Deudas con el sistema de DeudaCuotas
        var deudas = persona.Deudas
                        .Where(d => d.Estado != EstadoDeuda.Pagada && !d.AceptaCuotas)
                        .ToList();

        // Balance ALL-TIME (no filtrado por mes)
        decimal desdGastos = todas.Sum(p => p.Monto);
        decimal desdDeudas = deudas.Sum(d =>
        {
            decimal saldo = d.Monto - (d.MontoPagado ?? 0);
            return d.Direccion == DireccionDeuda.MeDeben ? saldo : -saldo;
        });

        var pagos = await pagoRepo.GetByPersonaAsync(id);
        decimal desdPagos = pagos.Sum(p => p.Monto);

        // Filtrar por el mes seleccionado para mostrar en pantalla
        var filtradas = (mes > 0 && anio > 0)
            ? todas.Where(p => p.Mes == mes && p.Anio == anio).ToList()
            : todas;

        var porMes = filtradas
            .GroupBy(p => (p.Anio, p.Mes))
            .OrderByDescending(g => g.Key.Anio).ThenByDescending(g => g.Key.Mes)
            .Select(g => new PersonaMesVM
            {
                Anio            = g.Key.Anio,
                Mes             = g.Key.Mes,
                Total           = g.Sum(p => p.Monto),
                Participaciones = g.ToList()
            }).ToList();

        // ── Movimientos unificados del mes (signos pre-calculados) ────────────
        var mvsGastos = filtradas.Select(g => new MovimientoPersonaVM
        {
            TipoMov     = g.EsCuota ? TipoMovimientoPersona.Credito : TipoMovimientoPersona.Gasto,
            Fecha       = g.FechaCompra,
            Categoria   = g.EsCuota
                              ? $"{g.Categoria} (cuota {g.NumeroCuota}/{g.TotalCuotas})"
                              : g.Categoria,
            Descripcion = g.Descripcion,
            Monto       = g.Monto,
            RefId       = g.GastoItemId
        });

        var pagosFiltrados = (mes > 0 && anio > 0)
            ? pagos.Where(p => p.Fecha.Month == mes && p.Fecha.Year == anio)
            : pagos;
        var mvsPagos = pagosFiltrados.Select(p => new MovimientoPersonaVM
        {
            TipoMov     = TipoMovimientoPersona.Pago,
            Fecha       = p.Fecha,
            Categoria   = "—",
            Descripcion = p.Descripcion,
            Monto       = p.Monto,
            RefId       = p.Id
        });

        var deudasFiltradas = (mes > 0 && anio > 0)
            ? deudas.Where(d => d.Fecha.Month == mes && d.Fecha.Year == anio)
            : deudas;
        var mvsDeudas = deudasFiltradas.Select(d => new MovimientoPersonaVM
        {
            TipoMov     = d.Direccion == DireccionDeuda.MeDeben
                              ? TipoMovimientoPersona.DeudaMe
                              : TipoMovimientoPersona.DeudaLe,
            Fecha       = d.Fecha,
            Categoria   = "—",
            Descripcion = d.Descripcion,
            Monto       = d.Monto - (d.MontoPagado ?? 0),
            RefId       = d.Id,
            GastoItemId = d.GastoItemId   // vincula de vuelta al gasto original
        });

        var movimientos = mvsGastos.Concat(mvsPagos).Concat(mvsDeudas)
                                   .OrderBy(m => m.Fecha)
                                   .ToList();

        return new PersonaDetalleVM
        {
            Persona          = persona,
            Balance          = desdGastos + desdDeudas - desdPagos,
            TotalMovimientos = todas.Count(p => !p.EsCuota || p.NumeroCuota == 1),
            PorMes           = porMes,
            Deudas           = deudas,
            TotalDeudas      = desdDeudas,
            Pagos            = pagos,
            TotalPagos       = desdPagos,
            Movimientos      = movimientos
        };
    }

    public async Task<Result> SavePagoAsync(PagoPersonaFormVM vm)
    {
        if (vm.Monto <= 0 || vm.Monto > 9_999_999_999.99m)
            return Result.Fail("El monto debe ser mayor a $0 y menor a $9.999.999.999,99.");
        if (vm.CuentaId <= 0)
            return Result.Fail("Debe seleccionar una cuenta.");

        await pagoRepo.AddAsync(new PagoPersona
        {
            PersonaId   = vm.PersonaId,
            CuentaId    = vm.CuentaId,
            Monto       = vm.Monto,
            Moneda      = vm.Moneda,
            Fecha       = new DateTime(vm.Anio, vm.Mes, vm.Dia),
            Descripcion = string.IsNullOrWhiteSpace(vm.Descripcion) ? null : vm.Descripcion.Trim()
        });

        return Result.Ok();
    }

    public async Task<Result> EditPagoAsync(PagoPersonaFormVM vm)
    {
        if (vm.Monto <= 0 || vm.Monto > 9_999_999_999.99m)
            return Result.Fail("El monto debe ser mayor a $0 y menor a $9.999.999.999,99.");

        var pago = await pagoRepo.GetByIdAsync(vm.PagoId);
        if (pago == null) return Result.Fail("Pago no encontrado.");

        pago.Monto       = vm.Monto;
        pago.Fecha       = new DateTime(vm.Anio, vm.Mes, vm.Dia);
        pago.Descripcion = string.IsNullOrWhiteSpace(vm.Descripcion) ? null : vm.Descripcion.Trim();
        await pagoRepo.UpdateAsync(pago);

        return Result.Ok();
    }

    public async Task SaveAsync(PersonaFormVM vm)
    {
        if (vm.Id == 0)
        {
            var persona = new Persona { Nombre = vm.Nombre, Notas = vm.Notas };
            await personaRepo.AddAsync(persona);

            // Crear cuenta de crédito automática para trackear deudas mensuales
            await deudaRepo.AddAsync(new Deuda
            {
                PersonaId     = persona.Id,
                NombrePersona = persona.Nombre,
                Monto         = 0,
                Fecha         = DateTime.Today,
                Descripcion   = "Cuenta de crédito",
                Direccion     = DireccionDeuda.MeDeben,
                Estado        = EstadoDeuda.Activa,
                AceptaCuotas  = true
            });
        }
        else
        {
            var p = await personaRepo.GetByIdAsync(vm.Id)
                    ?? throw new KeyNotFoundException($"Persona {vm.Id} no encontrada");
            p.Nombre = vm.Nombre;
            p.Notas  = vm.Notas;
            await personaRepo.UpdateAsync(p);
        }
    }

    public Task DeleteAsync(int id) => personaRepo.DeleteAsync(id);
}
