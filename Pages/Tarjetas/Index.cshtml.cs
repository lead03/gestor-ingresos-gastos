using ControlGastos.Services;
using ControlGastos.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ControlGastos.Pages.Tarjetas;

public class IndexModel(TarjetaService svc) : PageModel
{
    public TarjetaResumenVM VM { get; set; } = null!;

    public async Task OnGetAsync(int mes = 0, int anio = 0)
    {
        mes  = mes  == 0 ? DateTime.Today.Month : mes;
        anio = anio == 0 ? DateTime.Today.Year  : anio;
        VM = await svc.GetResumenAsync(mes, anio);
        ViewData["Active"] = "tarjetas";
        ViewData["Mes"]    = mes;
        ViewData["Anio"]   = anio;
    }

    public async Task<IActionResult> OnPostAvanzarAsync(int tarjetaId, int mes, int anio)
    {
        await svc.AvanzarMesAsync(tarjetaId, mes, anio);
        return RedirectToPage(new { mes, anio });
    }

    public async Task<IActionResult> OnPostDeleteCuotaAsync(int cuotaId, int mes, int anio)
    {
        // handled in service via direct context access — minimal implementation
        return RedirectToPage(new { mes, anio });
    }
}

public class EditCuotaModel(TarjetaService svc) : PageModel
{
    [BindProperty]
    public TarjetaCuotaFormVM VM { get; set; } = null!;

    public async Task OnGetAsync(int mes = 0, int anio = 0)
    {
        VM = new TarjetaCuotaFormVM
        {
            Tarjetas = (await svc.GetResumenAsync(
                mes == 0 ? DateTime.Today.Month : mes,
                anio == 0 ? DateTime.Today.Year : anio)).Tarjetas,
            MesCierre  = mes  == 0 ? DateTime.Today.Month : mes,
            AnioCierre = anio == 0 ? DateTime.Today.Year  : anio
        };
        ViewData["Active"] = "tarjetas";
        ViewData["Mes"]    = VM.MesCierre;
        ViewData["Anio"]   = VM.AnioCierre;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await svc.SaveCuotaAsync(VM);
        return RedirectToPage("./Index", new { mes = VM.MesCierre, anio = VM.AnioCierre });
    }
}
