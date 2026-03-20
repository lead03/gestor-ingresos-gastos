using ControlGastos.Models;
using ControlGastos.Repositories;
using ControlGastos.ViewModels;

namespace ControlGastos.Services;

public class PersonaService(
    IPersonaRepository           personaRepo,
    IGastoParticipanteRepository participanteRepo,
    IDeudaRepository             deudaRepo)
{
    public async Task<PersonaListVM> GetListAsync()
    {
        var personas    = await personaRepo.GetAllAsync();
        var todasDeudas = await deudaRepo.GetAllAsync();

        var resumen = new List<PersonaResumenVM>();

        foreach (var p in personas)
        {
            var participaciones = await participanteRepo.GetByPersonaAsync(p.Id);
            var deudas = todasDeudas.Where(d => d.PersonaId == p.Id).ToList();

            // Balance: suma de gastos compartidos + deudas directas
            decimal desdGastos = participaciones
                .Where(par => par.Tipo == "Persona")
                .Sum(par => par.Monto);

            decimal desdDeudas = deudas
                .Where(d => d.Estado != "Pagada")
                .Sum(d => d.Direccion == "MeDeben"
                    ? d.Monto - (d.MontoPagado ?? 0)
                    : -(d.Monto - (d.MontoPagado ?? 0)));

            resumen.Add(new PersonaResumenVM
            {
                Id               = p.Id,
                Nombre           = p.Nombre,
                Notas            = p.Notas,
                TotalMovimientos = participaciones.Count,
                Balance          = desdGastos + desdDeudas
            });
        }

        return new PersonaListVM { Personas = resumen };
    }

    public async Task<PersonaDetalleVM> GetDetalleAsync(int id)
    {
        var persona = await personaRepo.GetByIdWithDetalleAsync(id)
                      ?? throw new KeyNotFoundException($"Persona {id} no encontrada");

        var participaciones = await participanteRepo.GetByPersonaAsync(id);
        var deudas = persona.Deudas
                        .Where(d => d.Estado != "Pagada")
                        .ToList();

        // Balance total
        decimal desdGastos = participaciones
            .Where(p => p.Tipo == "Persona")
            .Sum(p => p.Monto);

        decimal desdDeudas = deudas.Sum(d =>
            d.Direccion == "MeDeben"
                ? d.Monto - (d.MontoPagado ?? 0)
                : -(d.Monto - (d.MontoPagado ?? 0)));

        // Agrupar participaciones por mes
        var porMes = participaciones
            .GroupBy(p => (p.GastoItem.Anio, p.GastoItem.Mes))
            .OrderByDescending(g => g.Key.Anio).ThenByDescending(g => g.Key.Mes)
            .Select(g => new PersonaMesVM
            {
                Anio           = g.Key.Anio,
                Mes            = g.Key.Mes,
                Total          = g.Sum(p => p.Monto),
                Participaciones= g.ToList()
            }).ToList();

        return new PersonaDetalleVM
        {
            Persona     = persona,
            Balance     = desdGastos + desdDeudas,
            PorMes      = porMes,
            Deudas      = deudas,
            TotalDeudas = desdDeudas
        };
    }

    public async Task SaveAsync(PersonaFormVM vm)
    {
        if (vm.Id == 0)
        {
            await personaRepo.AddAsync(new Persona
            {
                Nombre = vm.Nombre,
                Notas  = vm.Notas
            });
        }
        else
        {
            var p = await personaRepo.GetByIdAsync(vm.Id)
                    ?? throw new KeyNotFoundException($"Persona {vm.Id} no encontrada");
            p.Nombre = vm.Nombre;
            p.Notas  = vm.Notas;
            await personaRepo.UpdateAsync(p);
        }
    }

    public Task DeleteAsync(int id) => personaRepo.DeleteAsync(id);
}
