using ControlGastos.Services;
using ControlGastos.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ControlGastos.Pages.Personas;

public class DetalleModel(PersonaService svc) : PageModel
{
    public PersonaDetalleVM VM { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id, int mes = 0, int anio = 0)
    {
        int m = mes  == 0 ? DateTime.Today.Month : mes;
        int a = anio == 0 ? DateTime.Today.Year  : anio;
        try
        {
            VM = await svc.GetDetalleAsync(id, m, a);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        ViewData["Active"] = "personas";
        ViewData["Mes"]    = m;
        ViewData["Anio"]   = a;
        return Page();
    }
}
