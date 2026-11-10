using Microsoft.EntityFrameworkCore;
using Vezza.Domain.Entities;
using Vezza.Infra.Context.Mappings;

namespace Vezza.Infra.Context;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options) : base(options) { }
        
    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Autorizacao> Autorizacoes { get; set; }
    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<Obra> Obras { get; set; }
    public DbSet<Orcamento> Orcamentos { get; set; }
    public DbSet<OrcamentoItem> OrcamentoItens { get; set; }
    public DbSet<Fluxo> Fluxos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {        
        modelBuilder.ApplyConfiguration(new UsuarioMap());
        modelBuilder.ApplyConfiguration(new AutorizacaoMap());
        modelBuilder.ApplyConfiguration(new ClienteMap());
        modelBuilder.ApplyConfiguration(new ObraMap());
        modelBuilder.ApplyConfiguration(new OrcamentoMap());
        modelBuilder.ApplyConfiguration(new OrcamentoItemMap());
        modelBuilder.ApplyConfiguration(new FluxoMap());
    }
}
