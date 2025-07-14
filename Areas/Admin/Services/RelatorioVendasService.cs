using Lanches.Context;
using Lanches.Models;
using Microsoft.EntityFrameworkCore;

namespace Lanches.Areas.Admin.Services
{
    public class RelatorioVendasService
    {
        private readonly AppDbContext context;
        public RelatorioVendasService(AppDbContext _context)
        {
            context = _context;
        }

        public async Task<List<Pedido>> FindByDateAsync(DateTime? minDate, DateTime? maxDate)
        {
            var query = context.Pedidos.AsQueryable();

            if (minDate.HasValue)
            {
                query = query.Where(p => p.PedidoEnviado >= minDate.Value.ToUniversalTime());
            }

            if (maxDate.HasValue)
            {
                query = query.Where(p => p.PedidoEnviado <= maxDate.Value.ToUniversalTime());
            }

            return await query
                .Include(p => p.PedidoItens)
                .ThenInclude(pi => pi.Lanche)
                .OrderByDescending(p => p.PedidoEnviado)
                .ToListAsync();
        }

    }
}
