using Lanches.Context;
using Lanches.Models;
using Microsoft.EntityFrameworkCore;

namespace Lanches.Areas.Admin.Services
{
    public class GraficoVendasService
    {
        private readonly AppDbContext context;

        public GraficoVendasService(AppDbContext context)
        {
            this.context = context;
        }

        public List<LancheGrafico> GetVendasLanches(int dias = 360)
        {
            var data = DateTime.Now.AddDays(-dias).ToUniversalTime();

            var lanches = context.PedidoDetalhes
                .Where(pd => pd.Pedido.PedidoEnviado >= data)
                .Join(context.Lanches,
                    pd => pd.LancheId,
                    l => l.LancheId,
                    (pd, l) => new { pd, l })
                .GroupBy(x => new { x.l.LancheId, x.l.Nome })
                .Select(g => new LancheGrafico
                {
                    LancheNome = g.Key.Nome,
                    LanchesQuantidade = g.Sum(x => x.pd.Quantidade),
                    LanchesValorTotal = g.Sum(x => x.pd.Preco * x.pd.Quantidade)
                })
                .ToList();

            return lanches;
        }
    }
}
