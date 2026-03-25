using ControlGastos.Repositories;
using ControlGastos.Services;
using ControlGastos.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace ControlGastos.Pages.Personas;

public class DetalleModel(PersonaService svc, ICuentaRepository cuentaRepo) : PageModel
{
    public PersonaDetalleVM VM { get; set; } = null!;
    public string CuentasJson  { get; set; } = "[]";

    [BindProperty] public PagoPersonaFormVM PagoForm { get; set; } = new();

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

        var cuentas = await cuentaRepo.GetAllActivasAsync();
        CuentasJson = JsonSerializer.Serialize(
            cuentas.Select(c => new { id = c.Id, nombre = c.Nombre, moneda = c.Moneda.ToString() }));

        ViewData["Active"] = "personas";
        ViewData["Mes"]    = m;
        ViewData["Anio"]   = a;
        return Page();
    }

    public async Task<IActionResult> OnPostPagoAsync(int id, int mes, int anio)
    {
        var result = await svc.SavePagoAsync(PagoForm);
        if (!result.Success)
            TempData["Error"] = result.Error;

        return RedirectToPage(new { id, mes, anio });
    }

    public async Task<IActionResult> OnPostEditPagoAsync(int id, int mes, int anio)
    {
        var result = await svc.EditPagoAsync(PagoForm);
        if (!result.Success)
            TempData["Error"] = result.Error;

        return RedirectToPage(new { id, mes, anio });
    }
}
