using Logistica.Domain.Repartidores;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistica.Infrastructure.Persistence.Configurations;

public sealed class RepartidorConfig : IEntityTypeConfiguration<Repartidor>
{
    public void Configure(EntityTypeBuilder<Repartidor> builder)
    {
        builder.ToTable("repartidores");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasColumnName("id").ValueGeneratedNever();

        builder.Ignore(r => r.RepartidorId);
        builder.Ignore(r => r.DomainEvents);

        builder.Property(r => r.Nombre).HasColumnName("nombre").IsRequired();
        builder.Property(r => r.Telefono).HasColumnName("telefono").IsRequired();

        builder.OwnsOne(r => r.Vehiculo, vehiculo =>
        {
            vehiculo.Property(v => v.Tipo).HasColumnName("vehiculo_tipo").IsRequired();
            // Sin indice unico: dos vehiculos con la misma placa no violan ninguna invariante
            // declarada (§14.2.3 DESIGN.md).
            vehiculo.Property(v => v.Placa).HasColumnName("vehiculo_placa").IsRequired();
            vehiculo.Property(v => v.CapacidadPaquetes).HasColumnName("vehiculo_capacidad_paquetes").IsRequired(); // I2
        });
        builder.Navigation(r => r.Vehiculo).IsRequired();

        // Nace en true (I11); se consume con AsignarRuta() y se recupera con Liberar().
        builder.Property(r => r.Disponible).HasColumnName("disponible").IsRequired();
    }
}
