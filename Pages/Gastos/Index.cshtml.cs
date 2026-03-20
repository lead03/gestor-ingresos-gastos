using ControlGastos.Services;
using ControlGastos.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ControlGastos.Pages.Gastos;

public class IndexModel(GastoService svc) : PageModel
{
    public GastoListVM VM { get; set; } = null!;

    public async Task OnGetAsync(int mes = 0, int anio = 0)
    {
        mes  = mes  == 0 ? DateTime.Today.Month : mes;
        anio = anio == 0 ? DateTime.Today.Year  : anio;
        VM = await svc.GetListAsync(mes, anio);
        ViewData["Active"] = "gastos";
        ViewData["Mes"] = mes;
        ViewData["Anio"] = anio;
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id, int mes, int anio)
    {
        await svc.DeleteAsync(id);
        return RedirectToPage(new { mes, anio });
    }
}
