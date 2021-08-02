using IAutoM8.InfusionSoft.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace IAutoM8.Controllers
{
    public class HomeController : Controller
    {
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Index()
        {
            return View();
        }

    }
}
