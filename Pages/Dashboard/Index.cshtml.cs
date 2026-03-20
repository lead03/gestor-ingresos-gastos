using ControlGastos.Services;
using ControlGastos.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ControlGastos.Pages.Dashboard;

public class IndexModel(DashboardService svc) : PageModel
{
    public DashboardVM VM { get; set; } = null!;

    public async Task OnGetAsync(int mes = 0, int anio = 0)
    {
        mes  = mes  == 0 ? DateTime.Today.Month : mes;
        anio = anio == 0 ? DateTime.Today.Year  : anio;
        VM = await svc.GetDashboardAsync(mes, anio);
        ViewData["Active"] = "dashboard";
        ViewData["Mes"]    = mes;
        ViewData["Anio"]   = anio;
    }
}
