using ControlGastos.Services;
using ControlGastos.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ControlGastos.Pages.Tarjetas;

public class IndexModel(TarjetaService svc) : PageModel
{
    public TarjetaResumenVM VM { get; set; } = null!;

    public async Task OnGetAsync(int mes = 0, int anio = 0, int? tarjetaId = null)
    {
        mes  = mes  == 0 ? DateTime.Today.Month : mes;
        anio = anio == 0 ? DateTime.Today.Year  : anio;
        VM = await svc.GetResumenAsync(mes, anio, tarjetaId);
        ViewData["Active"] = "tarjetas";
        ViewData["Mes"]    = mes;
        ViewData["Anio"]   = anio;
    }

    public async Task<IActionResult> OnPostAvanzarAsync(int tarjetaId, int mes, int anio)
    {
        await svc.AvanzarMesAsync(tarjetaId, mes, anio);
        return RedirectToPage(new { mes, anio });
    }

    public async Task<IActionResult> OnPostDeleteTarjetaAsync(int tarjetaId, int mes, int anio)
    {
        var result = await svc.DeleteTarjetaAsync(tarjetaId);
        if (!result.Success)
            TempData["Error"] = result.Error;
        return RedirectToPage(new { mes, anio, tab = "gestionar" });
    }
}

public class EditCuotaModel(TarjetaService svc) : PageModel
{
    [BindProperty]
    public TarjetaCuotaFormVM VM { get; set; } = null!;

    public async Task OnGetAsync(int? id, int mes = 0, int anio = 0)
    {
        mes  = mes  == 0 ? DateTime.Today.Month : mes;
        anio = anio == 0 ? DateTime.Today.Year  : anio;

        VM = new TarjetaCuotaFormVM
        {
            Tarjetas   = await svc.GetAllTarjetasAsync(),
            MesCierre  = mes,
            AnioCierre = anio
        };

        ViewData["Active"] = "tarjetas";
        ViewData["Mes"]    = mes;
        ViewData["Anio"]   = anio;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await svc.SaveCuotaAsync(VM);
        return RedirectToPage("./Index", new { mes = VM.MesCierre, anio = VM.AnioCierre });
    }
}

public class EditFechaMensualModel(TarjetaService svc) : PageModel
{
    [BindProperty]
    public FechaMensualFormVM VM { get; set; } = new();

    public async Task OnGetAsync(int tarjetaId, int mes, int anio)
    {
        var tarjeta = await svc.GetTarjetaByIdAsync(tarjetaId);
        var fechaMensual = await svc.GetFechaMensualAsync(tarjetaId, mes, anio);

        VM = new FechaMensualFormVM
        {
            TarjetaId      = tarjetaId,
            Mes            = mes,
            Anio           = anio,
            TarjetaNombre  = tarjeta?.Nombre ?? "",
            // Pre-cargar con fechas del mes si ya existen, sino con las de la tarjeta
            DiaCierre      = fechaMensual?.DiaCierre      ?? tarjeta?.DiaCierre      ?? 1,
            DiaVencimiento = fechaMensual?.DiaVencimiento ?? tarjeta?.DiaVencimiento ?? 1
        };

        ViewData["Active"] = "tarjetas";
        ViewData["Mes"]    = mes;
        ViewData["Anio"]   = anio;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        await svc.SaveFechaMensualAsync(VM);
        return RedirectToPage("./Index", new { mes = VM.Mes, anio = VM.Anio, tab = "gestionar" });
    }
}

public class EditTarjetaModel(TarjetaService svc, ConfiguracionService cfgSvc) : PageModel
{
    [BindProperty]
    public TarjetaFormVM VM { get; set; } = new();

    public async Task OnGetAsync(int? id)
    {
        if (id.HasValue)
        {
            var t = await svc.GetTarjetaByIdAsync(id.Value);
            if (t != null)
            {
                VM = new TarjetaFormVM
                {
                    Id             = t.Id,
                    Nombre         = t.Nombre,
                    Banco          = t.Banco,
                    RedTarjetaId   = t.RedTarjetaId,
                    DiaCierre      = t.DiaCierre,
                    DiaVencimiento = t.DiaVencimiento,
                    LimiteCredito  = t.LimiteCredito
                };
            }
        }
        VM.OpcionesRed   = await cfgSvc.GetRedesAsync();
        VM.OpcionesBanco = (await cfgSvc.GetBancosAsync()).Select(b => b.Nombre).ToList();
        ViewData["Active"] = "tarjetas";
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        var result = await svc.SaveTarjetaAsync(VM);
        if (!result.Success)
        {
            ModelState.AddModelError("", result.Error ?? "Error al guardar.");
            return Page();
        }
        return RedirectToPage("./Index", new { tab = "gestionar" });
    }
}
