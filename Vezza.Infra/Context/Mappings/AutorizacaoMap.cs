using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vezza.Domain.Entities;

namespace Vezza.Infra.Context.Mappings;

public class AutorizacaoMap : IEntityTypeConfiguration<Autorizacao>
{
    public void Configure(EntityTypeBuilder<Autorizacao> builder)
    {
        builder.ToTable("Autorizacao");

        builder.HasKey(a => a.AutorizacaoId);

        builder.Property(a => a.Email)
            .IsRequired()
            .HasColumnName("Email")
            .HasColumnType("VARCHAR(500)");

        builder.Property(a => a.Tipo)
            .IsRequired()
            .HasColumnName("Tipo")
            .HasColumnType("VARCHAR(500)");

        builder.Property(a => a.Codigo)
            .IsRequired()
            .HasColumnName("Codigo")
            .HasColumnType("VARCHAR(64)");

        builder.Property(a => a.Validade)
            .IsRequired()
            .HasColumnName("Validade")
            .HasColumnType("DATETIME");

        builder.Property(a => a.Utilizado)
            .IsRequired()
            .HasColumnName("Utilizado")
            .HasColumnType("BIT");

        builder.Property(a => a.Tentativas)
            .IsRequired()
            .HasDefaultValue(0)
            .HasColumnName("Tentativas")
            .HasColumnType("INT");
    }
}
