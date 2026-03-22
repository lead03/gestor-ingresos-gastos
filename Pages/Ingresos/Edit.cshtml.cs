using ControlGastos.Services;
using ControlGastos.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ControlGastos.Pages.Ingresos;

public class EditModel(IngresoService svc) : PageModel
{
    [BindProperty] public IngresoFormVM VM { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int? id, int mes = 0, int anio = 0)
    {
        VM = await svc.GetFormAsync(id);
        if (mes  > 0) VM.Mes  = mes;
        if (anio > 0) VM.Anio = anio;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) { await RecargarListasAsync(); return Page(); }
        var result = await svc.SaveAsync(VM);
        if (!result.Success)
        {
            ModelState.AddModelError("", result.Error ?? "Error al guardar.");
            await RecargarListasAsync();
            return Page();
        }
        return RedirectToPage("./Index", new { mes = VM.Mes, anio = VM.Anio });
    }

    private async Task RecargarListasAsync()
    {
        var fresh = await svc.GetFormAsync();
        VM.Tipos   = fresh.Tipos;
        VM.Cuentas = fresh.Cuentas;
    }
}
