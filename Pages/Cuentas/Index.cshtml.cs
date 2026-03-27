using ControlGastos.Services;
using ControlGastos.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ControlGastos.Pages.Cuentas;

public class IndexModel(CuentaService svc, ConfiguracionService cfgSvc) : PageModel
{
    public CuentaListVM VM { get; set; } = null!;

    [BindProperty]
    public CuentaFormVM Form { get; set; } = new();

    public async Task OnGetAsync()
    {
        VM = await svc.GetListAsync();
        Form.OpcionesBanco     = await cfgSvc.GetBancosAsync();
        Form.OpcionesBilletera = await cfgSvc.GetBilleterasAsync();
        ViewData["Active"] = "cuentas";
    }

    public async Task<IActionResult> OnPostSaveAsync()
    {
        if (!ModelState.IsValid)
        {
            VM = await svc.GetListAsync();
            Form.OpcionesBanco     = await cfgSvc.GetBancosAsync();
            Form.OpcionesBilletera = await cfgSvc.GetBilleterasAsync();
            return Page();
        }

        var result = await svc.SaveAsync(Form);
        if (!result.Success)
            ModelState.AddModelError(string.Empty, result.Error!);

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        await svc.DeleteAsync(id);
        return RedirectToPage();
    }
}
