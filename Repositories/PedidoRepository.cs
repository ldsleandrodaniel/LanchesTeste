using Lanches.Context;
using Lanches.Models;
using Lanches.Repositories.Interfaces;

namespace Lanches.Repositories
{
    public class PedidoRepository : IPedidoRepository
    {
        private readonly AppDbContext _appDbContext;
        private readonly CarrinhoCompra _carrinhoCompra;

        public PedidoRepository(AppDbContext appDbContext, CarrinhoCompra carrinhoCompra)
        {
            _appDbContext = appDbContext;
            _carrinhoCompra = carrinhoCompra;
        }

        public void CriarPedido(Pedido pedido)
        {
            // Usar transaction para garantir que tudo seja salvo ou nada
            using var transaction = _appDbContext.Database.BeginTransaction();
            
            try
            {
                pedido.PedidoEnviado = DateTime.Now; // Evite UtcNow para PostgreSQL
                _appDbContext.Pedidos.Add(pedido);
                _appDbContext.SaveChanges(); // Salva o pedido primeiro para gerar o ID

                // Adiciona os itens do carrinho
                foreach (var carrinhoItem in _carrinhoCompra.CarrinhoCompraItems)
                {
                    var pedidoDetail = new PedidoDetalhe()
                    {
                        Quantidade = carrinhoItem.Quantidade,
                        LancheId = carrinhoItem.Lanche.LancheId,
                        PedidoId = pedido.PedidoId, // Agora o PedidoId já existe
                        Preco = carrinhoItem.Lanche.Preco
                    };
                    _appDbContext.PedidoDetalhes.Add(pedidoDetail);
                }

                _appDbContext.SaveChanges(); // Salva os itens
                transaction.Commit(); // Confirma a transação
            }
            catch
            {
                transaction.Rollback(); // Em caso de erro, desfaz tudo
                throw; // Re-lança a exceção para ser tratada no Controller
            }
        }
    }
}
