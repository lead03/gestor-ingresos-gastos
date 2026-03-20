# Control de Gastos — .NET 8 + IIS Local

Sistema de control de gastos e ingresos mensuales con gestión de tarjetas de crédito en cuotas, deudas y dashboard con gráficos.

---

## Requisitos

- .NET 8 SDK — https://dotnet.microsoft.com/download/dotnet/8
- SQL Server LocalDB (incluido con Visual Studio) — o SQL Server Express
- IIS con módulo ASP.NET Core (para deploy local) — o simplemente `dotnet run` para desarrollo

---

## Instalación rápida (desarrollo)

```bash
# 1. Restaurar paquetes
dotnet restore

# 2. Aplicar migraciones y crear la base de datos
dotnet ef database update

# 3. Ejecutar
dotnet run

# Abre: https://localhost:5001
```

> La migración inicial crea todas las tablas y carga las categorías de gasto y tarjetas por defecto.

---

## Deploy en IIS local

```bash
# 1. Publicar
dotnet publish -c Release -o ./publish

# 2. En IIS Manager:
#    - Crear nuevo sitio → apuntar a la carpeta ./publish
#    - Application Pool → Sin código administrado (No Managed Code)
#    - Puerto: el que prefieras (ej. 8080)
```

Asegurate de tener instalado el **ASP.NET Core Hosting Bundle** para IIS:
https://dotnet.microsoft.com/download/dotnet/8 → "Hosting Bundle"

---

## Estructura del proyecto

```
ControlGastos/
├── Models/          → Entidades de dominio
├── Data/            → AppDbContext + configuraciones EF Core
├── Services/        → Lógica de negocio (GastoService, etc.)
├── ViewModels/      → VMs para cada página
├── Migrations/      → Migración inicial con seed
├── Pages/
│   ├── Dashboard/   → KPIs + 3 gráficos Chart.js
│   ├── Gastos/      → Lista por día + formulario con división
│   ├── Ingresos/    → Lista + formulario por tipo
│   ├── Tarjetas/    → Resumen por tarjeta + gestión de cuotas
│   └── MeDeben/     → Me deben / le debo + modal inline
├── wwwroot/
│   └── css/site.css → Tema oscuro completo
└── web.config       → Configuración IIS
```

---

## Funcionalidades

### Gastos
- Registro por día con categoría (fijos / variables)
- **División automática**: checkbox "se divide" → calcula tu parte según cantidad de personas
- Vinculación opcional a cuota de tarjeta
- Vista agrupada por día con subtotal diario
- Edición y eliminación

### Ingresos
- 8 tipos: Propio, Distribuido, CuentaPropia, Ahorro, Dpto, USS, FIMA, Resto
- Totales por tipo en strip superior

### Tarjetas de crédito
- 4 tarjetas pre-cargadas (Galicia VISA, Santander VISA, MercadoPago MC, Santander AMEX)
- Gestión de compras en cuotas con cálculo automático del monto por cuota
- Registro de "paga parte" (otra persona te debe su parte)
- Botón "→ Siguiente mes" para propagar cuotas con saldo al mes siguiente
- Barra de progreso de cuotas pagadas

### Dashboard
- KPIs: ingresos, gastos (fijos/variables), balance, total cuentas, me deben
- Gráfico torta: gastos por categoría
- Gráfico barras: histórico 6 meses
- Gráfico línea: gastos vs ingresos por día

### Me deben / Le debo
- Registro bidireccional de deudas
- Estados: Activa / Parcial / Pagada
- Botón "✓ Marcar como pagada" de un click
- Neto a favor en KPIs

---

## Cadena de conexión

En `appsettings.json`:
```json
"ConnectionStrings": {
  "Default": "Server=(localdb)\\mssqllocaldb;Database=ControlGastosDB;Trusted_Connection=True;"
}
```

Para SQL Server Express, cambiar a:
```json
"Default": "Server=.\\SQLEXPRESS;Database=ControlGastosDB;Trusted_Connection=True;"
```
