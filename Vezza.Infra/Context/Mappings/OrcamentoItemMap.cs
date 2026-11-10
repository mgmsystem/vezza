using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vezza.Domain.Entities;

namespace Vezza.Infra.Context.Mappings;

public class OrcamentoItemMap : IEntityTypeConfiguration<OrcamentoItem>
{
    public void Configure(EntityTypeBuilder<OrcamentoItem> builder)
    {
        builder.ToTable("OrcamentoItem");

        builder.HasKey(x => x.OrcamentoItemId);

        builder.Property(x => x.OrcamentoId)
            .IsRequired(true)
            .HasColumnName("OrcamentoId")
            .HasColumnType("INT");

        builder.Property(x => x.Descricao)
            .IsRequired(false)
            .HasColumnName("Descricao")
            .HasColumnType("VARCHAR(5000)");

        builder.Property(x => x.Valor)
            .IsRequired(false)
            .HasColumnName("Valor")
            .HasColumnType("DECIMAL(19,4)");
    }
}
