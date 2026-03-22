using ControlGastos.Models;
using ControlGastos.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ControlGastos.Pages.Configuracion;

public class IndexModel(ConfiguracionService svc, GastoService gastoSvc, CotizacionService cotizacionSvc, IngresoService ingresoSvc) : PageModel
{
    public List<ConfigOpcion>   Redes      { get; set; } = new();
    public List<ConfigOpcion>   Bancos     { get; set; } = new();
    public List<CategoriaGasto> Categorias { get; set; } = new();
    public List<TipoIngreso>    TiposIngreso { get; set; } = new();

    public string   TipoDolarActual       { get; set; } = "blue";
    public decimal? CotizacionActual      { get; set; }
    public decimal? CotizacionManual      { get; set; }
    public string? FuenteCotizacion    { get; set; }
    public string? FuenteCotizacionTipo { get; set; }

    [BindProperty] public string  NuevaRed              { get; set; } = "";
    [BindProperty] public string  NuevoBanco            { get; set; } = "";
    [BindProperty] public string  NuevaCatNombre        { get; set; } = "";
    [BindProperty] public string  NuevaCatTipo          { get; set; } = "Variable";
    [BindProperty] public int     EditCatId             { get; set; }
    [BindProperty] public string  EditCatNombre         { get; set; } = "";
    [BindProperty] public string  EditCatTipo           { get; set; } = "Variable";
    [BindProperty] public string  NuevoTipoDolar        { get; set; } = "blue";
    [BindProperty] public decimal CotizacionManualInput { get; set; }
    [BindProperty] public string  NuevoTipoNombre       { get; set; } = "";
    [BindProperty] public int     EditTipoId            { get; set; }
    [BindProperty] public string  EditTipoNombre        { get; set; } = "";

    public async Task OnGetAsync()
    {
        await CargarAsync();
    }

    // ── Config opciones ───────────────────────────────────────────
    public async Task<IActionResult> OnPostAgregarRedAsync()
    {
        if (!string.IsNullOrWhiteSpace(NuevaRed)) await svc.AddRedAsync(NuevaRed);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostAgregarBancoAsync()
    {
        if (!string.IsNullOrWhiteSpace(NuevoBanco)) await svc.AddBancoAsync(NuevoBanco);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostEliminarAsync(int id)
    {
        await svc.DeleteAsync(id);
        return RedirectToPage();
    }

    // ── Categorías ────────────────────────────────────────────────
    public async Task<IActionResult> OnPostAgregarCatAsync()
    {
        if (!string.IsNullOrWhiteSpace(NuevaCatNombre))
            await gastoSvc.AgregarCategoriaAsync(NuevaCatNombre, NuevaCatTipo);
        return RedirectToPage(null, "categorias");
    }

    public async Task<IActionResult> OnPostEditarCatAsync()
    {
        if (EditCatId > 0 && !string.IsNullOrWhiteSpace(EditCatNombre))
            await gastoSvc.EditarCategoriaAsync(EditCatId, EditCatNombre, EditCatTipo);
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

    public async Task<IActionResult> OnPostGuardarCotizacionAsync()
    {
        await cotizacionSvc.SaveTipoDolarAsync(NuevoTipoDolar);
        if (CotizacionManualInput > 0)
            await cotizacionSvc.SaveCotizacionManualAsync(CotizacionManualInput);
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
        Redes      = await svc.GetRedesAsync();
        Bancos     = await svc.GetBancosAsync();
        Categorias = await gastoSvc.GetTodasCategoriasAsync();
        TiposIngreso = await ingresoSvc.GetTodosTiposAsync();
        TipoDolarActual  = await cotizacionSvc.GetTipoDolarAsync();
        var cotizRes         = await cotizacionSvc.GetCotizacionConFuenteAsync();
        CotizacionActual     = cotizRes?.Valor;
        FuenteCotizacion     = cotizRes?.Fuente;
        FuenteCotizacionTipo = cotizRes?.FuenteTipo;
        CotizacionManual = await cotizacionSvc.GetCotizacionManualAsync();
        ViewData["Active"] = "configuracion";
    }
}
