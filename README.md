# Control de Gastos — .NET 8 + IIS Local

Sistema de control de gastos e ingresos mensuales con gestión de tarjetas de crédito en cuotas, cuentas bancarias, personas, deudas con seguimiento mes a mes y dashboard con gráficos.

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
dotnet publish -c Release -o C:\inetpub\ControlGastos

# 2. En IIS Manager:
#    - Crear nuevo sitio → apuntar a C:\inetpub\ControlGastos
#    - Application Pool → Sin código administrado (No Managed Code)
#    - Puerto: el que prefieras (ej. 8080)

# 3. Permisos de carpeta
icacls "C:\inetpub\ControlGastos" /grant "IIS AppPool\ControlGastos:(OI)(CI)F"
```

Asegurate de tener instalado el **ASP.NET Core Hosting Bundle** para IIS:
https://dotnet.microsoft.com/download/dotnet/8 → "Hosting Bundle"

---

## Estructura del proyecto

```
ControlGastos/
├── Models/              → Entidades de dominio
├── Data/                → AppDbContext + configuraciones EF Core
├── Services/            → Lógica de negocio
├── ViewModels/          → VMs para cada página
├── Migrations/          → Migraciones EF Core con seed inicial
├── Pages/
│   ├── Dashboard/       → KPIs + 3 gráficos Chart.js
│   ├── Gastos/          → Lista por día + formulario con división y cuotas
│   ├── Ingresos/        → Lista + formulario con distribución en cuentas
│   ├── Tarjetas/        → Resumen por tarjeta + gestión de compras en cuotas
│   ├── Cuentas/         → Cuentas bancarias/billeteras con saldo calculado
│   ├── Personas/        → Personas compartidas + historial mes a mes
│   ├── MeDeben/         → Deudas bidireccionales + cuotas mensuales
│   └── Configuracion/   → Configuración general de la app
├── wwwroot/
│   └── css/site.css     → Tema oscuro completo
└── web.config           → Configuración IIS
```

---

## Funcionalidades

### Gastos
- Registro por día con categoría (fijos / variables)
- **División automática**: checkbox "se divide" → calcula tu parte según participantes
- Pago con cuenta bancaria o tarjeta de crédito (en cuotas)
- Vinculación a cuota de tarjeta de crédito
- Vista agrupada por día con subtotal diario
- Totales por tipo (fijos/variables) en ARS y USD
- Edición y eliminación

### Ingresos
- 8 tipos: Propio, Distribuido, CuentaPropia, Ahorro, Dpto, USS, FIMA, Resto
- Distribución en múltiples cuentas por ingreso
- Totales por tipo en strip superior

### Tarjetas de crédito
- Gestión de tarjetas con banco, red (VISA/Mastercard/AMEX), día de cierre y vencimiento
- Compras en cuotas con cálculo automático del monto por cuota
- Registro de "paga parte" (otra persona aporta su porción)
- Override de fechas de cierre/vencimiento por mes
- Barra de progreso de cuotas pagadas
- Propagación automática de cuotas a meses siguientes

### Cuentas
- Registro de cuentas bancarias, billeteras y efectivo
- Saldo calculado automáticamente desde movimientos de gastos e ingresos
- Alertas de saldo mínimo
- Historial de movimientos por cuenta

### Personas
- Registro de personas para gastos compartidos
- Balance total por persona (me debe / le debo / saldado)
- Historial mensual de participaciones, con desglose cuota a cuota para compras en tarjeta

### Me deben / Le debo
- Registro bidireccional de deudas
- Estados: Activa / Parcial / Pagada
- **Deudas con cuotas mensuales** (`AceptaCuotas`): seguimiento mes a mes del saldo
- Saldo virtual calculado desde participaciones en gastos compartidos
- Botón "✓ Marcar como pagada" de un click
- Neto a favor en KPIs del dashboard

### Dashboard
- KPIs: ingresos, gastos (fijos/variables), balance, total cuentas, me deben
- Cotización USD/ARS actualizada
- Gráfico torta: gastos por categoría
- Gráfico barras: histórico 6 meses
- Gráfico línea: gastos vs ingresos por día

---

## Stack y dependencias

| Paquete | Uso |
|---|---|
| `Microsoft.EntityFrameworkCore.SqlServer` | ORM + SQL Server |
| `Microsoft.EntityFrameworkCore.Sqlite` | Soporte SQLite alternativo |
| `ClosedXML` | Exportación a Excel |
| `FluentValidation.AspNetCore` | Validaciones de formularios |
| Chart.js (CDN) | Gráficos del dashboard |

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
