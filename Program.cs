using ControlGastos.Data;
using ControlGastos.Repositories;
using ControlGastos.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages();
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlite("DataSource=gastos_prueba.db"));

// Repositories
builder.Services.AddScoped<IGastoRepository,             GastoRepository>();
builder.Services.AddScoped<IGastoParticipanteRepository, GastoParticipanteRepository>();
builder.Services.AddScoped<IIngresoRepository,           IngresoRepository>();
builder.Services.AddScoped<ITarjetaRepository,           TarjetaRepository>();
builder.Services.AddScoped<IDeudaRepository,             DeudaRepository>();
builder.Services.AddScoped<ICuentaRepository,            CuentaRepository>();
builder.Services.AddScoped<IPersonaRepository,           PersonaRepository>();

// Services
builder.Services.AddScoped<GastoService>();
builder.Services.AddScoped<IngresoService>();
builder.Services.AddScoped<TarjetaService>();
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<DeudaService>();
builder.Services.AddScoped<PersonaService>();
builder.Services.AddScoped<CuentaService>();
builder.Services.AddScoped<IConfiguracionRepository, ConfiguracionRepository>();
builder.Services.AddScoped<ConfiguracionService>();

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
    // Migración manual: agregar columna LimiteCredito si no existe
    try { db.Database.ExecuteSqlRaw("ALTER TABLE Tarjetas ADD COLUMN LimiteCredito REAL"); } catch { /* ya existe */ }

    // Migración manual: tabla de fechas mensuales por tarjeta
    try
    {
        db.Database.ExecuteSqlRaw("""
            CREATE TABLE IF NOT EXISTS TarjetaFechasMensuales (
                Id             INTEGER PRIMARY KEY AUTOINCREMENT,
                TarjetaId      INTEGER NOT NULL,
                Mes            INTEGER NOT NULL,
                Anio           INTEGER NOT NULL,
                DiaCierre      INTEGER NOT NULL,
                DiaVencimiento INTEGER NOT NULL,
                FOREIGN KEY (TarjetaId) REFERENCES Tarjetas(Id),
                UNIQUE (TarjetaId, Mes, Anio)
            )
            """);
    } catch { /* ya existe */ }

    try { db.Database.ExecuteSqlRaw("ALTER TABLE TarjetaCuotas ADD COLUMN GastoItemId INTEGER"); } catch { /* ya existe */ }

    // Migración manual: tabla de opciones configurables
    try
    {
        db.Database.ExecuteSqlRaw("""
            CREATE TABLE IF NOT EXISTS ConfigOpciones (
                Id     INTEGER PRIMARY KEY AUTOINCREMENT,
                Tipo   TEXT    NOT NULL,
                Valor  TEXT    NOT NULL,
                Orden  INTEGER NOT NULL DEFAULT 0,
                UNIQUE (Tipo, Valor)
            )
            """);
    } catch { /* ya existe */ }

    // Seed de opciones por defecto (si la tabla está vacía)
    var hayRedes  = db.ConfigOpciones.Any(c => c.Tipo == "Red");
    var hayBancos = db.ConfigOpciones.Any(c => c.Tipo == "Banco");

    if (!hayRedes)
    {
        string[] redes = ["VISA", "Mastercard", "AMEX", "Naranja", "Cabal"];
        for (int i = 0; i < redes.Length; i++)
            db.ConfigOpciones.Add(new ControlGastos.Models.ConfigOpcion { Tipo = "Red", Valor = redes[i], Orden = i + 1 });
        db.SaveChanges();
    }
    if (!hayBancos)
    {
        string[] bancos = ["Galicia", "Santander", "MercadoPago", "Macro", "BBVA", "Provincia", "Nación", "HSBC", "ICBC", "Naranja X"];
        for (int i = 0; i < bancos.Length; i++)
            db.ConfigOpciones.Add(new ControlGastos.Models.ConfigOpcion { Tipo = "Banco", Valor = bancos[i], Orden = i + 1 });
        db.SaveChanges();
    }
    try { db.Database.ExecuteSqlRaw("ALTER TABLE CategoriasGasto ADD COLUMN Habilitada INTEGER NOT NULL DEFAULT 1"); } catch { /* ya existe */ }
}
if (!app.Environment.IsDevelopment())
    app.UseExceptionHandler("/Error");
app.UseStaticFiles();
app.UseRouting();
app.MapRazorPages();
app.Run();
