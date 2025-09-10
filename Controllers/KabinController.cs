using BLBM_ENV.Data;
using BLBM_ENV.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BLBM_ENV.Controllers
{
    public class KabinController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const int RACK_SIZE_U = 42;

        public KabinController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var allLocations = await _context.Envanterler
                .Where(s => !string.IsNullOrEmpty(s.Location))
                .Select(s => s.Location!)
                .Distinct()
                .OrderBy(l => l)
                .ToListAsync();

            return View(allLocations);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FindDevice(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                TempData["ErrorMessage"] = "Lütfen bir arama terimi girin.";
                return RedirectToAction(nameof(Index));
            }

            searchTerm = searchTerm.Trim().ToLower();

            var device = await _context.Envanterler
                .AsNoTracking()
                .FirstOrDefaultAsync(s =>
                    (s.DeviceName.ToLower() == searchTerm) ||
                    (s.ServiceTagSerialNumber != null && s.ServiceTagSerialNumber.ToLower() == searchTerm)
                );

            if (device == null)
            {
                TempData["ErrorMessage"] = $"'{searchTerm}' ile eşleşen bir cihaz bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            if (string.IsNullOrEmpty(device.Location) || string.IsNullOrEmpty(device.Kabin) || string.IsNullOrEmpty(device.KabinU))
            {
                TempData["WarningMessage"] = $"'{device.DeviceName}' cihazı bulundu, ancak kabin konum bilgileri eksik olduğu için görselleştirilemiyor. Cihazın kendi detay sayfası açıldı.";
                return RedirectToAction("Details", "Envanter", new { id = device.ID });
            }

            return RedirectToAction(nameof(RackView), new { location = device.Location, highlight = device.ID });
        }

        public async Task<IActionResult> RackView(string location)
        {
            if (string.IsNullOrEmpty(location)) { return RedirectToAction(nameof(Index)); }

            var allServers = await _context.Envanterler.AsNoTracking().ToListAsync();
            var allLocations = allServers.Where(s => !string.IsNullOrEmpty(s.Location)).Select(s => s.Location!).Distinct().OrderBy(l => l).ToList();

            var viewModel = new RackVisualizationViewModel
            {
                AllLocations = allLocations,
                SelectedLocation = location
            };

            var serversInLocation = allServers.Where(s => s.Location == location && !string.IsNullOrEmpty(s.Kabin) && !string.IsNullOrEmpty(s.KabinU)).ToList();
            var cabinetsInLocation = serversInLocation.Select(s => s.Kabin!).Distinct().OrderBy(c => c);

            // --- YENİ EKLENDİ: Aynı isme sahip cihazları bulmak için bir gruplama ---
            var deviceNameCounts = serversInLocation
                .GroupBy(s => s.DeviceName)
                .ToDictionary(g => g.Key, g => g.Count());

            foreach (var cabinetName in cabinetsInLocation)
            {
                var frontU_Map = new Dictionary<int, List<Envanter>>();
                var rearU_Map = new Dictionary<int, List<Envanter>>();
                var serversInCabinet = serversInLocation.Where(s => s.Kabin == cabinetName);

                foreach (var server in serversInCabinet)
                {
                    // --- YENİ EKLENDİ: Eğer aynı isimden birden fazla varsa, DeviceName'i güncelle ---
                    if (deviceNameCounts.GetValueOrDefault(server.DeviceName, 0) > 1)
                    {
                        server.DeviceName = $"{server.DeviceName} ({server.ServiceTagSerialNumber})";
                    }

                    var (startU, endU) = ParseKabinU(server.KabinU);
                    if (startU == 0 || endU == 0) continue;

                    bool isFront = (server.RearFront ?? "").Trim().ToUpper().StartsWith("F");
                    bool isRear = (server.RearFront ?? "").Trim().ToUpper().StartsWith("R");

                    var targetMap = isRear ? rearU_Map : frontU_Map;

                    for (int u = startU; u <= endU; u++)
                    {
                        if (!targetMap.ContainsKey(u)) { targetMap[u] = new List<Envanter>(); }
                        targetMap[u].Add(server);
                    }
                }

                var frontUnits = new List<RackUnitViewModel>();
                var rearUnits = new List<RackUnitViewModel>();
                for (int i = 1; i <= RACK_SIZE_U; i++)
                {
                    frontUnits.Add(new RackUnitViewModel { U_Number = i, OccupyingServers = frontU_Map.GetValueOrDefault(i, new List<Envanter>()) });
                    rearUnits.Add(new RackUnitViewModel { U_Number = i, OccupyingServers = rearU_Map.GetValueOrDefault(i, new List<Envanter>()) });
                }

                viewModel.Racks[cabinetName + " (Ön)"] = frontUnits;
                viewModel.Racks[cabinetName + " (Arka)"] = rearUnits;
            }
            return View(viewModel);
        }

        private (int, int) ParseKabinU(string kabinU)
        {
            if (string.IsNullOrWhiteSpace(kabinU)) return (0, 0);
            string trimmedKabinU = kabinU.Trim();
            if (trimmedKabinU.Contains('-'))
            {
                string[] parts = trimmedKabinU.Split('-');
                if (parts.Length == 2 && int.TryParse(parts[0].Trim(), out int u1) && int.TryParse(parts[1].Trim(), out int u2))
                {
                    return (Math.Min(u1, u2), Math.Max(u1, u2));
                }
            }
            if (int.TryParse(trimmedKabinU, out int singleU))
            {
                return (singleU, singleU);
            }
            return (0, 0);
        }
    }
}