using Logistica.Domain.Rutas;
using Logistica.Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistica.Infrastructure.Persistence.Configurations;

public sealed class ParadaEntregaConfig : IEntityTypeConfiguration<ParadaEntrega>
{
    public void Configure(EntityTypeBuilder<ParadaEntrega> builder)
    {
        // CHECK de I5 (D-08, §14.3 DESIGN.md): la exclusion mutua entre constancia e incidencia
        // no esta respaldada solo por el codigo del agregado, sino tambien por el esquema.
        builder.ToTable("paradas_entrega", t => t.HasCheckConstraint(
            "CK_paradas_entrega_constancia_incidencia_excluyentes",
            "NOT (constancia_tipo IS NOT NULL AND incidencia_motivo IS NOT NULL)"));

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id").ValueGeneratedNever();

        builder.Ignore(p => p.ParadaId);
        builder.Ignore(p => p.DomainEvents);

        // FK sombra hacia rutas.id: la relacion (incluido OnDelete.Cascade) ya la declara
        // RutaEntregaConfig.HasMany; aqui solo se nombra la columna (§5.3, §14.2.2 DESIGN.md).
        builder.Property<Guid>("RutaId").HasColumnName("ruta_id");

        builder.Property(p => p.PaqueteId)
            .HasColumnName("paquete_id")
            .HasConversion(id => id.Value, value => PaqueteId.From(value))
            .IsRequired();

        // I14: heredado del paquete, nunca resuelto.
        builder.Property(p => p.PacienteId)
            .HasColumnName("paciente_id")
            .HasConversion(id => id.Value, value => PacienteId.From(value))
            .IsRequired();

        builder.Property(p => p.PacienteNombre)
            .HasColumnName("paciente_nombre")
            .IsRequired();

        // I14, RN-23: es el dato que vuelve a BC4 en la incidencia.
        builder.Property(p => p.ContratoCateringId)
            .HasColumnName("contrato_catering_id")
            .HasConversion(id => id.Value, value => ContratoId.From(value))
            .IsRequired();

        builder.Property(p => p.Orden)
            .HasColumnName("orden")
            .IsRequired();

        builder.Property(p => p.Estado)
            .HasColumnName("estado")
            .HasConversion<string>()
            .IsRequired();

        builder.OwnsOne(p => p.DireccionEntrega, direccion =>
        {
            direccion.Property(d => d.Calle).HasColumnName("direccion_calle").IsRequired();
            direccion.Property(d => d.Zona).HasColumnName("direccion_zona").IsRequired();
            direccion.Property(d => d.Ciudad).HasColumnName("direccion_ciudad").IsRequired();
            direccion.Property(d => d.Referencia).HasColumnName("direccion_referencia");

            // RN-15: sin esto no hay optimizacion.
            direccion.OwnsOne(d => d.Coordenadas, coordenadas =>
            {
                coordenadas.Property(c => c.Latitud).HasColumnName("direccion_latitud").HasColumnType("numeric").IsRequired();
                coordenadas.Property(c => c.Longitud).HasColumnName("direccion_longitud").HasColumnType("numeric").IsRequired();
            });
            direccion.Navigation(d => d.Coordenadas).IsRequired();
        });
        builder.Navigation(p => p.DireccionEntrega).IsRequired();

        // ConstanciaEntrega: OwnsOne OPCIONAL (0..1), SIN configuracion especial — nada de
        // IsRequired(false), nada de tabla aparte. EF Core trata una fila con las columnas del
        // owned type en null como "sin instancia" (§12.3 del maestro, §14.4 DESIGN.md).
        builder.OwnsOne(p => p.Constancia, constancia =>
        {
            constancia.Property(c => c.Tipo).HasColumnName("constancia_tipo").HasConversion<string>();
            constancia.Property(c => c.UrlEvidencia).HasColumnName("constancia_url_evidencia");
            constancia.Property(c => c.ReceptorNombre).HasColumnName("constancia_receptor_nombre");
            constancia.Property(c => c.FechaHora).HasColumnName("constancia_fecha_hora").HasColumnType("timestamptz");

            // Opcional DENTRO de otro opcional.
            constancia.OwnsOne(c => c.CoordenadasConfirmacion, coordenadas =>
            {
                coordenadas.Property(c => c.Latitud).HasColumnName("constancia_confirmacion_latitud").HasColumnType("numeric");
                coordenadas.Property(c => c.Longitud).HasColumnName("constancia_confirmacion_longitud").HasColumnType("numeric");
            });
        });

        // IncidenciaEntrega: mismo criterio, sin configuracion especial.
        builder.OwnsOne(p => p.Incidencia, incidencia =>
        {
            incidencia.Property(i => i.Motivo).HasColumnName("incidencia_motivo").HasConversion<string>();
            incidencia.Property(i => i.Descripcion).HasColumnName("incidencia_descripcion");
            incidencia.Property(i => i.UrlFoto).HasColumnName("incidencia_url_foto");
            incidencia.Property(i => i.FechaHora).HasColumnName("incidencia_fecha_hora").HasColumnType("timestamptz");
        });
    }
}
