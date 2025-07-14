using Lanches.Areas.Admin.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lanches.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminRelatorioVendasController : Controller
    {
        
        
            private readonly RelatorioVendasService relatorioVendasService;

            public AdminRelatorioVendasController(RelatorioVendasService _relatorioVendasService)
            {
                relatorioVendasService = _relatorioVendasService;
            }

            public IActionResult Index()
            {
                return View();
            }

            public async Task<IActionResult> RelatorioVendaSimples(DateTime? minDate,
                DateTime? maxDate)
            {
            var now = DateTime.Now.ToLocalTime();
           
            if (!minDate.HasValue)
                {
                minDate ??= new DateTime(now.Year, 1, 1, 0, 0, 0, DateTimeKind.Local);
            }
                if (!maxDate.HasValue)
                {
                maxDate ??= now;
            }

            ViewData["minDate"] = minDate.Value.ToString("yyyy-MM-ddTHH:mm:ss");
            ViewData["maxDate"] = maxDate.Value.ToString("yyyy-MM-ddTHH:mm:ss");

            var result = await relatorioVendasService.FindByDateAsync(minDate, maxDate);
                return View(result);
            }
        
    }
}
