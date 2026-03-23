using ControlGastos.Models;
using Microsoft.EntityFrameworkCore;

namespace ControlGastos.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<CategoriaGasto>    CategoriasGasto    => Set<CategoriaGasto>();
    public DbSet<GastoItem>         Gastos             => Set<GastoItem>();
    public DbSet<GastoParticipante> GastoParticipantes => Set<GastoParticipante>();
    public DbSet<Ingreso>           Ingresos           => Set<Ingreso>();
    public DbSet<Tarjeta>           Tarjetas           => Set<Tarjeta>();
    public DbSet<TarjetaCuota>      TarjetaCuotas      => Set<TarjetaCuota>();
    public DbSet<Cuenta>            Cuentas            => Set<Cuenta>();
    public DbSet<Deuda>             Deudas             => Set<Deuda>();
    public DbSet<Persona>           Personas           => Set<Persona>();
    public DbSet<EfectivoDesglose>    EfectivoDesgloses    => Set<EfectivoDesglose>();
    public DbSet<TarjetaFechaMensual> TarjetaFechasMensuales => Set<TarjetaFechaMensual>();
    public DbSet<TipoIngreso>         TiposIngreso           => Set<TipoIngreso>();
    public DbSet<IngresoDistribucion> IngresoDistribuciones  => Set<IngresoDistribucion>();
    public DbSet<ConfigOpcion> ConfigOpciones => Set<ConfigOpcion>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        // GastoItem → Categoria
        mb.Entity<GastoItem>()
            .HasOne(g => g.Categoria)
            .WithMany(c => c.Gastos)
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

        mb.Entity<ConfigOpcion>()
            .HasIndex(c => new { c.Tipo, c.Valor })
            .IsUnique();

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

        mb.Entity<GastoItem>()
            .Property(g => g.Moneda)
            .HasConversion<string>();

        mb.Entity<TarjetaCuota>()
            .Property(tc => tc.Moneda)
            .HasConversion<string>();

        mb.Entity<Deuda>()
            .Property(d => d.Direccion)
            .HasConversion<string>();

        mb.Entity<Deuda>()
            .Property(d => d.Estado)
            .HasConversion<string>();

        mb.Entity<Cuenta>()
            .Property(c => c.Tipo)
            .HasConversion<string>();

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
        mb.Entity<Tarjeta>().Property(t => t.LimiteCredito).HasPrecision(18, 2);

        // Seed categorías
        mb.Entity<CategoriaGasto>().HasData(
            new CategoriaGasto { Id = 1,  Nombre = "Alimentos",         Tipo = "Variable" },
            new CategoriaGasto { Id = 2,  Nombre = "Higiene/perfumería", Tipo = "Variable" },
            new CategoriaGasto { Id = 3,  Nombre = "Farmacia",           Tipo = "Variable" },
            new CategoriaGasto { Id = 4,  Nombre = "Salidas/delivery",   Tipo = "Variable" },
            new CategoriaGasto { Id = 5,  Nombre = "Forrajería",         Tipo = "Variable" },
            new CategoriaGasto { Id = 6,  Nombre = "Bancarios",          Tipo = "Variable" },
            new CategoriaGasto { Id = 7,  Nombre = "Automóvil",          Tipo = "Variable" },
            new CategoriaGasto { Id = 8,  Nombre = "Tarjeta de Naty",    Tipo = "Variable" },
            new CategoriaGasto { Id = 9,  Nombre = "Tienda XBOX",        Tipo = "Variable" },
            new CategoriaGasto { Id = 10, Nombre = "Tienda PS Store",    Tipo = "Variable" },
            new CategoriaGasto { Id = 11, Nombre = "Tienda Steam",       Tipo = "Variable" },
            new CategoriaGasto { Id = 12, Nombre = "Tienda Nintendo",    Tipo = "Variable" },
            new CategoriaGasto { Id = 13, Nombre = "E-Shops",            Tipo = "Variable" },
            new CategoriaGasto { Id = 14, Nombre = "Otros",              Tipo = "Variable" },
            new CategoriaGasto { Id = 15, Nombre = "AySA",               Tipo = "Fijo" },
            new CategoriaGasto { Id = 16, Nombre = "Bomba",              Tipo = "Fijo" },
            new CategoriaGasto { Id = 17, Nombre = "Electricidad",       Tipo = "Fijo" },
            new CategoriaGasto { Id = 18, Nombre = "Gas",                Tipo = "Fijo" },
            new CategoriaGasto { Id = 19, Nombre = "Municipio",          Tipo = "Fijo" },
            new CategoriaGasto { Id = 20, Nombre = "ARBA/AFIP",          Tipo = "Fijo" },
            new CategoriaGasto { Id = 21, Nombre = "Alquiler",           Tipo = "Fijo" },
            new CategoriaGasto { Id = 22, Nombre = "Telered",            Tipo = "Fijo" },
            new CategoriaGasto { Id = 23, Nombre = "Claro",              Tipo = "Fijo" },
            new CategoriaGasto { Id = 24, Nombre = "Seguro",             Tipo = "Fijo" },
            new CategoriaGasto { Id = 25, Nombre = "Contaduría",         Tipo = "Fijo" },
            new CategoriaGasto { Id = 26, Nombre = "Jeep plan",          Tipo = "Fijo" },
            new CategoriaGasto { Id = 27, Nombre = "Suscripciones",      Tipo = "Fijo" }
        );

        // Seed tarjetas
        mb.Entity<Tarjeta>().HasData(
            new Tarjeta { Id = 1, Nombre = "Galicia - VISA",           Banco = "Galicia",     Red = "VISA",       DiaCierre = 19, DiaVencimiento = 4  },
            new Tarjeta { Id = 2, Nombre = "Santander - VISA",         Banco = "Santander",   Red = "VISA",       DiaCierre = 12, DiaVencimiento = 3  },
            new Tarjeta { Id = 3, Nombre = "MercadoPago - MasterCard", Banco = "MercadoPago", Red = "Mastercard", DiaCierre = 12, DiaVencimiento = 17 },
            new Tarjeta { Id = 4, Nombre = "Santander - AMEX",         Banco = "Santander",   Red = "AMEX",       DiaCierre = 12, DiaVencimiento = 3  }
        );
    }
}
