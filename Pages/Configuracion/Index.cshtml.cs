using ControlGastos.Models;
using ControlGastos.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ControlGastos.Pages.Configuracion;

public class IndexModel(ConfiguracionService svc) : PageModel
{
    public List<ConfigOpcion> Redes  { get; set; } = new();
    public List<ConfigOpcion> Bancos { get; set; } = new();

    [BindProperty] public string NuevaRed   { get; set; } = "";
    [BindProperty] public string NuevoBanco { get; set; } = "";

    public async Task OnGetAsync()
    {
        await CargarAsync();
        ViewData["Active"] = "configuracion";
    }

    public async Task<IActionResult> OnPostAgregarRedAsync()
    {
        if (!string.IsNullOrWhiteSpace(NuevaRed))
            await svc.AddRedAsync(NuevaRed);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostAgregarBancoAsync()
    {
        if (!string.IsNullOrWhiteSpace(NuevoBanco))
            await svc.AddBancoAsync(NuevoBanco);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostEliminarAsync(int id)
    {
        await svc.DeleteAsync(id);
        return RedirectToPage();
    }

    private async Task CargarAsync()
    {
        Redes  = await svc.GetRedesAsync();
        Bancos = await svc.GetBancosAsync();
        ViewData["Active"] = "configuracion";
    }
}
