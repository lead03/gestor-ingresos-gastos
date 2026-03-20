using ControlGastos.Services;
using ControlGastos.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ControlGastos.Pages.Gastos;

public class EditModel(GastoService svc) : PageModel
{
    [BindProperty]
    public GastoFormVM VM { get; set; } = null!;

    public async Task OnGetAsync(int? id, int mes = 0, int anio = 0)
    {
        VM = await svc.GetFormAsync(id);
        if (mes != 0) VM.Mes = mes;
        if (anio != 0) VM.Anio = anio;
        ViewData["Active"] = "gastos";
        ViewData["Mes"] = VM.Mes;
        ViewData["Anio"] = VM.Anio;
        ViewData["Title"] = id.HasValue ? "Editar gasto" : "Nuevo gasto";
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            var form = await svc.GetFormAsync();
            VM.Categorias = form.Categorias;
            VM.Cuotas = form.Cuotas;
            VM.Personas = form.Personas;
            return Page();
        }

        await svc.SaveAsync(VM);
        return RedirectToPage("./Index", new { mes = VM.Mes, anio = VM.Anio });
    }
}