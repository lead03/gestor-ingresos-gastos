using ControlGastos.Data;
using ControlGastos.Models;
using Microsoft.EntityFrameworkCore;

namespace ControlGastos.Repositories;

public class DeudaRepository(AppDbContext db) : IDeudaRepository
{
    public Task<List<Deuda>> GetAllAsync() =>
        db.Deudas.OrderByDescending(d => d.Fecha).ToListAsync();

    public Task<Deuda?> GetByIdAsync(int id) =>
        db.Deudas.FindAsync(id).AsTask();

    public async Task AddAsync(Deuda deuda)
    {
        db.Deudas.Add(deuda);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Deuda deuda)
    {
        db.Deudas.Update(deuda);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var d = await db.Deudas.FindAsync(id);
        if (d != null)
        {
            db.Deudas.Remove(d);
            await db.SaveChangesAsync();
        }
    }
}
