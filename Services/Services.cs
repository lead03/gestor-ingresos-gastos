using ControlGastos.Data;
using ControlGastos.Models;
using ControlGastos.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace ControlGastos.Services;

public class GastoService(AppDbContext db)
{
    public async Task<GastoListVM> GetListAsync(int mes, int anio)
    {
        var items = await db.Gastos
            .Include(g => g.Categoria)
            .Include(g => g.TarjetaCuota)
            .Where(g => g.Mes == mes && g.Anio == anio)
            .OrderBy(g => g.Dia)
            .ToListAsync();

        var vm = new GastoListVM
        {
            Mes = mes,
            Anio = anio,
            Items = items,
            TotalFijos = items.Where(g => g.Categoria.Tipo == "Fijo")
                              .Sum(g => g.SeDivide ? g.MontoDividido ?? g.Monto : g.Monto),
            TotalVariables = items.Where(g => g.Categoria.Tipo == "Variable")
                                  .Sum(g => g.SeDivide ? g.MontoDividido ?? g.Monto : g.Monto),
            PorDia = items.GroupBy(g => g.Dia)
                          .ToDictionary(grp => grp.Key, grp => grp.ToList())
        };
        return vm;
    }

    public async Task<GastoFormVM> GetFormAsync(int? id = null)
    {
        var vm = new GastoFormVM
        {
            Categorias = await db.CategoriasGasto.OrderBy(c => c.Tipo).ThenBy(c => c.Nombre).ToListAsync(),
            Cuotas = (await db.TarjetaCuotas.Include(tc => tc.Tarjeta)
                  .ToListAsync())
                  .Where(tc => tc.CuotasRestantes > 0)
                  .ToList()
        };
        if (id.HasValue)
        {
            var g = await db.Gastos.FindAsync(id.Value);
            if (g != null)
            {
                vm.Id = g.Id; vm.Mes = g.Mes; vm.Anio = g.Anio; vm.Dia = g.Dia;
                vm.CategoriaId = g.CategoriaId; vm.Monto = g.Monto;
                vm.SeDivide = g.SeDivide; vm.CantidadPersonas = g.CantidadPersonas;
                vm.MontoDividido = g.MontoDividido; vm.Descripcion = g.Descripcion;
                vm.MedioPago = g.MedioPago; vm.TarjetaCuotaId = g.TarjetaCuotaId;
            }
        }
        return vm;
    }

    public async Task SaveAsync(GastoFormVM vm)
    {
        if (vm.SeDivide && vm.CantidadPersonas.HasValue && vm.CantidadPersonas > 0)
            vm.MontoDividido = Math.Round(vm.Monto / vm.CantidadPersonas.Value, 2);

        if (vm.Id == 0)
        {
            db.Gastos.Add(new GastoItem
            {
                Mes = vm.Mes, Anio = vm.Anio, Dia = vm.Dia,
                CategoriaId = vm.CategoriaId, Monto = vm.Monto,
                SeDivide = vm.SeDivide, CantidadPersonas = vm.CantidadPersonas,
                MontoDividido = vm.MontoDividido, Descripcion = vm.Descripcion,
                MedioPago = vm.MedioPago, TarjetaCuotaId = vm.TarjetaCuotaId
            });
        }
        else
        {
            var g = await db.Gastos.FindAsync(vm.Id) ?? throw new KeyNotFoundException();
            g.Mes = vm.Mes; g.Anio = vm.Anio; g.Dia = vm.Dia;
            g.CategoriaId = vm.CategoriaId; g.Monto = vm.Monto;
            g.SeDivide = vm.SeDivide; g.CantidadPersonas = vm.CantidadPersonas;
            g.MontoDividido = vm.MontoDividido; g.Descripcion = vm.Descripcion;
            g.MedioPago = vm.MedioPago; g.TarjetaCuotaId = vm.TarjetaCuotaId;
        }
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var g = await db.Gastos.FindAsync(id);
        if (g != null) { db.Gastos.Remove(g); await db.SaveChangesAsync(); }
    }
}

public class IngresoService(AppDbContext db)
{
    public async Task<IngresoListVM> GetListAsync(int mes, int anio)
    {
        var items = await db.Ingresos
            .Where(i => i.Mes == mes && i.Anio == anio)
            .OrderBy(i => i.Dia)
            .ToListAsync();

        return new IngresoListVM
        {
            Mes = mes, Anio = anio, Items = items,
            TotalPropio       = items.Where(i => i.Tipo == "Propio").Sum(i => i.Monto),
            TotalDistribuido  = items.Where(i => i.Tipo == "Distribuido").Sum(i => i.Monto),
            TotalCuentaPropia = items.Where(i => i.Tipo == "CuentaPropia").Sum(i => i.Monto),
            TotalAhorro       = items.Where(i => i.Tipo == "Ahorro").Sum(i => i.Monto),
            TotalDpto         = items.Where(i => i.Tipo == "Dpto").Sum(i => i.Monto),
            TotalUSS          = items.Where(i => i.Tipo == "USS").Sum(i => i.Monto),
            TotalFIMA         = items.Where(i => i.Tipo == "FIMA").Sum(i => i.Monto),
            TotalResto        = items.Where(i => i.Tipo == "Resto").Sum(i => i.Monto),
        };
    }

    public async Task SaveAsync(IngresoFormVM vm)
    {
        if (vm.Id == 0)
            db.Ingresos.Add(new Ingreso { Mes=vm.Mes, Anio=vm.Anio, Dia=vm.Dia,
                Tipo=vm.Tipo, Monto=vm.Monto, Descripcion=vm.Descripcion, Fuente=vm.Fuente });
        else
        {
            var i = await db.Ingresos.FindAsync(vm.Id) ?? throw new KeyNotFoundException();
            i.Mes=vm.Mes; i.Anio=vm.Anio; i.Dia=vm.Dia;
            i.Tipo=vm.Tipo; i.Monto=vm.Monto; i.Descripcion=vm.Descripcion; i.Fuente=vm.Fuente;
        }
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var i = await db.Ingresos.FindAsync(id);
        if (i != null) { db.Ingresos.Remove(i); await db.SaveChangesAsync(); }
    }
}

public class TarjetaService(AppDbContext db)
{
    public async Task<TarjetaResumenVM> GetResumenAsync(int mes, int anio)
    {
        var tarjetas = await db.Tarjetas.ToListAsync();
        var cuotas = await db.TarjetaCuotas
            .Include(tc => tc.Tarjeta)
            .Where(tc => tc.MesCierre == mes && tc.AnioCierre == anio)
            .OrderBy(tc => tc.FechaCompra)
            .ToListAsync();

        var agrupadas = cuotas.GroupBy(tc => tc.TarjetaId)
                              .ToDictionary(g => g.Key, g => g.ToList());
        var totales = agrupadas.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Sum(tc => tc.MontoCuota));

        return new TarjetaResumenVM
        {
            Mes = mes, Anio = anio, Tarjetas = tarjetas,
            CuotasPorTarjeta = agrupadas,
            TotalPorTarjeta = totales,
            TotalGeneral = totales.Values.Sum()
        };
    }

    public async Task SaveCuotaAsync(TarjetaCuotaFormVM vm)
    {
        if (vm.TotalCuotas > 0 && vm.MontoTotal > 0)
            vm.MontoCuota = Math.Round(vm.MontoTotal / vm.TotalCuotas, 2);

        if (vm.Id == 0)
            db.TarjetaCuotas.Add(new TarjetaCuota {
                TarjetaId=vm.TarjetaId, Comercio=vm.Comercio, FechaCompra=vm.FechaCompra,
                MontoTotal=vm.MontoTotal, TotalCuotas=vm.TotalCuotas, MontoCuota=vm.MontoCuota,
                MesCierre=vm.MesCierre, AnioCierre=vm.AnioCierre, CuotasPagadas=vm.CuotasPagadas,
                PagaParte=vm.PagaParte, MontoPagoOtro=vm.MontoPagoOtro });
        else
        {
            var tc = await db.TarjetaCuotas.FindAsync(vm.Id) ?? throw new KeyNotFoundException();
            tc.TarjetaId=vm.TarjetaId; tc.Comercio=vm.Comercio; tc.FechaCompra=vm.FechaCompra;
            tc.MontoTotal=vm.MontoTotal; tc.TotalCuotas=vm.TotalCuotas; tc.MontoCuota=vm.MontoCuota;
            tc.MesCierre=vm.MesCierre; tc.AnioCierre=vm.AnioCierre; tc.CuotasPagadas=vm.CuotasPagadas;
            tc.PagaParte=vm.PagaParte; tc.MontoPagoOtro=vm.MontoPagoOtro;
        }
        await db.SaveChangesAsync();
    }

    public async Task AvanzarMesAsync(int tarjetaId, int mesActual, int anioActual)
    {
        // Copiar cuotas con saldo restante al mes siguiente
        var cuotas = await db.TarjetaCuotas
            .Where(tc => tc.TarjetaId == tarjetaId && tc.MesCierre == mesActual && tc.AnioCierre == anioActual && tc.CuotasRestantes > 1)
            .ToListAsync();

        int mesSig = mesActual == 12 ? 1 : mesActual + 1;
        int anioSig = mesActual == 12 ? anioActual + 1 : anioActual;

        foreach (var tc in cuotas)
        {
            var existente = await db.TarjetaCuotas
                .AnyAsync(x => x.TarjetaId == tarjetaId && x.Comercio == tc.Comercio
                            && x.MesCierre == mesSig && x.AnioCierre == anioSig);
            if (!existente)
            {
                db.TarjetaCuotas.Add(new TarjetaCuota
                {
                    TarjetaId = tc.TarjetaId, Comercio = tc.Comercio,
                    FechaCompra = tc.FechaCompra, MontoTotal = tc.MontoTotal,
                    TotalCuotas = tc.TotalCuotas, MontoCuota = tc.MontoCuota,
                    MesCierre = mesSig, AnioCierre = anioSig,
                    CuotasPagadas = tc.CuotasPagadas + 1,
                    PagaParte = tc.PagaParte, MontoPagoOtro = tc.MontoPagoOtro
                });
            }
        }
        await db.SaveChangesAsync();
    }
}

public class DashboardService(AppDbContext db)
{
    public async Task<DashboardVM> GetDashboardAsync(int mes, int anio)
    {
        var gastos = await db.Gastos.Include(g => g.Categoria)
            .Where(g => g.Mes == mes && g.Anio == anio).ToListAsync();
        var ingresos = await db.Ingresos
            .Where(i => i.Mes == mes && i.Anio == anio).ToListAsync();
        var cuentas = await db.Cuentas
            .Where(c => c.Mes == mes && c.Anio == anio).ToListAsync();
        var deudas = await db.Deudas
            .Where(d => d.Estado == "Activa" && d.Direccion == "MeDeben")
            .ToListAsync();

        decimal MontoEfectivo(GastoItem g) => g.SeDivide ? g.MontoDividido ?? g.Monto : g.Monto;

        var vm = new DashboardVM
        {
            Mes = mes, Anio = anio,
            TotalGastos   = gastos.Sum(MontoEfectivo),
            TotalIngresos = ingresos.Sum(i => i.Monto),
            TotalGastosFijos    = gastos.Where(g => g.Categoria.Tipo == "Fijo").Sum(MontoEfectivo),
            TotalGastosVariables= gastos.Where(g => g.Categoria.Tipo == "Variable").Sum(MontoEfectivo),
            Cuentas       = cuentas,
            TotalCuentas  = cuentas.Sum(c => c.SaldoFinal),
            TotalMeDeben  = deudas.Sum(d => d.Monto - (d.MontoPagado ?? 0)),
            DeudasActivas = deudas,
            GastosPorCategoria = gastos
                .GroupBy(g => g.Categoria.Nombre)
                .Select(grp => (grp.Key, grp.Sum(MontoEfectivo)))
                .OrderByDescending(x => x.Item2)
                .ToList(),
            PorDia = Enumerable.Range(1, DateTime.DaysInMonth(anio, mes))
                .Select(d => (
                    d,
                    gastos.Where(g => g.Dia == d).Sum(MontoEfectivo),
                    ingresos.Where(i => i.Dia == d).Sum(i => i.Monto)
                )).ToList()
        };

        // Histórico últimos 6 meses
        var meses = new List<(string label, decimal g, decimal i)>();
        for (int offset = 5; offset >= 0; offset--)
        {
            int m = mes - offset; int a = anio;
            if (m <= 0) { m += 12; a--; }
            var gM = await db.Gastos.Include(g => g.Categoria).Where(g => g.Mes==m && g.Anio==a).ToListAsync();
            var iM = await db.Ingresos.Where(i => i.Mes==m && i.Anio==a).ToListAsync();
            meses.Add((new DateTime(a, m, 1).ToString("MMM"), gM.Sum(MontoEfectivo), iM.Sum(i => i.Monto)));
        }
        vm.Historico = meses;

        return vm;
    }
}

public class DeudaService(AppDbContext db)
{
    public async Task<DeudaListVM> GetListAsync()
    {
        var deudas = await db.Deudas.OrderByDescending(d => d.Fecha).ToListAsync();
        var meDeben = deudas.Where(d => d.Direccion == "MeDeben").ToList();
        var leDebo  = deudas.Where(d => d.Direccion == "LeDebo").ToList();
        return new DeudaListVM
        {
            MeDeben = meDeben, LeDebo = leDebo,
            TotalMeDeben = meDeben.Where(d => d.Estado != "Pagada").Sum(d => d.Monto - (d.MontoPagado ?? 0)),
            TotalLeDebo  = leDebo .Where(d => d.Estado != "Pagada").Sum(d => d.Monto - (d.MontoPagado ?? 0)),
        };
    }

    public async Task SaveAsync(DeudaFormVM vm)
    {
        if (vm.Id == 0)
            db.Deudas.Add(new Deuda { Persona=vm.Persona, Monto=vm.Monto, Fecha=vm.Fecha,
                Descripcion=vm.Descripcion, Direccion=vm.Direccion, Estado=vm.Estado, MontoPagado=vm.MontoPagado });
        else
        {
            var d = await db.Deudas.FindAsync(vm.Id) ?? throw new KeyNotFoundException();
            d.Persona=vm.Persona; d.Monto=vm.Monto; d.Fecha=vm.Fecha;
            d.Descripcion=vm.Descripcion; d.Direccion=vm.Direccion;
            d.Estado=vm.Estado; d.MontoPagado=vm.MontoPagado;
        }
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var d = await db.Deudas.FindAsync(id);
        if (d != null) { db.Deudas.Remove(d); await db.SaveChangesAsync(); }
    }
}
