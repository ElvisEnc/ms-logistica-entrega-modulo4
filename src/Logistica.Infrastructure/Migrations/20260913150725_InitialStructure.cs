using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Logistica.Infrastructure.Migrations;

/// <inheritdoc />
public partial class InitialStructure : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "paquetes_recibidos",
            columns: table => new
            {
                paquete_id = table.Column<Guid>(type: "uuid", nullable: false),
                paciente_id = table.Column<Guid>(type: "uuid", nullable: false),
                paciente_nombre = table.Column<string>(type: "text", nullable: false),
                direccion_calle = table.Column<string>(type: "text", nullable: false),
                direccion_zona = table.Column<string>(type: "text", nullable: false),
                direccion_ciudad = table.Column<string>(type: "text", nullable: false),
                direccion_referencia = table.Column<string>(type: "text", nullable: true),
                direccion_latitud = table.Column<decimal>(type: "numeric", nullable: false),
                direccion_longitud = table.Column<decimal>(type: "numeric", nullable: false),
                contrato_catering_id = table.Column<Guid>(type: "uuid", nullable: false),
                etiqueta_paquete_id = table.Column<Guid>(type: "uuid", nullable: false),
                etiqueta_nombre_paciente = table.Column<string>(type: "text", nullable: false),
                etiqueta_nro_identificacion = table.Column<string>(type: "text", nullable: false),
                etiqueta_direccion_calle = table.Column<string>(type: "text", nullable: false),
                etiqueta_direccion_zona = table.Column<string>(type: "text", nullable: false),
                etiqueta_direccion_ciudad = table.Column<string>(type: "text", nullable: false),
                etiqueta_direccion_referencia = table.Column<string>(type: "text", nullable: true),
                etiqueta_direccion_latitud = table.Column<decimal>(type: "numeric", nullable: false),
                etiqueta_direccion_longitud = table.Column<decimal>(type: "numeric", nullable: false),
                etiqueta_fecha = table.Column<DateOnly>(type: "date", nullable: false),
                etiqueta_codigo_qr = table.Column<string>(type: "text", nullable: true),
                fecha_entrega = table.Column<DateOnly>(type: "date", nullable: false),
                estado = table.Column<string>(type: "text", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_paquetes_recibidos", x => x.paquete_id);
            });

        migrationBuilder.CreateTable(
            name: "repartidores",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                nombre = table.Column<string>(type: "text", nullable: false),
                telefono = table.Column<string>(type: "text", nullable: false),
                vehiculo_tipo = table.Column<string>(type: "text", nullable: false),
                vehiculo_placa = table.Column<string>(type: "text", nullable: false),
                vehiculo_capacidad_paquetes = table.Column<int>(type: "integer", nullable: false),
                disponible = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_repartidores", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "rutas",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                fecha = table.Column<DateOnly>(type: "date", nullable: false),
                repartidor_id = table.Column<Guid>(type: "uuid", nullable: false),
                estado = table.Column<string>(type: "text", nullable: false),
                distancia_total_km = table.Column<decimal>(type: "numeric", nullable: false),
                tiempo_estimado_min = table.Column<int>(type: "integer", nullable: false),
                optimizada = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_rutas", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "paradas_entrega",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                paquete_id = table.Column<Guid>(type: "uuid", nullable: false),
                paciente_id = table.Column<Guid>(type: "uuid", nullable: false),
                paciente_nombre = table.Column<string>(type: "text", nullable: false),
                direccion_calle = table.Column<string>(type: "text", nullable: false),
                direccion_zona = table.Column<string>(type: "text", nullable: false),
                direccion_ciudad = table.Column<string>(type: "text", nullable: false),
                direccion_referencia = table.Column<string>(type: "text", nullable: true),
                direccion_latitud = table.Column<decimal>(type: "numeric", nullable: false),
                direccion_longitud = table.Column<decimal>(type: "numeric", nullable: false),
                contrato_catering_id = table.Column<Guid>(type: "uuid", nullable: false),
                orden = table.Column<int>(type: "integer", nullable: false),
                estado = table.Column<string>(type: "text", nullable: false),
                constancia_fecha_hora = table.Column<DateTime>(type: "timestamptz", nullable: true),
                constancia_tipo = table.Column<string>(type: "text", nullable: true),
                constancia_url_evidencia = table.Column<string>(type: "text", nullable: true),
                constancia_receptor_nombre = table.Column<string>(type: "text", nullable: true),
                constancia_confirmacion_latitud = table.Column<decimal>(type: "numeric", nullable: true),
                constancia_confirmacion_longitud = table.Column<decimal>(type: "numeric", nullable: true),
                incidencia_fecha_hora = table.Column<DateTime>(type: "timestamptz", nullable: true),
                incidencia_motivo = table.Column<string>(type: "text", nullable: true),
                incidencia_descripcion = table.Column<string>(type: "text", nullable: true),
                incidencia_url_foto = table.Column<string>(type: "text", nullable: true),
                ruta_id = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_paradas_entrega", x => x.id);
                table.CheckConstraint("CK_paradas_entrega_constancia_incidencia_excluyentes", "NOT (constancia_tipo IS NOT NULL AND incidencia_motivo IS NOT NULL)");
                table.ForeignKey(
                    name: "FK_paradas_entrega_rutas_ruta_id",
                    column: x => x.ruta_id,
                    principalTable: "rutas",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_paquetes_recibidos_fecha_entrega_estado",
            table: "paquetes_recibidos",
            columns: new[] { "fecha_entrega", "estado" });

        migrationBuilder.CreateIndex(
            name: "IX_paradas_entrega_ruta_id",
            table: "paradas_entrega",
            column: "ruta_id");

        migrationBuilder.CreateIndex(
            name: "IX_rutas_fecha",
            table: "rutas",
            column: "fecha");

        migrationBuilder.CreateIndex(
            name: "UX_rutas_repartidor_activa",
            table: "rutas",
            column: "repartidor_id",
            unique: true,
            filter: "estado IN ('PENDIENTE','EN_CAMINO')");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "paquetes_recibidos");

        migrationBuilder.DropTable(
            name: "paradas_entrega");

        migrationBuilder.DropTable(
            name: "repartidores");

        migrationBuilder.DropTable(
            name: "rutas");
    }
}
