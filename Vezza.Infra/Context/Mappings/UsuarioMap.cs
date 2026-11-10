using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vezza.Domain.Entities;

namespace Vezza.Infra.Context.Mappings;

public class UsuarioMap : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("Usuarios");

        builder.HasKey(x => x.UsuarioId);

        builder.Property(x => x.Nome)
            .IsRequired()
            .HasColumnName("Nome")
            .HasColumnType("VARCHAR(500)");

        builder.Property(x => x.Email)
            .IsRequired()
            .HasColumnName("Email")
            .HasColumnType("VARCHAR(500)");

        builder.Property(x => x.Senha)
            .IsRequired()
            .HasColumnName("Senha")
            .HasColumnType("VARCHAR(500)");

        builder.Property(x => x.Nivel)
            .IsRequired()
            .HasColumnName("Nivel");
    }
}
