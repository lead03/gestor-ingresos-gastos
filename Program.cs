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

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}
if (!app.Environment.IsDevelopment())
    app.UseExceptionHandler("/Error");
app.UseStaticFiles();
app.UseRouting();
app.MapRazorPages();
app.Run();
