// Konum: Controllers/HomeController.cs

using BLBM_ENV.Models; // HATA BUNUN EKS�KL���NDEN KAYNAKLANIYORDU
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BLBM_ENV.Controllers // Projenizin ad� ile uyumlu ad alan�
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        // Bu action'�, EnvanterController'� ana sayfa yapmak i�in de�i�tirebiliriz
        // ama �imdilik standart b�rakal�m.
        public IActionResult Index()
        {
            // Ana sayfay� Envanter listesine y�nlendirelim.
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