using ControlGastos.Data;
using ControlGastos.Repositories;
using ControlGastos.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages();
builder.Services.AddHttpClient("dolarapi");
builder.Services.AddMemoryCache();
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
builder.Services.AddScoped<CotizacionService>();

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

    // Tabla Monedas (lookup) + seed
    try
    {
        db.Database.ExecuteSqlRaw("""
            CREATE TABLE IF NOT EXISTS Monedas (
                Id      INTEGER PRIMARY KEY,
                Codigo  TEXT NOT NULL,
                Nombre  TEXT NOT NULL,
                Simbolo TEXT NOT NULL
            )
            """);
    } catch { }
    try { db.Database.ExecuteSqlRaw("INSERT OR IGNORE INTO Monedas (Id, Codigo, Nombre, Simbolo) VALUES (1,'ARS','Peso Argentino','$')");   } catch { }
    try { db.Database.ExecuteSqlRaw("INSERT OR IGNORE INTO Monedas (Id, Codigo, Nombre, Simbolo) VALUES (2,'USD','Dólar','U$D')"); } catch { }

    // MonedaId en GastoItems (migra desde columna Moneda TEXT)
    try { db.Database.ExecuteSqlRaw("ALTER TABLE GastoItems     ADD COLUMN MonedaId INTEGER NOT NULL DEFAULT 1"); } catch { }
    try { db.Database.ExecuteSqlRaw("UPDATE GastoItems     SET MonedaId = CASE WHEN Moneda = 'USD' THEN 2 ELSE 1 END WHERE MonedaId = 1"); } catch { }
    // MonedaId en TarjetaCuotas (migra desde columna Moneda TEXT)
    try { db.Database.ExecuteSqlRaw("ALTER TABLE TarjetaCuotas  ADD COLUMN MonedaId INTEGER NOT NULL DEFAULT 1"); } catch { }
    try { db.Database.ExecuteSqlRaw("UPDATE TarjetaCuotas  SET MonedaId = CASE WHEN Moneda = 'USD' THEN 2 ELSE 1 END WHERE MonedaId = 1"); } catch { }
    // MonedaId en Cuentas e Ingresos (columnas nuevas)
    try { db.Database.ExecuteSqlRaw("ALTER TABLE Cuentas        ADD COLUMN MonedaId INTEGER NOT NULL DEFAULT 1"); } catch { }
    try { db.Database.ExecuteSqlRaw("ALTER TABLE Ingresos       ADD COLUMN MonedaId INTEGER NOT NULL DEFAULT 1"); } catch { }

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
    try { db.Database.ExecuteSqlRaw("ALTER TABLE Gastos ADD COLUMN Moneda TEXT NOT NULL DEFAULT 'ARS'"); } catch { }
    try { db.Database.ExecuteSqlRaw("ALTER TABLE TarjetaCuotas ADD COLUMN Moneda TEXT NOT NULL DEFAULT 'ARS'"); } catch { }

    // Tabla TiposIngreso
    try { db.Database.ExecuteSqlRaw(@"
        CREATE TABLE IF NOT EXISTS TiposIngreso (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Nombre TEXT NOT NULL,
            Habilitada INTEGER NOT NULL DEFAULT 1
        )"); } catch { }

    // Seed TiposIngreso si está vacía
    try {
        db.Database.ExecuteSqlRaw(@"
            INSERT OR IGNORE INTO TiposIngreso (Id, Nombre, Habilitada) VALUES
            (1, 'Sueldo / Salario', 1),
            (2, 'Freelance', 1),
            (3, 'Alquiler cobrado', 1),
            (4, 'Ahorros', 1),
            (5, 'Dólares', 1),
            (6, 'Inversiones', 1),
            (7, 'Otros', 1)
        ");
    } catch { }

    // Tabla IngresoDistribuciones
    try { db.Database.ExecuteSqlRaw(@"
        CREATE TABLE IF NOT EXISTS IngresoDistribuciones (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            IngresoId INTEGER NOT NULL,
            CuentaId INTEGER NOT NULL,
            Monto TEXT NOT NULL DEFAULT '0',
            FOREIGN KEY (IngresoId) REFERENCES Ingresos(Id) ON DELETE CASCADE,
            FOREIGN KEY (CuentaId) REFERENCES Cuentas(Id)
        )"); } catch { }

    // Columna TipoIngresoId en Ingresos (DEFAULT 7 = Otros)
    try { db.Database.ExecuteSqlRaw("ALTER TABLE Ingresos ADD COLUMN TipoIngresoId INTEGER NOT NULL DEFAULT 7"); } catch { }
    // Migrar datos existentes: CuentaId → IngresoDistribuciones
    try {
        db.Database.ExecuteSqlRaw(@"
            INSERT OR IGNORE INTO IngresoDistribuciones (IngresoId, CuentaId, Monto)
            SELECT Id, CuentaId, Monto FROM Ingresos
            WHERE CuentaId IS NOT NULL
              AND Id NOT IN (SELECT DISTINCT IngresoId FROM IngresoDistribuciones)
        ");
    } catch { }

    // Crear cuenta Efectivo por defecto si no hay ninguna
    try {
        db.Database.ExecuteSqlRaw(@"
            INSERT OR IGNORE INTO Cuentas (Nombre, Tipo, SaldoInicial, Activa)
            SELECT 'Efectivo', 'Efectivo', 0, 1
            WHERE NOT EXISTS (SELECT 1 FROM Cuentas WHERE Activa = 1)
        ");
    } catch { }
}
if (!app.Environment.IsDevelopment())
    app.UseExceptionHandler("/Error");
app.UseStaticFiles();
app.UseRouting();
app.MapRazorPages();
app.Run();
