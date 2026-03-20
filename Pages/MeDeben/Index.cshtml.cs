using ControlGastos.Services;
using ControlGastos.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ControlGastos.Pages.MeDeben;

public class IndexModel(DeudaService svc) : PageModel
{
    public DeudaListVM VM { get; set; } = null!;

    [BindProperty]
    public DeudaFormVM Form { get; set; } = new();

    public async Task OnGetAsync()
    {
        VM = await svc.GetListAsync();
        ViewData["Active"] = "medeben";
    }

    public async Task<IActionResult> OnPostSaveAsync()
    {
        await svc.SaveAsync(Form);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        await svc.DeleteAsync(id);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostMarcarPagadaAsync(int id, decimal montoPagado)
    {
        var lista = await svc.GetListAsync();
        var deuda = lista.MeDeben.Concat(lista.LeDebo).FirstOrDefault(x => x.Id == id);
        if (deuda != null)
        {
            await svc.SaveAsync(new DeudaFormVM
            {
                Id           = deuda.Id,
                NombrePersona= deuda.NombrePersona,
                PersonaId    = deuda.PersonaId,
                Monto        = deuda.Monto,
                Fecha        = deuda.Fecha,
                Descripcion  = deuda.Descripcion,
                Direccion    = deuda.Direccion,
                Estado       = montoPagado >= deuda.Monto ? "Pagada" : "Parcial",
                MontoPagado  = montoPagado
            });
        }
        return RedirectToPage();
    }
}
