using ControlGastos.Models;
using Microsoft.EntityFrameworkCore;

namespace ControlGastos.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{

    public DbSet<TipoCategoriaGasto> TiposCategoriaGasto => Set<TipoCategoriaGasto>();
    public DbSet<CategoriaGasto>    CategoriasGasto     => Set<CategoriaGasto>();
    public DbSet<GastoItem>         Gastos             => Set<GastoItem>();
    public DbSet<GastoParticipante> GastoParticipantes => Set<GastoParticipante>();
    public DbSet<Ingreso>           Ingresos           => Set<Ingreso>();
    public DbSet<Tarjeta>           Tarjetas           => Set<Tarjeta>();
    public DbSet<TarjetaCuota>      TarjetaCuotas      => Set<TarjetaCuota>();
    public DbSet<Cuenta>            Cuentas            => Set<Cuenta>();
    public DbSet<Deuda>      Deudas      => Set<Deuda>();
    public DbSet<DeudaCuota> DeudaCuotas => Set<DeudaCuota>();
    public DbSet<Persona>           Personas           => Set<Persona>();
    public DbSet<PagoPersona>       PagosPersona       => Set<PagoPersona>();
    public DbSet<EfectivoDesglose>    EfectivoDesgloses    => Set<EfectivoDesglose>();
    public DbSet<TarjetaFechaMensual> TarjetaFechasMensuales => Set<TarjetaFechaMensual>();
    public DbSet<TipoIngreso>         TiposIngreso           => Set<TipoIngreso>();
    public DbSet<IngresoDistribucion> IngresoDistribuciones  => Set<IngresoDistribucion>();
    public DbSet<MonedaItem>   Monedas        => Set<MonedaItem>();
    public DbSet<RedTarjeta> RedesTarjeta => Set<RedTarjeta>();
    public DbSet<Banco> Bancos => Set<Banco>();
    public DbSet<Billetera> Billeteras => Set<Billetera>();
    public DbSet<CotizacionConfig> CotizacionConfigs => Set<CotizacionConfig>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        // CategoriaGasto
        mb.Entity<CategoriaGasto>().Property(e => e.Nombre).HasMaxLength(80);

        // CategoriaGasto → TipoCategoriaGasto
        mb.Entity<CategoriaGasto>()
            .HasOne(c => c.Tipo)
            .WithMany(t => t.Categorias)
            .HasForeignKey(c => c.TipoId)
            .OnDelete(DeleteBehavior.Restrict);

        // GastoItem → CategoriaGasto
        mb.Entity<GastoItem>()
            .HasOne(g => g.Categoria)
            .WithMany(c => c.GastoItems)
            .HasForeignKey(g => g.CategoriaId);

        // GastoItem → Cuenta (medio de pago)
        mb.Entity<GastoItem>()
            .HasOne(g => g.Cuenta)
            .WithMany()
            .HasForeignKey(g => g.CuentaId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        // GastoItem → Tarjeta (medio de pago)
        mb.Entity<GastoItem>()
            .HasOne(g => g.Tarjeta)
            .WithMany()
            .HasForeignKey(g => g.TarjetaId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        // GastoItem → TarjetaCuota
        mb.Entity<GastoItem>()
            .HasOne(g => g.TarjetaCuota)
            .WithMany(tc => tc.GastosAsociados)
            .HasForeignKey(g => g.TarjetaCuotaId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        // GastoItem → Persona (Pagador — quien pagó en lugar del usuario)
        mb.Entity<GastoItem>()
            .HasOne(g => g.PagadorPersona)
            .WithMany()
            .HasForeignKey(g => g.PagadorPersonaId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        // GastoParticipante → GastoItem
        mb.Entity<GastoParticipante>()
            .HasOne(p => p.GastoItem)
            .WithMany(g => g.Participantes)
            .HasForeignKey(p => p.GastoItemId)
            .OnDelete(DeleteBehavior.Cascade);

        // GastoParticipante → Persona
        mb.Entity<GastoParticipante>()
            .HasOne(p => p.Persona)
            .WithMany(per => per.Participaciones)
            .HasForeignKey(p => p.PersonaId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        // Deuda → Persona
        mb.Entity<Deuda>()
            .HasOne(d => d.Persona)
            .WithMany(per => per.Deudas)
            .HasForeignKey(d => d.PersonaId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        // TarjetaCuota → Tarjeta
        mb.Entity<TarjetaCuota>()
            .HasOne(tc => tc.Tarjeta)
            .WithMany(t => t.Cuotas)
            .HasForeignKey(tc => tc.TarjetaId);

        // TarjetaFechaMensual → Tarjeta
        mb.Entity<TarjetaFechaMensual>()
            .HasOne(f => f.Tarjeta)
            .WithMany(t => t.FechasMensuales)
            .HasForeignKey(f => f.TarjetaId);

        mb.Entity<TarjetaFechaMensual>()
            .HasIndex(f => new { f.TarjetaId, f.Mes, f.Anio })
            .IsUnique();

        // RedTarjeta → Tarjeta
        mb.Entity<Tarjeta>()
            .HasOne(t => t.RedTarjeta)
            .WithMany(r => r.Tarjetas)
            .HasForeignKey(t => t.RedTarjetaId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        // Ingreso → TipoIngreso
        mb.Entity<Ingreso>()
            .HasOne(i => i.TipoIngreso)
            .WithMany(t => t.Ingresos)
            .HasForeignKey(i => i.TipoIngresoId);

        // IngresoDistribucion → Ingreso
        mb.Entity<IngresoDistribucion>()
            .HasOne(d => d.Ingreso)
            .WithMany(i => i.Distribuciones)
            .HasForeignKey(d => d.IngresoId)
            .OnDelete(DeleteBehavior.Cascade);

        // IngresoDistribucion → Cuenta
        mb.Entity<IngresoDistribucion>()
            .HasOne(d => d.Cuenta)
            .WithMany()
            .HasForeignKey(d => d.CuentaId)
            .OnDelete(DeleteBehavior.Restrict);

        mb.Entity<IngresoDistribucion>().Property(d => d.Monto).HasPrecision(18, 2);

        // Enum conversions — stored as string for readability
        mb.Entity<GastoParticipante>()
            .Property(p => p.Tipo)
            .HasConversion<string>();

        // Moneda: el enum se guarda como int (MonedaId) para mantener FK con la tabla Monedas
        mb.Entity<GastoItem>()
            .Property(g => g.Moneda)
            .HasColumnName("MonedaId")
            .HasConversion<int>();

        mb.Entity<TarjetaCuota>()
            .Property(tc => tc.Moneda)
            .HasColumnName("MonedaId")
            .HasConversion<int>();

        mb.Entity<Ingreso>()
            .Property(i => i.Moneda)
            .HasColumnName("MonedaId")
            .HasConversion<int>();

        mb.Entity<Cuenta>()
            .Property(c => c.Moneda)
            .HasColumnName("MonedaId")
            .HasConversion<int>();

        mb.Entity<Deuda>()
            .Property(d => d.Direccion)
            .HasConversion<string>();

        mb.Entity<Deuda>()
            .Property(d => d.Estado)
            .HasConversion<string>();

        // Cuenta → Banco
        mb.Entity<Cuenta>()
            .HasOne(c => c.Banco)
            .WithMany(b => b.Cuentas)
            .HasForeignKey(c => c.BancoId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        // Billetera
        mb.Entity<Billetera>().Property(e => e.Nombre).HasMaxLength(60);

        // Cuenta → Billetera
        mb.Entity<Cuenta>()
            .HasOne(c => c.Billetera)
            .WithMany(b => b.Cuentas)
            .HasForeignKey(c => c.BilleteraId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        // Cuenta.TipoEntidad — stored as string for readability
        mb.Entity<Cuenta>()
            .Property(c => c.TipoEntidad)
            .HasConversion<string>();

        // CotizacionConfig — singleton con Id=1 fijo
        mb.Entity<CotizacionConfig>()
            .Property(c => c.Id)
            .ValueGeneratedNever();
        mb.Entity<CotizacionConfig>().Property(c => c.CotizacionManual).HasPrecision(18, 2);
        mb.Entity<CotizacionConfig>().Property(c => c.UltimoValor).HasPrecision(18, 2);

        // Precisiones
        mb.Entity<GastoItem>().Property(g => g.Monto).HasPrecision(18, 2);
        mb.Entity<GastoItem>().Property(g => g.MiParte).HasPrecision(18, 2);
        mb.Entity<GastoParticipante>().Property(p => p.Monto).HasPrecision(18, 2);
        mb.Entity<Ingreso>().Property(i => i.Monto).HasPrecision(18, 2);
        mb.Entity<TarjetaCuota>().Property(tc => tc.MontoTotal).HasPrecision(18, 2);
        mb.Entity<TarjetaCuota>().Property(tc => tc.MontoCuota).HasPrecision(18, 2);
        mb.Entity<Cuenta>().Property(c => c.SaldoInicial).HasPrecision(18, 2);
        mb.Entity<Cuenta>().Property(c => c.AlertaSaldo).HasPrecision(18, 2);

        mb.Entity<Deuda>().Property(d => d.Monto).HasPrecision(18, 2);
        mb.Entity<Deuda>().Property(d => d.MontoPagado).HasPrecision(18, 2);

        // DeudaCuota → Deuda
        mb.Entity<DeudaCuota>()
            .HasOne(dc => dc.Deuda)
            .WithMany(d => d.Cuotas)
            .HasForeignKey(dc => dc.DeudaId)
            .OnDelete(DeleteBehavior.Cascade);

        mb.Entity<DeudaCuota>()
            .Property(dc => dc.Estado)
            .HasConversion<string>();

        mb.Entity<DeudaCuota>().Property(dc => dc.Monto).HasPrecision(18, 2);
        mb.Entity<DeudaCuota>().Property(dc => dc.MontoPagado).HasPrecision(18, 2);
        mb.Entity<Tarjeta>().Property(t => t.LimiteCredito).HasPrecision(18, 2);

        // PagoPersona → Persona
        mb.Entity<PagoPersona>()
            .HasOne(p => p.Persona)
            .WithMany()
            .HasForeignKey(p => p.PersonaId)
            .OnDelete(DeleteBehavior.Cascade);

        // PagoPersona → Cuenta (requerido)
        mb.Entity<PagoPersona>()
            .HasOne(p => p.Cuenta)
            .WithMany()
            .HasForeignKey(p => p.CuentaId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        mb.Entity<PagoPersona>()
            .Property(p => p.Moneda)
            .HasColumnName("MonedaId")
            .HasConversion<int>();

        mb.Entity<PagoPersona>().Property(p => p.Monto).HasPrecision(18, 2);
    }
}
