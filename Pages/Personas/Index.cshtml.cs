using ControlGastos.Services;
using ControlGastos.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ControlGastos.Pages.Personas;

public class IndexModel(PersonaService svc) : PageModel
{
    public PersonaListVM VM { get; set; } = null!;

    [BindProperty]
    public PersonaFormVM Form { get; set; } = new();

    public async Task OnGetAsync()
    {
        VM = await svc.GetListAsync();
        ViewData["Active"] = "personas";
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
}
