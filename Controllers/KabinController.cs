using Microsoft.AspNetCore.Mvc;

namespace BLBM_ENV.Controllers
{
    // Bu Controller, kullanıcının gördüğü HTML sayfalarını sunmakla görevlidir.
    public class KabinController : Controller
    {
        // Lokasyon seçme ve arama sayfasını gösterir.
        public IActionResult Index()
        {
            return View();
        }

        // Bir lokasyondaki tüm kabinleri gösteren sayfayı açar.
        // URL'den gelen 'location' parametresini alır ve View'a gönderir.
        public IActionResult RackView(string location)
        {
            if (string.IsNullOrEmpty(location))
            {
                // Eğer bir lokasyon belirtilmemişse, ana seçim sayfasına geri yönlendir.
                return RedirectToAction("Index");
            }
            ViewBag.Location = location;
            return View();
        }
    }
}