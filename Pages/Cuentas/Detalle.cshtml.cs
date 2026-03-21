using ControlGastos.Services;
using ControlGastos.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ControlGastos.Pages.Cuentas;

public class DetalleModel(CuentaService svc) : PageModel
{
    public CuentaDetalleVM VM { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var vm = await svc.GetDetalleAsync(id);
        if (vm == null) return NotFound();
        VM = vm;
        ViewData["Active"] = "cuentas";
        return Page();
    }
}
