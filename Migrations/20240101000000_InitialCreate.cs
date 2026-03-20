using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControlGastos.Migrations;

public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder mb)
    {
        mb.CreateTable("CategoriasGasto", t => new
        {
            Id     = t.Column<int>(nullable: false).Annotation("SqlServer:Identity", "1, 1"),
            Nombre = t.Column<string>(maxLength: 80, nullable: false),
            Tipo   = t.Column<string>(nullable: false, defaultValue: "Variable"),
        }, constraints: t => t.PrimaryKey("PK_CategoriasGasto", x => x.Id));

        mb.CreateTable("Tarjetas", t => new
        {
            Id               = t.Column<int>(nullable: false).Annotation("SqlServer:Identity", "1, 1"),
            Nombre           = t.Column<string>(maxLength: 60, nullable: false),
            Banco            = t.Column<string>(maxLength: 30, nullable: false),
            Red              = t.Column<string>(maxLength: 20, nullable: false),
            DiaCierre        = t.Column<int>(nullable: false),
            DiaVencimiento   = t.Column<int>(nullable: false),
        }, constraints: t => t.PrimaryKey("PK_Tarjetas", x => x.Id));

        mb.CreateTable("TarjetaCuotas", t => new
        {
            Id              = t.Column<int>(nullable: false).Annotation("SqlServer:Identity", "1, 1"),
            TarjetaId       = t.Column<int>(nullable: false),
            Comercio        = t.Column<string>(maxLength: 150, nullable: false),
            FechaCompra     = t.Column<DateTime>(nullable: false),
            MontoTotal      = t.Column<decimal>(precision: 18, scale: 2, nullable: false),
            TotalCuotas     = t.Column<int>(nullable: false),
            MontoCuota      = t.Column<decimal>(precision: 18, scale: 2, nullable: false),
            MesCierre       = t.Column<int>(nullable: false),
            AnioCierre      = t.Column<int>(nullable: false),
            CuotasPagadas   = t.Column<int>(nullable: false),
            PagaParte       = t.Column<string>(maxLength: 10, nullable: false),
            MontoPagoOtro   = t.Column<decimal>(precision: 18, scale: 2, nullable: true),
        }, constraints: t =>
        {
            t.PrimaryKey("PK_TarjetaCuotas", x => x.Id);
            t.ForeignKey("FK_TarjetaCuotas_Tarjetas_TarjetaId", x => x.TarjetaId, "Tarjetas", "Id", onDelete: ReferentialAction.Cascade);
        });

        mb.CreateTable("Gastos", t => new
        {
            Id              = t.Column<int>(nullable: false).Annotation("SqlServer:Identity", "1, 1"),
            Mes             = t.Column<int>(nullable: false),
            Anio            = t.Column<int>(nullable: false),
            Dia             = t.Column<int>(nullable: false),
            CategoriaId     = t.Column<int>(nullable: false),
            Monto           = t.Column<decimal>(precision: 18, scale: 2, nullable: false),
            SeDivide        = t.Column<bool>(nullable: false),
            MontoDividido   = t.Column<decimal>(precision: 18, scale: 2, nullable: true),
            CantidadPersonas= t.Column<int>(nullable: true),
            Descripcion     = t.Column<string>(maxLength: 200, nullable: true),
            MedioPago       = t.Column<string>(maxLength: 50, nullable: true),
            TarjetaCuotaId  = t.Column<int>(nullable: true),
        }, constraints: t =>
        {
            t.PrimaryKey("PK_Gastos", x => x.Id);
            t.ForeignKey("FK_Gastos_CategoriasGasto_CategoriaId", x => x.CategoriaId, "CategoriasGasto", "Id", onDelete: ReferentialAction.Cascade);
            t.ForeignKey("FK_Gastos_TarjetaCuotas_TarjetaCuotaId", x => x.TarjetaCuotaId, "TarjetaCuotas", "Id", onDelete: ReferentialAction.SetNull);
        });

        mb.CreateTable("Ingresos", t => new
        {
            Id          = t.Column<int>(nullable: false).Annotation("SqlServer:Identity", "1, 1"),
            Mes         = t.Column<int>(nullable: false),
            Anio        = t.Column<int>(nullable: false),
            Dia         = t.Column<int>(nullable: false),
            Tipo        = t.Column<string>(maxLength: 50, nullable: false),
            Monto       = t.Column<decimal>(precision: 18, scale: 2, nullable: false),
            Descripcion = t.Column<string>(maxLength: 200, nullable: true),
            Fuente      = t.Column<string>(maxLength: 50, nullable: true),
        }, constraints: t => t.PrimaryKey("PK_Ingresos", x => x.Id));

        mb.CreateTable("Cuentas", t => new
        {
            Id          = t.Column<int>(nullable: false).Annotation("SqlServer:Identity", "1, 1"),
            Nombre      = t.Column<string>(maxLength: 60, nullable: false),
            SaldoInicio = t.Column<decimal>(precision: 18, scale: 2, nullable: false),
            SaldoFinal  = t.Column<decimal>(precision: 18, scale: 2, nullable: false),
            Mes         = t.Column<int>(nullable: false),
            Anio        = t.Column<int>(nullable: false),
        }, constraints: t => t.PrimaryKey("PK_Cuentas", x => x.Id));

        mb.CreateTable("Deudas", t => new
        {
            Id          = t.Column<int>(nullable: false).Annotation("SqlServer:Identity", "1, 1"),
            Persona     = t.Column<string>(maxLength: 80, nullable: false),
            Monto       = t.Column<decimal>(precision: 18, scale: 2, nullable: false),
            Fecha       = t.Column<DateTime>(nullable: false),
            Descripcion = t.Column<string>(maxLength: 200, nullable: true),
            Direccion   = t.Column<string>(nullable: false),
            Estado      = t.Column<string>(nullable: false),
            MontoPagado = t.Column<decimal>(precision: 18, scale: 2, nullable: true),
        }, constraints: t => t.PrimaryKey("PK_Deudas", x => x.Id));

        mb.CreateTable("EfectivoDesgloses", t => new
        {
            Id           = t.Column<int>(nullable: false).Annotation("SqlServer:Identity", "1, 1"),
            Mes          = t.Column<int>(nullable: false),
            Anio         = t.Column<int>(nullable: false),
            Denominacion = t.Column<int>(nullable: false),
            Cantidad     = t.Column<int>(nullable: false),
        }, constraints: t => t.PrimaryKey("PK_EfectivoDesgloses", x => x.Id));

        // Indexes
        mb.CreateIndex("IX_Gastos_CategoriaId",    "Gastos",       "CategoriaId");
        mb.CreateIndex("IX_Gastos_TarjetaCuotaId", "Gastos",       "TarjetaCuotaId");
        mb.CreateIndex("IX_TarjetaCuotas_TarjetaId","TarjetaCuotas","TarjetaId");

        // Seed categories
        mb.InsertData("CategoriasGasto", new[]{"Id","Nombre","Tipo"}, new object[,]
        {
            {1,"Alimentos","Variable"},       {2,"Higiene/perfumería","Variable"},
            {3,"Farmacia","Variable"},         {4,"Salidas/delivery","Variable"},
            {5,"Forrajería","Variable"},       {6,"Bancarios","Variable"},
            {7,"Automóvil","Variable"},        {8,"Tarjeta de Naty","Variable"},
            {9,"Tienda XBOX","Variable"},      {10,"Tienda PS Store","Variable"},
            {11,"Tienda Steam","Variable"},    {12,"Tienda Nintendo","Variable"},
            {13,"E-Shops","Variable"},         {14,"Otros","Variable"},
            {15,"AySA","Fijo"},               {16,"Bomba","Fijo"},
            {17,"Electricidad","Fijo"},        {18,"Gas","Fijo"},
            {19,"Municipio","Fijo"},           {20,"ARBA/AFIP","Fijo"},
            {21,"Alquiler","Fijo"},            {22,"Telered","Fijo"},
            {23,"Claro","Fijo"},               {24,"Seguro","Fijo"},
            {25,"Contaduría","Fijo"},          {26,"Jeep plan","Fijo"},
            {27,"Suscripciones","Fijo"},
        });

        // Seed tarjetas
        mb.InsertData("Tarjetas", new[]{"Id","Nombre","Banco","Red","DiaCierre","DiaVencimiento"}, new object[,]
        {
            {1,"Galicia - VISA",          "Galicia",     "VISA",       19, 4},
            {2,"Santander - VISA",        "Santander",   "VISA",       12, 3},
            {3,"MercadoPago - MasterCard","MercadoPago", "Mastercard", 12, 17},
            {4,"Santander - AMEX",        "Santander",   "AMEX",       12, 3},
        });
    }

    protected override void Down(MigrationBuilder mb)
    {
        mb.DropTable("Gastos");
        mb.DropTable("Ingresos");
        mb.DropTable("TarjetaCuotas");
        mb.DropTable("Tarjetas");
        mb.DropTable("CategoriasGasto");
        mb.DropTable("Cuentas");
        mb.DropTable("Deudas");
        mb.DropTable("EfectivoDesgloses");
    }
}
