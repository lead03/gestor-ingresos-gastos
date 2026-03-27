using ControlGastos.Common;
using ControlGastos.Models;
using ControlGastos.Repositories;
using ControlGastos.ViewModels;

namespace ControlGastos.Services;

public class GastoService(
    IGastoRepository             gastoRepo,
    IGastoParticipanteRepository participanteRepo,
    ITarjetaRepository           tarjetaRepo,
    IPersonaRepository           personaRepo,
    ICuentaRepository            cuentaRepo,
    IDeudaRepository             deudaRepo,
    CotizacionService            cotizacionSvc)
{
    public async Task<GastoListVM> GetListAsync(int mes, int anio)
    {
        var items = await gastoRepo.GetByMesAsync(mes, anio);

        decimal MontoEfectivo(GastoItem g) => g.MiParteMes;

        var vm = new GastoListVM
        {
            Mes            = mes,
            Anio           = anio,
            Items          = items,
            TotalFijos        = items.Where(g => g.Categoria.TipoId == 1 && g.Moneda != Moneda.USD).Sum(MontoEfectivo),
            TotalVariables    = items.Where(g => g.Categoria.TipoId == 2 && g.Moneda != Moneda.USD).Sum(MontoEfectivo),
            TotalFijosUsd     = items.Where(g => g.Categoria.TipoId == 1 && g.Moneda == Moneda.USD).Sum(MontoEfectivo),
            TotalVariablesUsd = items.Where(g => g.Categoria.TipoId == 2 && g.Moneda == Moneda.USD).Sum(MontoEfectivo),
            PorDia         = items.GroupBy(g => g.Fecha.Day)
                                  .ToDictionary(grp => grp.Key, grp => grp.ToList())
        };
        var cotizRes = await cotizacionSvc.GetCotizacionConFuenteAsync();
        vm.CotizacionDolar       = cotizRes?.Valor;
        vm.FuenteCotizacion      = cotizRes?.Fuente;
        vm.FuenteCotizacionTipo  = cotizRes?.FuenteTipo;
        return vm;
    }

    public async Task<GastoFormVM> GetFormAsync(int? id = null)
    {
        var cuentasConSaldo = await ObtenerCuentasConSaldoAsync();

        var vm = new GastoFormVM
        {
            Categorias = await gastoRepo.GetCategoriasAsync(),
            Cuotas     = await tarjetaRepo.GetCuotasActivasAsync(),
            Personas   = await personaRepo.GetAllAsync(),
            Cuentas    = cuentasConSaldo,
            Tarjetas   = await tarjetaRepo.GetAllAsync()
        };

        if (id.HasValue)
        {
            var g = await gastoRepo.GetByIdWithParticipantesAsync(id.Value);
            if (g != null)
            {
                vm.Id = g.Id; vm.Fecha = g.Fecha;
                vm.CategoriaId = g.CategoriaId; vm.Monto = g.Monto;
                vm.SeDivide = g.SeDivide; vm.Descripcion = g.Descripcion;
                vm.CuentaId = g.CuentaId; vm.TarjetaId = g.TarjetaId;
                vm.TarjetaCuotaId = g.TarjetaCuotaId;
                vm.Moneda = g.Moneda;
                vm.PagadorPersonaId = g.PagadorPersonaId;
                if (g.TarjetaCuota != null) vm.CantidadCuotas = g.TarjetaCuota.TotalCuotas;
                vm.Participantes = g.Participantes.Select(p => new ParticipanteFormVM
                {
                    Id          = p.Id,
                    Tipo        = p.Tipo.ToString(),
                    Descripcion = p.Descripcion,
                    Monto       = p.Monto,
                    PersonaId   = p.PersonaId
                }).ToList();
            }
        }
        return vm;
    }

    public async Task<Result> SaveAsync(GastoFormVM vm)
    {
        decimal miParte = vm.SeDivide
            ? vm.Participantes.Where(p => p.Tipo == "Yo").Sum(p => p.Monto)
            : vm.Monto;

        // Al editar con tarjeta: borrar cuotas viejas antes de crear las nuevas
        if (vm.Id != 0 && vm.TarjetaId.HasValue && vm.CantidadCuotas >= 1)
        {
            var gastoActual = await gastoRepo.GetByIdAsync(vm.Id);
            if (gastoActual?.TarjetaCuotaId.HasValue == true)
            {
                var primeraCuota = await tarjetaRepo.GetCuotaByIdAsync(gastoActual.TarjetaCuotaId.Value);
                if (primeraCuota != null)
                    await tarjetaRepo.DeleteCuotasGrupoAsync(
                        primeraCuota.TarjetaId,
                        primeraCuota.FechaCompra,
                        primeraCuota.MontoTotal,
                        primeraCuota.TotalCuotas);
            }
        }

        // Si paga con tarjeta y tiene más de 1 cuota, crear TarjetaCuotas automáticamente
        int? tarjetaCuotaId = vm.TarjetaCuotaId;
        if (vm.TarjetaId.HasValue && vm.CantidadCuotas >= 1)
        {
            var cuotaResult = await CrearCuotasAutomaticasAsync(vm);
            if (!cuotaResult.Success) return cuotaResult;
            tarjetaCuotaId = cuotaResult.Value; // ID de la primera cuota
        }

        int?    pagadorId = vm.SeDivide ? vm.PagadorPersonaId : null;

        int gastoId;
        if (vm.Id == 0)
        {
            var gasto = new GastoItem
            {
                Fecha = vm.Fecha,
                CategoriaId = vm.CategoriaId, Monto = vm.Monto,
                SeDivide = vm.SeDivide, MiParte = miParte,
                Descripcion = vm.Descripcion,
                CuentaId = vm.CuentaId, TarjetaId = vm.TarjetaId,
                TarjetaCuotaId = tarjetaCuotaId,
                Moneda = vm.Moneda,
                PagadorPersonaId = pagadorId
            };
            await gastoRepo.AddAsync(gasto);
            gastoId = gasto.Id;

            if (vm.SeDivide && vm.Participantes.Any())
                await GuardarParticipantesAsync(gasto.Id, vm.Participantes);
        }
        else
        {
            var g = await gastoRepo.GetByIdAsync(vm.Id);
            if (g == null) return Result.Fail($"Gasto {vm.Id} no encontrado.");

            g.Fecha = vm.Fecha;
            g.CategoriaId = vm.CategoriaId; g.Monto = vm.Monto;
            g.SeDivide = vm.SeDivide; g.MiParte = miParte;
            g.Descripcion = vm.Descripcion;
            g.CuentaId = vm.CuentaId; g.TarjetaId = vm.TarjetaId;
            g.TarjetaCuotaId = tarjetaCuotaId;
            g.Moneda = vm.Moneda;
            g.PagadorPersonaId = pagadorId;

            await gastoRepo.UpdateAsync(g);
            await participanteRepo.DeleteByGastoAsync(vm.Id);
            gastoId = vm.Id;

            if (vm.SeDivide && vm.Participantes.Any())
                await GuardarParticipantesAsync(vm.Id, vm.Participantes);
        }

        // ── Deuda auto-generada por "¿Quién pagó?" ─────────────────
        // Primero limpiar cualquier deuda previa vinculada a este gasto
        await deudaRepo.DeleteByGastoItemIdAsync(gastoId);

        // Si otra persona pagó, crear deuda "Le debo" por la parte del usuario
        if (pagadorId.HasValue)
        {
            var pagador  = await personaRepo.GetByIdAsync(pagadorId.Value);
            decimal miParteDeuda = vm.Participantes.Where(p => p.Tipo == "Yo").Sum(p => p.Monto);
            if (miParteDeuda > 0 && pagador != null)
            {
                await deudaRepo.AddAsync(new Deuda
                {
                    PersonaId     = pagadorId,
                    NombrePersona = pagador.Nombre,
                    Monto         = miParteDeuda,
                    Fecha         = vm.Fecha,
                    Descripcion   = string.IsNullOrWhiteSpace(vm.Descripcion)
                                        ? $"Pagó el gasto por mí"
                                        : $"Pagó: {vm.Descripcion}",
                    Direccion     = DireccionDeuda.LeDebo,
                    Estado        = EstadoDeuda.Activa,
                    GastoItemId   = gastoId
                });
            }
        }

        // Actualizar GastoItemId en todas las cuotas del grupo
        if (vm.TarjetaId.HasValue && vm.CantidadCuotas >= 1)
        {
            var fechaCompra = vm.Fecha;
            await tarjetaRepo.ActualizarGastoIdCuotasAsync(
                vm.TarjetaId.Value, fechaCompra, vm.Monto, vm.CantidadCuotas, gastoId);
        }

        return Result.Ok();
    }

    public async Task DeleteAsync(int id)
    {
        await deudaRepo.DeleteByGastoItemIdAsync(id);
        await gastoRepo.DeleteAsync(id);
    }

    // ── Creación automática de cuotas ─────────────────────────────────
    private async Task<Result<int>> CrearCuotasAutomaticasAsync(GastoFormVM vm)
    {
        if (!vm.TarjetaId.HasValue)
            return Result.Fail<int>("No hay tarjeta seleccionada.");

        var tarjeta = await tarjetaRepo.GetByIdAsync(vm.TarjetaId.Value);
        if (tarjeta == null)
            return Result.Fail<int>("Tarjeta no encontrada.");

        var fechaCompra = vm.Fecha;
        decimal montoCuota = Math.Round(vm.Monto / vm.CantidadCuotas, 2);

        // Usar DiaCierre del mes específico si existe; si no, el default de la tarjeta
        var fechaMensual = await tarjetaRepo.GetFechaMensualAsync(vm.TarjetaId.Value, vm.Fecha.Month, vm.Fecha.Year);
        int diaCierre    = fechaMensual?.DiaCierre ?? tarjeta.DiaCierre;

        // Determinar mes de cierre de la primera cuota
        // Si el día del gasto es ANTES o EN el día de cierre → cierra este mes
        // Si es DESPUÉS del día de cierre → cierra el mes siguiente
        int mesCierre  = fechaCompra.Day <= diaCierre
            ? fechaCompra.Month
            : (fechaCompra.Month == 12 ? 1 : fechaCompra.Month + 1);
        int anioCierre = fechaCompra.Day <= diaCierre
            ? fechaCompra.Year
            : (fechaCompra.Month == 12 ? fechaCompra.Year + 1 : fechaCompra.Year);

        int primeraCuotaId = 0;

        for (int i = 0; i < vm.CantidadCuotas; i++)
        {
            // Calcular mes/año de cada cuota
            int mesActual  = mesCierre  + i;
            int anioActual = anioCierre;
            while (mesActual > 12) { mesActual -= 12; anioActual++; }

            // Ajustar monto de la última cuota para absorber diferencia de redondeo
            decimal montoEstaCuota = i == vm.CantidadCuotas - 1
                ? vm.Monto - (montoCuota * (vm.CantidadCuotas - 1))
                : montoCuota;

            var cuota = new TarjetaCuota
            {
                TarjetaId     = vm.TarjetaId.Value,
                Comercio      = vm.Descripcion ?? "Sin descripción",
                FechaCompra   = fechaCompra,
                MontoTotal    = vm.Monto,
                TotalCuotas   = vm.CantidadCuotas,
                MontoCuota    = Math.Round(montoEstaCuota, 2),
                MesCierre     = mesActual,
                AnioCierre    = anioActual,
                CuotasPagadas = i,
                PagaParte     = "NO",
                Moneda        = vm.Moneda
            };

            await tarjetaRepo.AddCuotaAsync(cuota);

            if (i == 0) primeraCuotaId = cuota.Id;
        }

        return Result.Ok(primeraCuotaId);
    }

    // ── Participantes ─────────────────────────────────────────────────
    private Task GuardarParticipantesAsync(int gastoId, List<ParticipanteFormVM> participantes)
    {
        var tipoMap = new Dictionary<string, TipoParticipante>
        {
            ["Yo"]      = TipoParticipante.Yo,
            ["Persona"] = TipoParticipante.Persona,
            ["Pagado"]  = TipoParticipante.Pagado
        };

        return participanteRepo.AddRangeAsync(participantes.Select(p => new GastoParticipante
        {
            GastoItemId = gastoId,
            Tipo        = tipoMap.GetValueOrDefault(p.Tipo, TipoParticipante.Yo),
            Descripcion = p.Descripcion,
            Monto       = p.Monto,
            PersonaId   = p.Tipo == "Persona" ? p.PersonaId : null
        }));
    }

    // ── Gestión de categorías ─────────────────────────────────────────
    public Task<List<CategoriaGasto>> GetTodasCategoriasAsync() =>
        gastoRepo.GetTodasCategoriasAsync();

    public async Task AgregarCategoriaAsync(string nombre, int tipoId)
    {
        await gastoRepo.AddCategoriaAsync(new CategoriaGasto
        {
            Nombre     = nombre.Trim(),
            TipoId     = tipoId,
            Habilitada = true
        });
    }

    public async Task EditarCategoriaAsync(int id, string nombre, int tipoId)
    {
        var todas = await gastoRepo.GetTodasCategoriasAsync();
        var cat   = todas.FirstOrDefault(c => c.Id == id);
        if (cat == null) return;
        cat.Nombre = nombre.Trim();
        cat.TipoId = tipoId;
        await gastoRepo.UpdateCategoriaAsync(cat);
    }

    /// <summary>
    /// Si tiene gastos → deshabilita (soft delete). Si no → elimina permanentemente.
    /// </summary>
    public async Task<string> DeshabilitarOEliminarAsync(int id)
    {
        if (await gastoRepo.CategoriaHasGastosAsync(id))
        {
            await gastoRepo.SetHabilitadaAsync(id, false);
            return "deshabilitada";
        }
        await gastoRepo.DeleteCategoriaAsync(id);
        return "eliminada";
    }

    public Task HabilitarCategoriaAsync(int id) =>
        gastoRepo.SetHabilitadaAsync(id, true);

    private async Task<List<CuentaResumenVM>> ObtenerCuentasConSaldoAsync()
    {
        var cuentas  = await cuentaRepo.GetAllActivasAsync();
        var resultado = new List<CuentaResumenVM>();

        foreach (var c in cuentas)
        {
            var gastos     = await cuentaRepo.GetGastosByCuentaAsync(c.Id);
            var ingresos   = await cuentaRepo.GetIngresosByCuentaAsync(c.Id);
            var pagosDeuda = await cuentaRepo.GetPagosDeudaByCuentaAsync(c.Id);
            var saldo      = c.SaldoInicial
                           + ingresos.Sum(i => i.Monto)
                           + pagosDeuda.SelectMany(p => p.Distribuciones)
                                       .Where(d => d.CuentaId == c.Id)
                                       .Sum(d => d.Monto)
                           - gastos.Sum(g => g.SeDivide ? g.MiParte ?? g.Monto : g.Monto);

            resultado.Add(new CuentaResumenVM
            {
                Id         = c.Id,
                Nombre     = c.Nombre,
                TipoEntidad = c.TipoEntidad,
                TipoNombre  = c.TipoEntidad.ToString(),
                SaldoActual = saldo,
                AlertaSaldo = c.AlertaSaldo
            });
        }
        return resultado;
    }
}
