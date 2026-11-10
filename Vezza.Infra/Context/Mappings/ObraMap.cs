using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vezza.Domain.Entities;

namespace Vezza.Infra.Context.Mappings;

public class ObraMap : IEntityTypeConfiguration<Obra>
{
    public void Configure(EntityTypeBuilder<Obra> builder)
    {
        builder.ToTable("Obra");

        builder.HasKey(x => x.ObraId);

        builder.Property(x => x.ClienteId)
            .IsRequired(true)
            .HasColumnName("ClienteId")
            .HasColumnType("INT");

        builder.Property(x => x.Descricao)
            .IsRequired(true)
            .HasColumnName("Descricao")
            .HasColumnType("VARCHAR(5000)");

        builder.Property(x => x.Cep)
            .IsRequired(false)
            .HasColumnName("Cep")
            .HasColumnType("VARCHAR(20)");

        builder.Property(x => x.Logradouro)
            .IsRequired(false)
            .HasColumnName("Logradouro")
            .HasColumnType("VARCHAR(500)");

        builder.Property(x => x.Numero)
            .IsRequired(false)
            .HasColumnName("Numero")
            .HasColumnType("VARCHAR(50)");

        builder.Property(x => x.Complemento)
            .IsRequired(false)
            .HasColumnName("Complemento")
            .HasColumnType("VARCHAR(500)");

        builder.Property(x => x.Bairro)
            .IsRequired(false)
            .HasColumnName("Bairro")
            .HasColumnType("VARCHAR(500)");

        builder.Property(x => x.Cidade)
            .IsRequired(false)
            .HasColumnName("Cidade")
            .HasColumnType("VARCHAR(500)");

        builder.Property(x => x.Estado)
            .IsRequired(false)
            .HasColumnName("Estado")
            .HasColumnType("VARCHAR(2)");

        builder.Property(x => x.DataCadastro)
            .IsRequired(true)
            .HasColumnName("DataCadastro")
            .HasColumnType("DATETIME");        
    }
}
