using ControlGastos.Data;
using ControlGastos.Repositories;
using ControlGastos.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages();
builder.Services.AddHttpClient("dolarapi");
builder.Services.AddMemoryCache();
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlite(builder.Configuration.GetConnectionString("Default")));

// Repositories
builder.Services.AddScoped<IGastoRepository, GastoRepository>();
builder.Services.AddScoped<IGastoParticipanteRepository, GastoParticipanteRepository>();
builder.Services.AddScoped<IIngresoRepository, IngresoRepository>();
builder.Services.AddScoped<ITarjetaRepository, TarjetaRepository>();
builder.Services.AddScoped<IDeudaRepository, DeudaRepository>();
builder.Services.AddScoped<ICuentaRepository, CuentaRepository>();
builder.Services.AddScoped<IPersonaRepository, PersonaRepository>();
builder.Services.AddScoped<IBilleteraRepository, BilleteraRepository>();

// Services
builder.Services.AddScoped<GastoService>();
builder.Services.AddScoped<IngresoService>();
builder.Services.AddScoped<TarjetaService>();
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<DeudaService>();
builder.Services.AddScoped<PersonaService>();
builder.Services.AddScoped<CuentaService>();
builder.Services.AddScoped<ConfiguracionService>();
builder.Services.AddScoped<ICotizacionConfigRepository, CotizacionConfigRepository>();
builder.Services.AddScoped<CotizacionService>();
builder.Services.AddScoped<IPagoPersonaRepository, PagoPersonaRepository>();

var invariantCulture = System.Globalization.CultureInfo.InvariantCulture;
builder.Services.Configure<Microsoft.AspNetCore.Builder.RequestLocalizationOptions>(opts =>
{
    opts.DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture(invariantCulture);
    opts.SupportedCultures    = [invariantCulture];
    opts.SupportedUICultures  = [invariantCulture];
});

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();

    // Seed TiposCategoriaGasto
    if (!db.TiposCategoriaGasto.Any())
    {
        db.TiposCategoriaGasto.AddRange(
            new ControlGastos.Models.TipoCategoriaGasto { Id = 1, Nombre = "Fijo" },
            new ControlGastos.Models.TipoCategoriaGasto { Id = 2, Nombre = "Variable" }
        );
        db.SaveChanges();
    }

    // Seed Monedas
    if (!db.Monedas.Any())
    {
        db.Monedas.AddRange(
            new ControlGastos.Models.MonedaItem { Id = (int)ControlGastos.Models.Moneda.ARS, Codigo = "ARS", Nombre = "Peso Argentino", Simbolo = "$" },
            new ControlGastos.Models.MonedaItem { Id = (int)ControlGastos.Models.Moneda.USD, Codigo = "USD", Nombre = "Dólar", Simbolo = "U$D" }
        );
        db.SaveChanges();
    }

    // Seed TiposIngreso
    if (!db.TiposIngreso.Any())
    {
        db.TiposIngreso.AddRange(
            new ControlGastos.Models.TipoIngreso { Nombre = "Sueldo / Salario", Habilitada = true },
            new ControlGastos.Models.TipoIngreso { Nombre = "Freelance", Habilitada = true },
            new ControlGastos.Models.TipoIngreso { Nombre = "Alquiler cobrado", Habilitada = true },
            new ControlGastos.Models.TipoIngreso { Nombre = "Ahorros", Habilitada = true },
            new ControlGastos.Models.TipoIngreso { Nombre = "Inversiones", Habilitada = true }
        );
        db.SaveChanges();
    }

    // Seed Categorías
    if (!db.CategoriasGasto.Any())
    {
        db.CategoriasGasto.AddRange(
            // Fijo (TipoId = 1)
            new ControlGastos.Models.CategoriaGasto { Nombre = "Alquiler", TipoId = 1 },
            new ControlGastos.Models.CategoriaGasto { Nombre = "Expensas", TipoId = 1 },
            new ControlGastos.Models.CategoriaGasto { Nombre = "Salud prepaga", TipoId = 1 },
            new ControlGastos.Models.CategoriaGasto { Nombre = "Agua", TipoId = 1 },
            new ControlGastos.Models.CategoriaGasto { Nombre = "Gas", TipoId = 1 },
            new ControlGastos.Models.CategoriaGasto { Nombre = "Electricidad", TipoId = 1 },
            new ControlGastos.Models.CategoriaGasto { Nombre = "Internet", TipoId = 1 },
            new ControlGastos.Models.CategoriaGasto { Nombre = "Telefonía", TipoId = 1 },
            new ControlGastos.Models.CategoriaGasto { Nombre = "Seguro automotriz", TipoId = 1 },
            new ControlGastos.Models.CategoriaGasto { Nombre = "Seguro de vida", TipoId = 1 },
            new ControlGastos.Models.CategoriaGasto { Nombre = "Seguro de hogar", TipoId = 1 },
            new ControlGastos.Models.CategoriaGasto { Nombre = "Impuesto municipal", TipoId = 1 },
            new ControlGastos.Models.CategoriaGasto { Nombre = "Impuesto provincial", TipoId = 1 },
            new ControlGastos.Models.CategoriaGasto { Nombre = "Impuesto nacional", TipoId = 1 },
            new ControlGastos.Models.CategoriaGasto { Nombre = "Cuota del auto", TipoId = 1 },
            new ControlGastos.Models.CategoriaGasto { Nombre = "Educación", TipoId = 1 },
            new ControlGastos.Models.CategoriaGasto { Nombre = "Actividad física", TipoId = 1 },
            new ControlGastos.Models.CategoriaGasto { Nombre = "Servicio doméstico", TipoId = 1 },
            new ControlGastos.Models.CategoriaGasto { Nombre = "Suscripciones", TipoId = 1 },
            new ControlGastos.Models.CategoriaGasto { Nombre = "Cochera", TipoId = 1 },
            // Variable (TipoId = 2)
            new ControlGastos.Models.CategoriaGasto { Nombre = "Carnicería", TipoId = 2 },
            new ControlGastos.Models.CategoriaGasto { Nombre = "Verdulería", TipoId = 2 },
            new ControlGastos.Models.CategoriaGasto { Nombre = "Granja", TipoId = 2 },
            new ControlGastos.Models.CategoriaGasto { Nombre = "Pastas", TipoId = 2 },
            new ControlGastos.Models.CategoriaGasto { Nombre = "Fiambrería", TipoId = 2 },
            new ControlGastos.Models.CategoriaGasto { Nombre = "Mercado", TipoId = 2 },
            new ControlGastos.Models.CategoriaGasto { Nombre = "Limpieza", TipoId = 2 },
            new ControlGastos.Models.CategoriaGasto { Nombre = "Perfumería", TipoId = 2 },
            new ControlGastos.Models.CategoriaGasto { Nombre = "Forrajería", TipoId = 2 },
            new ControlGastos.Models.CategoriaGasto { Nombre = "Panadería", TipoId = 2 },
            new ControlGastos.Models.CategoriaGasto { Nombre = "Delivery", TipoId = 2 },
            new ControlGastos.Models.CategoriaGasto { Nombre = "Restaurante", TipoId = 2 },
            new ControlGastos.Models.CategoriaGasto { Nombre = "Ferretería", TipoId = 2 },
            new ControlGastos.Models.CategoriaGasto { Nombre = "Muebles", TipoId = 2 },
            new ControlGastos.Models.CategoriaGasto { Nombre = "Mercería", TipoId = 2 },
            new ControlGastos.Models.CategoriaGasto { Nombre = "Indumentaria", TipoId = 2 },
            new ControlGastos.Models.CategoriaGasto { Nombre = "Librería", TipoId = 2 },
            new ControlGastos.Models.CategoriaGasto { Nombre = "Heladería", TipoId = 2 },
            new ControlGastos.Models.CategoriaGasto { Nombre = "Veterinaria", TipoId = 2 },
            new ControlGastos.Models.CategoriaGasto { Nombre = "Electrónica", TipoId = 2 },
            new ControlGastos.Models.CategoriaGasto { Nombre = "Juguetería", TipoId = 2 },
            new ControlGastos.Models.CategoriaGasto { Nombre = "Floristería", TipoId = 2 },
            new ControlGastos.Models.CategoriaGasto { Nombre = "Óptica", TipoId = 2 },
            new ControlGastos.Models.CategoriaGasto { Nombre = "Cerrajería", TipoId = 2 },
            new ControlGastos.Models.CategoriaGasto { Nombre = "Tintorería", TipoId = 2 },
            new ControlGastos.Models.CategoriaGasto { Nombre = "Transporte", TipoId = 2 },
            new ControlGastos.Models.CategoriaGasto { Nombre = "Salud", TipoId = 2 },
            new ControlGastos.Models.CategoriaGasto { Nombre = "Educación", TipoId = 2 },
            new ControlGastos.Models.CategoriaGasto { Nombre = "Entretenimiento", TipoId = 2 },
            new ControlGastos.Models.CategoriaGasto { Nombre = "Arte", TipoId = 2 },
            new ControlGastos.Models.CategoriaGasto { Nombre = "Gastos bancarios", TipoId = 2 },
            new ControlGastos.Models.CategoriaGasto { Nombre = "Préstamo", TipoId = 2 },
            new ControlGastos.Models.CategoriaGasto { Nombre = "Otros", TipoId = 2 },
            new ControlGastos.Models.CategoriaGasto { Nombre = "Impuestos en compras", TipoId = 2 }
        );
        db.SaveChanges();
    }

    // Seed RedesTarjeta
    if (!db.RedesTarjeta.Any())
    {
        string[] redes = ["VISA", "Mastercard", "AMEX", "Naranja", "Cabal"];
        for (int i = 0; i < redes.Length; i++)
            db.RedesTarjeta.Add(new ControlGastos.Models.RedTarjeta { Nombre = redes[i], Orden = i + 1 });
        db.SaveChanges();
    }

    // Seed Bancos
    if (!db.Bancos.Any())
    {
        string[] bancos = ["Galicia", "Santander", "Macro", "BBVA", "Provincia", "Nación", "HSBC", "ICBC"];
        for (int i = 0; i < bancos.Length; i++)
            db.Bancos.Add(new ControlGastos.Models.Banco { Nombre = bancos[i], Orden = i + 1 });
        db.SaveChanges();
    }

    // Seed Billeteras
    if (!db.Billeteras.Any())
    {
        string[] billeteras = ["MercadoPago", "Naranja X", "Ualá", "Lemon", "Brubank", "Personal Pay"];
        for (int i = 0; i < billeteras.Length; i++)
            db.Billeteras.Add(new ControlGastos.Models.Billetera { Nombre = billeteras[i], Orden = i + 1 });
        db.SaveChanges();
    }

    // Seed cuenta Efectivo si no hay ninguna activa
    if (!db.Cuentas.Any(c => c.Activa))
    {
        db.Cuentas.Add(new ControlGastos.Models.Cuenta { Nombre = "Efectivo", TipoEntidad = ControlGastos.Models.TipoEntidad.Efectivo, SaldoInicial = 0, Activa = true });
        db.SaveChanges();
    }

    // Seed CotizacionConfig (singleton Id=1)
    if (!db.CotizacionConfigs.Any())
    {
        db.CotizacionConfigs.Add(new ControlGastos.Models.CotizacionConfig { Id = 1 });
        db.SaveChanges();
    }
}
if (!app.Environment.IsDevelopment())
    app.UseExceptionHandler("/Error");
app.UseRequestLocalization();
app.UseStaticFiles();
app.UseRouting();
app.MapRazorPages();
app.Run();
