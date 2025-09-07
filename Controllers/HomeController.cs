// Konum: Controllers/HomeController.cs

using BLBM_ENV.Models; // HATA BUNUN EKSÝKLÝÐÝNDEN KAYNAKLANIYORDU
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BLBM_ENV.Controllers // Projenizin adý ile uyumlu ad alaný
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        // Bu action'ý, EnvanterController'ý ana sayfa yapmak için deðiþtirebiliriz
        // ama þimdilik standart býrakalým.
        public IActionResult Index()
        {
            // Ana sayfayý Envanter listesine yönlendirelim.
            return RedirectToAction("Index", "Envanter");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}