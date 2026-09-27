using Logistica.Domain.Rutas;
using Logistica.Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistica.Infrastructure.Persistence.Configurations;

public sealed class RutaEntregaConfig : IEntityTypeConfiguration<RutaEntrega>
{
    public void Configure(EntityTypeBuilder<RutaEntrega> builder)
    {
        builder.ToTable("rutas");

        builder.HasKey(r => r.Id);
        // ValueGeneratedNever (§14.4, §12.5 del maestro): el Guid lo genera el cliente en Crear.
        builder.Property(r => r.Id).HasColumnName("id").ValueGeneratedNever();

        // RutaId es una propiedad computada sobre Id (§5.3 DESIGN.md): no se persiste como columna.
        builder.Ignore(r => r.RutaId);

        builder.Property(r => r.Fecha)
            .HasColumnName("fecha")
            .HasColumnType("date")
            .IsRequired();

        builder.Property(r => r.RepartidorId)
            .HasColumnName("repartidor_id")
            .HasConversion(id => id.Value, value => RepartidorId.From(value))
            .IsRequired();

        builder.Property(r => r.Estado)
            .HasColumnName("estado")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(r => r.DistanciaTotalKm)
            .HasColumnName("distancia_total_km")
            .HasColumnType("numeric")
            .IsRequired();

        builder.Property(r => r.TiempoEstimadoMin)
            .HasColumnName("tiempo_estimado_min")
            .IsRequired();

        // Shadow property sobre el campo privado _optimizada (§14.2.1, §14.4 DESIGN.md): sostiene
        // I3 al releer el agregado. El nombre coincide con el del campo CLR, asi que EF Core lo
        // enlaza por convencion de backing field, sin HasField adicional.
        builder.Property<bool>("_optimizada")
            .HasColumnName("optimizada")
            .IsRequired();

        // Las TRES instrucciones obligatorias de §14.4 DESIGN.md para _paradas. Sin el Ignore de
        // la propiedad publica, el modelo falla EN TIEMPO DE DISEÑO: "cannot use field _paradas
        // because it is already used by Paradas".
        builder.Ignore(r => r.Paradas);
        builder.HasMany(typeof(ParadaEntrega), "_paradas")
            .WithOne()
            .HasForeignKey("RutaId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation("_paradas").HasField("_paradas");

        builder.Ignore(r => r.DomainEvents);

        // D-01 (§14.3, §19.1 DESIGN.md): respaldo fisico de I11, maximo una ruta activa por
        // repartidor. Cubre la condicion de carrera entre dos peticiones concurrentes que
        // consulten TieneRutaActivaAsync a la vez y ambas obtengan false.
        builder.HasIndex("RepartidorId")
            .IsUnique()
            .HasDatabaseName("UX_rutas_repartidor_activa")
            .HasFilter("estado IN ('PENDIENTE','EN_CAMINO')");

        // D-07 (§14.3, §19.2 DESIGN.md): GetEstadoEntregasQuery (HU-42) y
        // GetHistorialConstanciasQuery (HU-44) filtran por fecha.
        builder.HasIndex(r => r.Fecha)
            .HasDatabaseName("IX_rutas_fecha");
    }
}
