using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vezza.Domain.Entities;

namespace Vezza.Infra.Context.Mappings;

public class OrcamentoMap : IEntityTypeConfiguration<Orcamento>
{
    public void Configure(EntityTypeBuilder<Orcamento> builder)
    {
        builder.ToTable("Orcamento");

        builder.HasKey(x => x.OrcamentoId);

        builder.Property(x => x.ObraId)
            .IsRequired(true)
            .HasColumnName("ObraId")
            .HasColumnType("INT");

        builder.Property(x => x.Data)
            .IsRequired(false)
            .HasColumnName("Data")
            .HasColumnType("DATETIME");

        builder.Property(x => x.ObjetoProposta)
            .IsRequired(false)
            .HasColumnName("ObjetoProposta")
            .HasColumnType("VARCHAR(5000)");

        builder.Property(x => x.Valor)
            .IsRequired(false)
            .HasColumnName("Valor")
            .HasColumnType("DECIMAL(19,4)");

        builder.Property(x => x.CondPagamento)
            .IsRequired(false)
            .HasColumnName("CondPagamento")
            .HasColumnType("VARCHAR(5000)");

        builder.Property(x => x.Prazos)
            .IsRequired(false)
            .HasColumnName("Prazos")
            .HasColumnType("VARCHAR(5000)");

        builder.Property(x => x.Comentarios)
            .IsRequired(false)
            .HasColumnName("Comentarios")
            .HasColumnType("VARCHAR(5000)");

        builder.Property(x => x.NotasGerais)
            .IsRequired(false)
            .HasColumnName("NotasGerais")
            .HasColumnType("VARCHAR(5000)");

        builder.Property(x => x.InicioObra)
            .IsRequired(false)
            .HasColumnName("InicioObra")
            .HasColumnType("DATETIME");

        builder.Property(x => x.TerminoObra)
            .IsRequired(false)
            .HasColumnName("TerminoObra")
            .HasColumnType("DATETIME");

        builder.Property(x => x.Situacao)
            .IsRequired(false)
            .HasColumnName("Situacao")
            .HasColumnType("VARCHAR(50)");
    }
}
