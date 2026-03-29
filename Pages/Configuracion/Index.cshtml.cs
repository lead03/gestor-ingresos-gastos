using ControlGastos.Helpers;
using ControlGastos.Models;
using ControlGastos.Repositories;
using ControlGastos.Services;
using ControlGastos.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ControlGastos.Pages.Configuracion;

public class IndexModel(ConfiguracionService svc, CotizacionService cotizacionSvc, IngresoService ingresoSvc, ICotizacionConfigRepository cotizacionRepo) : PageModel
{
    public List<RedTarjeta>     Redes        { get; set; } = [];
    public List<BancoVM>        Bancos       { get; set; } = [];
    public List<BilleteraVM>    Billeteras   { get; set; } = [];
    public List<CategoriaGastoVM> Categorias  { get; set; } = [];
    public List<TipoIngreso>    TiposIngreso { get; set; } = [];

    public CotizacionConfig Config { get; set; } = new();

    public string TipoDolarActual { get; set; } = "blue";
    public decimal? CotizacionActual { get; set; }
    public decimal? CotizacionManual { get; set; }
    public string? FuenteCotizacion { get; set; }
    public string? FuenteCotizacionTipo { get; set; }
    public DateTime? FechaUltimaApi { get; set; }


    private const string TabCategorias  = "categorias";
    private const string TabBancos      = "bancos";
    private const string TabBilleteras  = "billeteras";

    [BindProperty] public CategoriaGastoVM FormCategoriaGasto { get; set; } = new();
    [BindProperty] public BancoVM          FormBanco          { get; set; } = new();
    [BindProperty] public BilleteraVM      FormBilletera      { get; set; } = new();
    [BindProperty] public string NuevaRed { get; set; } = "";
    [BindProperty] public string NuevoTipoDolar { get; set; } = "blue";
    [BindProperty] public decimal CotizacionManualInput { get; set; }
    [BindProperty] public string NuevoTipoNombre { get; set; } = "";
    [BindProperty] public int EditTipoId { get; set; }
    [BindProperty] public string EditTipoNombre { get; set; } = "";

    // CotizacionConfig form bindings
    [BindProperty] public string TipoDolar { get; set; } = "blue";
    [BindProperty] public string ApiUrl { get; set; } = "https://dolarapi.com/v1/dolares";
    [BindProperty] public bool UsarApi { get; set; } = true;
    [BindProperty] public decimal ManualValor { get; set; } = 0;

    public async Task OnGetAsync(string? tab)
    {
        if (!string.IsNullOrEmpty(tab))
            ViewData["ActiveTab"] = tab;
        await CargarAsync();
    }

    // ── Config opciones ───────────────────────────────────────────
    public async Task<IActionResult> OnPostAgregarRedAsync()
    {
        if (!string.IsNullOrWhiteSpace(NuevaRed)) await svc.AddRedAsync(NuevaRed);
        return RedirectToPage(null, "redes");
    }

    public async Task<IActionResult> OnPostEliminarRedAsync(int id)
    {
        await svc.DeleteRedAsync(id);
        return RedirectToPage(null, "redes");
    }

    public async Task<IActionResult> OnPostAgregarBancoAsync()
    {
        foreach (var key in ModelState.Keys
            .Where(k => !k.StartsWith(nameof(FormBanco))).ToList())
            ModelState.Remove(key);

        if (!ModelState.IsValid)
        {
            ViewData["ActiveTab"] = TabBancos;
            await CargarAsync();
            return Page();
        }

        var result = await svc.AddBancoAsync(FormBanco.Nombre);
        if (!result.Success)
        {
            ModelState.AddModelError(
                $"{nameof(FormBanco)}.{nameof(BancoVM.Nombre)}", result.Error!);
            ViewData["ActiveTab"] = TabBancos;
            await CargarAsync();
            return Page();
        }

        TempData[TempDataKeys.Exito] = $"Banco \"{FormBanco.Nombre}\" agregado correctamente.";
        return RedirectToPage(new { tab = TabBancos });
    }

    public async Task<IActionResult> OnPostEditarBancoAsync()
    {
        foreach (var key in ModelState.Keys
            .Where(k => !k.StartsWith(nameof(FormBanco))).ToList())
            ModelState.Remove(key);

        if (!ModelState.IsValid)
            return RedirectToPage(new { tab = TabBancos });

        var result = await svc.EditBancoAsync(FormBanco.Id, FormBanco.Nombre);
        if (!result.Success)
            TempData[TempDataKeys.Error] = result.Error;
        else
            TempData[TempDataKeys.Exito] = $"Banco renombrado a \"{FormBanco.Nombre}\" correctamente.";

        return RedirectToPage(new { tab = TabBancos });
    }

    public async Task<IActionResult> OnPostEliminarBancoAsync(int id)
    {
        var result = await svc.DeleteBancoAsync(id);
        if (!result.Success)
            TempData[TempDataKeys.Error] = result.Error;
        else
            TempData[TempDataKeys.Exito] = "Banco eliminado correctamente.";
        return RedirectToPage(new { tab = TabBancos });
    }

    public async Task<IActionResult> OnPostAgregarBilleteraAsync()
    {
        foreach (var key in ModelState.Keys
            .Where(k => !k.StartsWith(nameof(FormBilletera))).ToList())
            ModelState.Remove(key);

        if (!ModelState.IsValid)
        {
            ViewData["ActiveTab"] = TabBilleteras;
            await CargarAsync();
            return Page();
        }

        var result = await svc.AddBilleteraAsync(FormBilletera.Nombre);
        if (!result.Success)
        {
            ModelState.AddModelError(
                $"{nameof(FormBilletera)}.{nameof(BilleteraVM.Nombre)}", result.Error!);
            ViewData["ActiveTab"] = TabBilleteras;
            await CargarAsync();
            return Page();
        }

        TempData[TempDataKeys.Exito] = $"Billetera \"{FormBilletera.Nombre}\" agregada correctamente.";
        return RedirectToPage(new { tab = TabBilleteras });
    }

    public async Task<IActionResult> OnPostEditarBilleteraAsync()
    {
        foreach (var key in ModelState.Keys
            .Where(k => !k.StartsWith(nameof(FormBilletera))).ToList())
            ModelState.Remove(key);

        if (!ModelState.IsValid)
            return RedirectToPage(new { tab = TabBilleteras });

        var result = await svc.EditBilleteraAsync(FormBilletera.Id, FormBilletera.Nombre);
        if (!result.Success)
            TempData[TempDataKeys.Error] = result.Error;
        else
            TempData[TempDataKeys.Exito] = $"Billetera renombrada a \"{FormBilletera.Nombre}\" correctamente.";

        return RedirectToPage(new { tab = TabBilleteras });
    }

    public async Task<IActionResult> OnPostEliminarBilleteraAsync(int id)
    {
        var result = await svc.DeleteBilleteraAsync(id);
        if (!result.Success)
            TempData[TempDataKeys.Error] = result.Error;
        else
            TempData[TempDataKeys.Exito] = "Billetera eliminada correctamente.";

        return RedirectToPage(new { tab = TabBilleteras });
    }

    // ── Categorías de gasto ───────────────────────────────────────
    public async Task<IActionResult> OnPostAgregarCategoriaGastoAsync()
    {
        foreach (var key in ModelState.Keys
            .Where(k => !k.StartsWith(nameof(FormCategoriaGasto))).ToList())
            ModelState.Remove(key);

        if (!ModelState.IsValid)
        {
            ViewData["ActiveTab"] = TabCategorias;
            await CargarAsync();
            return Page();
        }

        var result = await svc.AddCategoriaGastoAsync(FormCategoriaGasto.Nombre, FormCategoriaGasto.TipoId);
        if (!result.Success)
        {
            ModelState.AddModelError(
                $"{nameof(FormCategoriaGasto)}.{nameof(CategoriaGastoVM.Nombre)}", result.Error!);
            ViewData["ActiveTab"] = TabCategorias;
            await CargarAsync();
            return Page();
        }

        TempData[TempDataKeys.Exito] = $"Categoría \"{FormCategoriaGasto.Nombre}\" agregada correctamente.";
        return RedirectToPage(new { tab = TabCategorias });
    }

    public async Task<IActionResult> OnPostEditarCategoriaGastoAsync()
    {
        foreach (var key in ModelState.Keys
            .Where(k => !k.StartsWith(nameof(FormCategoriaGasto))).ToList())
            ModelState.Remove(key);

        if (!ModelState.IsValid)
            return RedirectToPage(new { tab = TabCategorias });

        var result = await svc.EditCategoriaGastoAsync(FormCategoriaGasto.Id, FormCategoriaGasto.Nombre, FormCategoriaGasto.TipoId);
        if (!result.Success)
            TempData[TempDataKeys.Error] = result.Error;
        else
            TempData[TempDataKeys.Exito] = $"Categoría renombrada a \"{FormCategoriaGasto.Nombre}\" correctamente.";

        return RedirectToPage(new { tab = TabCategorias });
    }

    public async Task<IActionResult> OnPostEliminarCategoriaGastoAsync(int id)
    {
        var result = await svc.DeleteCategoriaGastoAsync(id);
        if (!result.Success)
            TempData[TempDataKeys.Error] = result.Error;
        else if (result.Value == "deshabilitada")
            TempData[TempDataKeys.Atencion] = "La categoría fue deshabilitada porque tiene gastos asociados.";
        else
            TempData[TempDataKeys.Exito] = "Categoría eliminada correctamente.";

        return RedirectToPage(new { tab = TabCategorias });
    }

    public async Task<IActionResult> OnPostHabilitarCategoriaGastoAsync(int id)
    {
        var result = await svc.HabilitarCategoriaGastoAsync(id);
        if (!result.Success)
            TempData[TempDataKeys.Error] = result.Error;
        else
            TempData[TempDataKeys.Exito] = "Categoría habilitada correctamente.";

        return RedirectToPage(new { tab = TabCategorias });
    }

    public async Task<IActionResult> OnPostReintentarCotizacionAsync()
    {
        var res = await cotizacionSvc.ReintentarAsync();
        if (res is { FuenteTipo: "api" })
            return new JsonResult(new { ok = true, valor = res.Valor, fecha = res.FechaUltimaApi?.ToString("dd/MM/yyyy HH:mm") });
        return new JsonResult(new { ok = false });
    }

    public async Task<IActionResult> OnPostGuardarCotizacionAsync()
    {
        var config = await cotizacionRepo.GetConfigAsync();
        config.TipoDolar = TipoDolar;
        config.ApiUrl = ApiUrl;
        config.UsarApi = UsarApi;
        config.CotizacionManual = ManualValor;
        await cotizacionRepo.SaveConfigAsync(config);
        return RedirectToPage(null, "cotizacion");
    }

    // ── Tipos de ingreso ──────────────────────────────────────────
    public async Task<IActionResult> OnPostAgregarTipoIngresoAsync()
    {
        if (!string.IsNullOrWhiteSpace(NuevoTipoNombre))
            await ingresoSvc.AgregarTipoAsync(NuevoTipoNombre);
        return RedirectToPage(null, "tipos-ingreso");
    }

    public async Task<IActionResult> OnPostEditarTipoIngresoAsync()
    {
        if (EditTipoId > 0 && !string.IsNullOrWhiteSpace(EditTipoNombre))
            await ingresoSvc.EditarTipoAsync(EditTipoId, EditTipoNombre);
        return RedirectToPage(null, "tipos-ingreso");
    }

    public async Task<IActionResult> OnPostDeshabilitarTipoIngresoAsync(int id)
    {
        await ingresoSvc.DeshabilitarOEliminarTipoAsync(id);
        return RedirectToPage(null, "tipos-ingreso");
    }

    public async Task<IActionResult> OnPostHabilitarTipoIngresoAsync(int id)
    {
        await ingresoSvc.HabilitarTipoAsync(id);
        return RedirectToPage(null, "tipos-ingreso");
    }

    private async Task CargarAsync()
    {
        Redes = await svc.GetRedesAsync();
        Bancos = await svc.GetBancosAsync();
        Billeteras = await svc.GetBilleterasAsync();
        Categorias = await svc.GetCategoriasGastoAsync();
        TiposIngreso = await ingresoSvc.GetTodosTiposAsync();
        Config = await cotizacionRepo.GetConfigAsync();
        TipoDolarActual = Config.TipoDolar;
        TipoDolar = Config.TipoDolar;
        ApiUrl = Config.ApiUrl;
        UsarApi = Config.UsarApi;
        ManualValor = Config.CotizacionManual;
        var cotizRes = await cotizacionSvc.GetCotizacionConFuenteAsync();
        CotizacionActual = cotizRes?.Valor;
        FuenteCotizacion = cotizRes?.Fuente;
        FuenteCotizacionTipo = cotizRes?.FuenteTipo;
        FechaUltimaApi = cotizRes?.FechaUltimaApi;
        CotizacionManual = Config.CotizacionManual > 0 ? Config.CotizacionManual : null;
        ViewData["Active"] = "configuracion";
    }
}
