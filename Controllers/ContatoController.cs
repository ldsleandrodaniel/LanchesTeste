using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lanches.Controllers
{
    [AllowAnonymous]
    public class ContatoController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
