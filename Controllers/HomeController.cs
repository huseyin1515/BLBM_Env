using BLBM_ENV.Data;
using BLBM_ENV.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;

namespace BLBM_ENV.Controllers
{
    // Giriþ yapýlmasý zorunlu olsun. Giriþ yapmayan dashboard'u göremez.
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // 1. Temel Sayýlar
            var totalDevices = await _context.Envanterler.CountAsync();
            var totalConnections = await _context.Baglantilar.CountAsync();
            var totalVips = await _context.Vips.CountAsync();
            var totalServers = await _context.Envanterler
                                     .Where(e => e.Tur == "Server")
                                     .CountAsync();

            // 2. Cihaz Türü Daðýlýmý (Pie Chart için)
            // Veritabanýnda gruplama yapýp hafýzaya çekiyoruz
            var deviceTypes = await _context.Envanterler
                .GroupBy(e => e.Tur)
                .Select(g => new { Tur = g.Key, Sayi = g.Count() })
                .ToListAsync();

            var deviceTypeDict = new Dictionary<string, int>();
            foreach (var item in deviceTypes)
            {
                // Boþ veya null türleri "Belirsiz" olarak etiketle
                string key = string.IsNullOrEmpty(item.Tur) ? "Belirsiz" : item.Tur;
                if (deviceTypeDict.ContainsKey(key)) deviceTypeDict[key] += item.Sayi;
                else deviceTypeDict.Add(key, item.Sayi);
            }

            // 3. Ýþletim Sistemi Daðýlýmý (Bar Chart için - En çok kullanýlan ilk 5)
            var osTypes = await _context.Envanterler
                .Where(e => !string.IsNullOrEmpty(e.OperatingSystem)) // Boþ olanlarý atla
                .GroupBy(e => e.OperatingSystem)
                .Select(g => new { OS = g.Key, Sayi = g.Count() })
                .OrderByDescending(x => x.Sayi)
                .Take(5)
                .ToListAsync();

            var osDict = osTypes.ToDictionary(k => k.OS, v => v.Sayi);

            // 4. Son Eklenen 5 Cihaz
            var lastDevices = await _context.Envanterler
                .OrderByDescending(e => e.ID)
                .Take(5)
                .ToListAsync();

            // Modeli Doldur
            var model = new DashboardViewModel
            {
                TotalDevices = totalDevices,
                TotalConnections = totalConnections,
                TotalVips = totalVips,
                TotalServers = totalServers,
                DeviceByTypes = deviceTypeDict,
                DeviceByOS = osDict,
                LastAddedDevices = lastDevices
            };

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
   
        public IActionResult Error()
        {
            // Hatanýn detayýný yakala
            var exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            if (exceptionHandlerPathFeature?.Error != null)
            {
                // HATA LOGLAMA: Hatanýn ne olduðu, nerede olduðu dosyaya yazýlýr.
                // Kullanýcý bunu görmez, sadece sistem yöneticisi görür.
                _logger.LogError(exceptionHandlerPathFeature.Error, "Sistem Hatasý Oluþtu! Yol: {Path}", exceptionHandlerPathFeature.Path);
            }

            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}   