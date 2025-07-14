using Lanches.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Lanches.Context
{
    public class AppDbContext : IdentityDbContext<IdentityUser>
    {
        
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            
        }

        public DbSet<Categoria> Categorias { get; set; }

        public DbSet<Lanche> Lanches { get; set; }

        public DbSet<CarrinhoCompraItem> CarrinhoCompraItens { get; set; }

        public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<PedidoDetalhe> PedidoDetalhes  { get; set; }

      
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // APENAS ESTA LINHA RESOLVE O PROBLEMA DA CHAVE PRIMÁRIA
            modelBuilder.Entity<CarrinhoCompraItem>()
                .Property(e => e.CarrinhoCompraItemId)
                .UseIdentityAlwaysColumn();
            modelBuilder.Entity<Categoria>()
                .Property(c => c.CategoriaId)
                .UseIdentityAlwaysColumn();
            modelBuilder.Entity<Lanche>()
                 .Property(l => l.LancheId)
                 .UseIdentityAlwaysColumn();

            modelBuilder.Entity<Pedido>()
                .Property(p => p.PedidoId)
                .UseIdentityAlwaysColumn();

            modelBuilder.Entity<PedidoDetalhe>()
                .Property(pd => pd.PedidoDetalheId)
                .UseIdentityAlwaysColumn();


        }
    }
}
