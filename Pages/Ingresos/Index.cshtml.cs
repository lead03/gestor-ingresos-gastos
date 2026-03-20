using ControlGastos.Services;
using ControlGastos.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ControlGastos.Pages.Ingresos;

public class IndexModel(IngresoService svc) : PageModel
{
    public IngresoListVM VM { get; set; } = null!;

    public async Task OnGetAsync(int mes = 0, int anio = 0)
    {
        mes  = mes  == 0 ? DateTime.Today.Month : mes;
        anio = anio == 0 ? DateTime.Today.Year  : anio;
        VM = await svc.GetListAsync(mes, anio);
        ViewData["Active"] = "ingresos";
        ViewData["Mes"]    = mes;
        ViewData["Anio"]   = anio;
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id, int mes, int anio)
    {
        await svc.DeleteAsync(id);
        return RedirectToPage(new { mes, anio });
    }
}

public class EditModel(IngresoService svc) : PageModel
{
    [BindProperty]
    public IngresoFormVM VM { get; set; } = new();

    public void OnGet(int mes = 0, int anio = 0)
    {
        VM.Mes  = mes  == 0 ? DateTime.Today.Month : mes;
        VM.Anio = anio == 0 ? DateTime.Today.Year  : anio;
        ViewData["Active"] = "ingresos";
        ViewData["Mes"]    = VM.Mes;
        ViewData["Anio"]   = VM.Anio;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await svc.SaveAsync(VM);
        return RedirectToPage("./Index", new { mes = VM.Mes, anio = VM.Anio });
    }
}
