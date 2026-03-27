using ControlGastos.Models;
using ControlGastos.Repositories;
using ControlGastos.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ControlGastos.Pages.Configuracion;

public class IndexModel(ConfiguracionService svc, GastoService gastoSvc, CotizacionService cotizacionSvc, IngresoService ingresoSvc, ICotizacionConfigRepository cotizacionRepo) : PageModel
{
    public List<RedTarjeta>     Redes      { get; set; } = new();
    public List<Banco>          Bancos     { get; set; } = new();
    public List<Billetera>      Billeteras { get; set; } = new();
    public List<CategoriaGasto> Categorias { get; set; } = new();
    public List<TipoIngreso>    TiposIngreso { get; set; } = new();

    public CotizacionConfig Config { get; set; } = new();

    public string   TipoDolarActual       { get; set; } = "blue";
    public decimal? CotizacionActual      { get; set; }
    public decimal? CotizacionManual      { get; set; }
    public string?  FuenteCotizacion      { get; set; }
    public string?  FuenteCotizacionTipo  { get; set; }
    public DateTime? FechaUltimaApi       { get; set; }

    [BindProperty] public string  NuevaRed              { get; set; } = "";
    [BindProperty] public string  NuevoBanco            { get; set; } = "";
    [BindProperty] public string  NuevaBilletera        { get; set; } = "";
    [BindProperty] public string  NuevaCatNombre        { get; set; } = "";
    [BindProperty] public int     NuevaCatTipoId        { get; set; } = 2;
    [BindProperty] public int     EditCatId             { get; set; }
    [BindProperty] public string  EditCatNombre         { get; set; } = "";
    [BindProperty] public int     EditCatTipoId         { get; set; } = 2;
    [BindProperty] public string  NuevoTipoDolar        { get; set; } = "blue";
    [BindProperty] public decimal CotizacionManualInput { get; set; }
    [BindProperty] public string  NuevoTipoNombre       { get; set; } = "";
    [BindProperty] public int     EditTipoId            { get; set; }
    [BindProperty] public string  EditTipoNombre        { get; set; } = "";

    // CotizacionConfig form bindings
    [BindProperty] public string  TipoDolar    { get; set; } = "blue";
    [BindProperty] public string  ApiUrl       { get; set; } = "https://dolarapi.com/v1/dolares";
    [BindProperty] public bool    UsarApi      { get; set; } = true;
    [BindProperty] public decimal ManualValor  { get; set; } = 0;

    public async Task OnGetAsync()
    {
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
        if (!string.IsNullOrWhiteSpace(NuevoBanco)) await svc.AddBancoAsync(NuevoBanco);
        return RedirectToPage(null, "bancos");
    }

    public async Task<IActionResult> OnPostEliminarBancoAsync(int id)
    {
        await svc.DeleteBancoAsync(id);
        return RedirectToPage(null, "bancos");
    }

    public async Task<IActionResult> OnPostAgregarBilleteraAsync()
    {
        if (!string.IsNullOrWhiteSpace(NuevaBilletera)) await svc.AddBilleteraAsync(NuevaBilletera);
        return RedirectToPage(null, "billeteras");
    }

    public async Task<IActionResult> OnPostEliminarBilleteraAsync(int id)
    {
        await svc.DeleteBilleteraAsync(id);
        return RedirectToPage(null, "billeteras");
    }

    // ── Categorías ────────────────────────────────────────────────
    public async Task<IActionResult> OnPostAgregarCatAsync()
    {
        if (!string.IsNullOrWhiteSpace(NuevaCatNombre))
            await gastoSvc.AgregarCategoriaAsync(NuevaCatNombre, NuevaCatTipoId);
        return RedirectToPage(null, "categorias");
    }

    public async Task<IActionResult> OnPostEditarCatAsync()
    {
        if (EditCatId > 0 && !string.IsNullOrWhiteSpace(EditCatNombre))
            await gastoSvc.EditarCategoriaAsync(EditCatId, EditCatNombre, EditCatTipoId);
        return RedirectToPage(null, "categorias");
    }

    public async Task<IActionResult> OnPostDeshabilitarCatAsync(int id)
    {
        await gastoSvc.DeshabilitarOEliminarAsync(id);
        return RedirectToPage(null, "categorias");
    }

    public async Task<IActionResult> OnPostHabilitarCatAsync(int id)
    {
        await gastoSvc.HabilitarCategoriaAsync(id);
        return RedirectToPage(null, "categorias");
    }

    public async Task<IActionResult> OnPostReintentarCotizacionAsync()
    {
        var res = await cotizacionSvc.ReintentarAsync();
        if (res is { FuenteTipo: "api" })
            return new JsonResult(new { ok = true,  valor = res.Valor, fecha = res.FechaUltimaApi?.ToString("dd/MM/yyyy HH:mm") });
        return new JsonResult(new { ok = false });
    }

    public async Task<IActionResult> OnPostGuardarCotizacionAsync()
    {
        var config = await cotizacionRepo.GetConfigAsync();
        config.TipoDolar        = TipoDolar;
        config.ApiUrl           = ApiUrl;
        config.UsarApi          = UsarApi;
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
        Redes        = await svc.GetRedesAsync();
        Bancos       = await svc.GetBancosAsync();
        Billeteras   = await svc.GetBilleterasAsync();
        Categorias   = await gastoSvc.GetTodasCategoriasAsync();
        TiposIngreso = await ingresoSvc.GetTodosTiposAsync();
        Config           = await cotizacionRepo.GetConfigAsync();
        TipoDolarActual  = Config.TipoDolar;
        TipoDolar        = Config.TipoDolar;
        ApiUrl           = Config.ApiUrl;
        UsarApi          = Config.UsarApi;
        ManualValor      = Config.CotizacionManual;
        var cotizRes         = await cotizacionSvc.GetCotizacionConFuenteAsync();
        CotizacionActual     = cotizRes?.Valor;
        FuenteCotizacion     = cotizRes?.Fuente;
        FuenteCotizacionTipo = cotizRes?.FuenteTipo;
        FechaUltimaApi       = cotizRes?.FechaUltimaApi;
        CotizacionManual = Config.CotizacionManual > 0 ? Config.CotizacionManual : null;
        ViewData["Active"] = "configuracion";
    }
}
