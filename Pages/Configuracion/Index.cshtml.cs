using ControlGastos.Models;
using ControlGastos.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ControlGastos.Pages.Configuracion;

public class IndexModel(ConfiguracionService svc, GastoService gastoSvc) : PageModel
{
    public List<ConfigOpcion>   Redes      { get; set; } = new();
    public List<ConfigOpcion>   Bancos     { get; set; } = new();
    public List<CategoriaGasto> Categorias { get; set; } = new();

    [BindProperty] public string NuevaRed           { get; set; } = "";
    [BindProperty] public string NuevoBanco         { get; set; } = "";
    [BindProperty] public string NuevaCatNombre     { get; set; } = "";
    [BindProperty] public string NuevaCatTipo       { get; set; } = "Variable";
    [BindProperty] public int    EditCatId          { get; set; }
    [BindProperty] public string EditCatNombre      { get; set; } = "";
    [BindProperty] public string EditCatTipo        { get; set; } = "Variable";

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

    private async Task CargarAsync()
    {
        Redes      = await svc.GetRedesAsync();
        Bancos     = await svc.GetBancosAsync();
        Categorias = await gastoSvc.GetTodasCategoriasAsync();
        ViewData["Active"] = "configuracion";
    }
}
