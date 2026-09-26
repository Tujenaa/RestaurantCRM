using Microsoft.AspNetCore.Mvc;
using RestaurantCRM.Web.Models;
using System.Diagnostics;

namespace RestaurantCRM.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return RedirectToAction("Index", "TrangChu");
        }

        public IActionResult Privacy()
        {
            return RedirectToAction("Index", "TrangChu");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
