using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vezza.Domain.Entities;

namespace Vezza.Infra.Context.Mappings;

public class ClienteMap : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder.ToTable("Cliente");

        builder.HasKey(x => x.ClienteId);

        builder.Property(x => x.RazaoSocial)
            .IsRequired(true)
            .HasColumnName("RazaoSocial")
            .HasColumnType("VARCHAR(500)");

        builder.Property(x => x.CnpjCpf)
            .IsRequired(true)
            .HasColumnName("CnpjCpf")
            .HasColumnType("VARCHAR(30)");

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

        builder.Property(x => x.Telefone)
            .IsRequired(false)
            .HasColumnName("Telefone")
            .HasColumnType("VARCHAR(50)");

        builder.Property(x => x.DataCadastro)
            .IsRequired(false)
            .HasColumnName("DataCadastro")
            .HasColumnType("DATETIME");
    }
}
