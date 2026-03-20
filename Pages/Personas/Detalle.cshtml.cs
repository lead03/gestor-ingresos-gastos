using ControlGastos.Services;
using ControlGastos.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ControlGastos.Pages.Personas;

public class DetalleModel(PersonaService svc) : PageModel
{
    public PersonaDetalleVM VM { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        try
        {
            VM = await svc.GetDetalleAsync(id);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        ViewData["Active"] = "personas";
        return Page();
    }
}
