using ControlGastos.Models;
using ControlGastos.Services;
using ControlGastos.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ControlGastos.Pages.MeDeben;

public class IndexModel(DeudaService svc) : PageModel
{
    public DeudaListVM VM { get; set; } = null!;

    [BindProperty] public DeudaFormVM      Form      { get; set; } = new();
    [BindProperty] public DeudaCuotaFormVM CuotaForm { get; set; } = new();

    public async Task OnGetAsync(int mes = 0, int anio = 0)
    {
        mes  = mes  == 0 ? DateTime.Today.Month : mes;
        anio = anio == 0 ? DateTime.Today.Year  : anio;
        VM = await svc.GetListAsync(mes, anio);
        ViewData["Active"] = "medeben";
        ViewData["Mes"]    = mes;
        ViewData["Anio"]   = anio;
    }

    // ── Deuda ────────────────────────────────────────────────────────

    public async Task<IActionResult> OnPostSaveAsync(int mes, int anio)
    {
        await svc.SaveAsync(Form);
        return RedirectToPage(new { mes, anio });
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id, int mes, int anio)
    {
        await svc.DeleteAsync(id);
        return RedirectToPage(new { mes, anio });
    }

    public async Task<IActionResult> OnPostMarcarPagadaAsync(int id, decimal montoPagado, int mes, int anio)
    {
        var lista = await svc.GetListAsync(mes, anio);
        var deuda = lista.MeDeben.Concat(lista.LeDebo).FirstOrDefault(x => x.Id == id);
        if (deuda != null)
        {
            await svc.SaveAsync(new DeudaFormVM
            {
                Id            = deuda.Id,
                NombrePersona = deuda.NombrePersona,
                PersonaId     = deuda.PersonaId,
                Monto         = deuda.Monto,
                Fecha         = deuda.Fecha,
                Descripcion   = deuda.Descripcion,
                Direccion     = deuda.Direccion,
                Estado        = montoPagado >= deuda.Monto ? EstadoDeuda.Pagada : EstadoDeuda.Parcial,
                MontoPagado   = montoPagado,
                AceptaCuotas  = deuda.AceptaCuotas
            });
        }
        return RedirectToPage(new { mes, anio });
    }

    // ── Cuotas mensuales ─────────────────────────────────────────────

    public async Task<IActionResult> OnPostSaveCuotaAsync(int mes, int anio)
    {
        var result = await svc.SaveCuotaAsync(CuotaForm);
        if (!result.Success) TempData["Error"] = result.Error;
        return RedirectToPage(new { mes, anio });
    }

    public async Task<IActionResult> OnPostMarcarCuotaPagadaAsync(int id, int mes, int anio)
    {
        await svc.MarcarCuotaPagadaAsync(id);
        return RedirectToPage(new { mes, anio });
    }

    public async Task<IActionResult> OnPostDeleteCuotaAsync(int id, int mes, int anio)
    {
        await svc.DeleteCuotaAsync(id);
        return RedirectToPage(new { mes, anio });
    }
}
