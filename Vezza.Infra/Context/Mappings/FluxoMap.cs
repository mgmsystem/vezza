using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vezza.Domain.Entities;

namespace Vezza.Infra.Context.Mappings;

public class FluxoMap : IEntityTypeConfiguration<Fluxo>
{
    public void Configure(EntityTypeBuilder<Fluxo> builder)
    {
        builder.ToTable("Fluxo");

        builder.HasKey(x => x.FluxoId);

        builder.Property(x => x.OrcamentoId)
            .IsRequired(false)
            .HasColumnName("OrcamentoId")
            .HasColumnType("INT");

        builder.Property(x => x.Data)
            .IsRequired(false)
            .HasColumnName("Data")
            .HasColumnType("DATETIME");

        builder.Property(x => x.Descricao)
            .IsRequired(false)
            .HasColumnName("Descricao")
            .HasColumnType("VARCHAR(5000)");

        builder.Property(x => x.DebitoCredito)
            .IsRequired(false)
            .HasColumnName("Debito_Credito")
            .HasColumnType("VARCHAR(5)");

        builder.Property(x => x.Valor)
            .IsRequired(false)
            .HasColumnName("Valor")
            .HasColumnType("DECIMAL(19,4)");

        builder.Property(x => x.Categoria)
            .IsRequired(false)
            .HasColumnName("Categoria")
            .HasColumnType("VARCHAR(500)");
    }
}
