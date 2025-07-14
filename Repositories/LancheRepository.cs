using Lanches.Context;
using Lanches.Models;
using Lanches.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Lanches.Repositories
{
    public class LancheRepository : ILancheRepository
    {
        private readonly AppDbContext _context;

        public LancheRepository(AppDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Lanche> Lanches => _context.Lanches.Include(c => c.Categoria);

        public IEnumerable<Lanche> LanchesPreferidos => _context.Lanches.Where(p=> 
                p.IsLanchePreferido).Include(c => c.Categoria);

        public Lanche GetLancheById(int lancheId) => _context.Lanches.FirstOrDefault(i=> 
                i.LancheId == lancheId);    
       
    }
}
