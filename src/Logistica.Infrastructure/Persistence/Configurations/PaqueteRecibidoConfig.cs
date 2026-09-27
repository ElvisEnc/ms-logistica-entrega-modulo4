using Logistica.Application.PaquetesRecibidos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistica.Infrastructure.Persistence.Configurations;

// Read model (§9.4 DESIGN.md): no hereda de Entity, no lleva Ignore(DomainEvents).
public sealed class PaqueteRecibidoConfig : IEntityTypeConfiguration<PaqueteRecibido>
{
    public void Configure(EntityTypeBuilder<PaqueteRecibido> builder)
    {
        builder.ToTable("paquetes_recibidos");

        // Clave natural de idempotencia (I12): el valor lo elige BC5, nunca la base.
        builder.HasKey(p => p.PaqueteId);
        builder.Property(p => p.PaqueteId).HasColumnName("paquete_id").ValueGeneratedNever();

        // Guid plano: el read model no usa IDs tipados (§9.4 DESIGN.md).
        builder.Property(p => p.PacienteId).HasColumnName("paciente_id").IsRequired();
        builder.Property(p => p.PacienteNombre).HasColumnName("paciente_nombre").IsRequired();
        builder.Property(p => p.ContratoCateringId).HasColumnName("contrato_catering_id").IsRequired();

        builder.OwnsOne(p => p.DireccionEntrega, direccion =>
        {
            direccion.Property(d => d.Calle).HasColumnName("direccion_calle").IsRequired();
            direccion.Property(d => d.Zona).HasColumnName("direccion_zona").IsRequired();
            direccion.Property(d => d.Ciudad).HasColumnName("direccion_ciudad").IsRequired();
            direccion.Property(d => d.Referencia).HasColumnName("direccion_referencia");

            direccion.OwnsOne(d => d.Coordenadas, coordenadas =>
            {
                coordenadas.Property(c => c.Latitud).HasColumnName("direccion_latitud").HasColumnType("numeric").IsRequired();
                coordenadas.Property(c => c.Longitud).HasColumnName("direccion_longitud").HasColumnType("numeric").IsRequired();
            });
            direccion.Navigation(d => d.Coordenadas).IsRequired();
        });
        builder.Navigation(p => p.DireccionEntrega).IsRequired();

        // Etiqueta (RN-12): OwnsOne anidado a TRES niveles, columna a columna, sin jsonb — se
        // consulta y filtra suelta (§12.4 del maestro, §14.2.4 DESIGN.md).
        builder.OwnsOne(p => p.Etiqueta, etiqueta =>
        {
            etiqueta.Property(e => e.PaqueteId).HasColumnName("etiqueta_paquete_id").IsRequired();
            etiqueta.Property(e => e.NombrePaciente).HasColumnName("etiqueta_nombre_paciente").IsRequired();
            etiqueta.Property(e => e.NroIdentificacion).HasColumnName("etiqueta_nro_identificacion").IsRequired();
            etiqueta.Property(e => e.Fecha).HasColumnName("etiqueta_fecha").HasColumnType("date").IsRequired();
            etiqueta.Property(e => e.CodigoQR).HasColumnName("etiqueta_codigo_qr");

            etiqueta.OwnsOne(e => e.DireccionEntrega, direccion =>
            {
                direccion.Property(d => d.Calle).HasColumnName("etiqueta_direccion_calle").IsRequired();
                direccion.Property(d => d.Zona).HasColumnName("etiqueta_direccion_zona").IsRequired();
                direccion.Property(d => d.Ciudad).HasColumnName("etiqueta_direccion_ciudad").IsRequired();
                direccion.Property(d => d.Referencia).HasColumnName("etiqueta_direccion_referencia");

                direccion.OwnsOne(d => d.Coordenadas, coordenadas =>
                {
                    coordenadas.Property(c => c.Latitud).HasColumnName("etiqueta_direccion_latitud").HasColumnType("numeric").IsRequired();
                    coordenadas.Property(c => c.Longitud).HasColumnName("etiqueta_direccion_longitud").HasColumnType("numeric").IsRequired();
                });
                direccion.Navigation(d => d.Coordenadas).IsRequired();
            });
            etiqueta.Navigation(e => e.DireccionEntrega).IsRequired();
        });
        builder.Navigation(p => p.Etiqueta).IsRequired();

        // Tomada de la RAIZ del evento, no de cada paquete (I10).
        builder.Property(p => p.FechaEntrega).HasColumnName("fecha_entrega").HasColumnType("date").IsRequired();

        builder.Property(p => p.Estado).HasColumnName("estado").HasConversion<string>().IsRequired();

        // D-06 (§14.3, §19.2 DESIGN.md): ObtenerPorAsignarAsync se ejecuta en CADA creacion de
        // ruta y GetPaquetesRecibidosQuery filtra por ambas columnas.
        builder.HasIndex(p => new { p.FechaEntrega, p.Estado })
            .HasDatabaseName("IX_paquetes_recibidos_fecha_entrega_estado");
    }
}
