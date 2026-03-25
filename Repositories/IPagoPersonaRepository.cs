using ControlGastos.Models;

namespace ControlGastos.Repositories;

public interface IPagoPersonaRepository
{
    Task<List<PagoPersona>> GetByPersonaAsync(int personaId);
    Task<PagoPersona?>      GetByIdAsync(int id);
    Task AddAsync(PagoPersona pago);
    Task UpdateAsync(PagoPersona pago);
}
