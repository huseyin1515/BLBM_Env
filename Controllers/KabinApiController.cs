using BLBM_ENV.Data;
using BLBM_ENV.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BLBM_ENV.Controllers
{
    // API metotları için yardımcı bir iç sınıf (internal class)
    internal class KabinUData
    {
        public int StartU { get; set; }
        public int HeightU { get; set; }
    }

    [Route("api/kabin")]
    [ApiController]
    public class KabinApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public KabinApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("locations")]
        public async Task<IActionResult> GetLocations()
        {
            var locations = await _context.Envanterler
                .Where(e => !string.IsNullOrEmpty(e.Location))
                .Select(e => e.Location)
                .Distinct()
                .OrderBy(loc => loc)
                .ToListAsync();

            return Ok(locations);
        }

        [HttpGet("location/{locationName}")]
        public async Task<IActionResult> GetFullLocationDetails(string locationName)
        {
            var devicesInLocation = await _context.Envanterler
                .Where(e => e.Location == locationName && !string.IsNullOrEmpty(e.Kabin) && !string.IsNullOrEmpty(e.KabinU))
                .OrderBy(e => e.Kabin)
                .ToListAsync();

            var kabinler = devicesInLocation
                .GroupBy(device => device.Kabin)
                .Select(kabinGroup => {
                    // *** YENİ VE DAHA SAĞLAM GRUPLAMA MANTIĞI ***
                    // Cihazları "Rear" ve "Diğerleri (Front)" olarak ikiye ayırıyoruz.
                    var groupedByPosition = kabinGroup
                        .GroupBy(d => "Rear".Equals(d.RearFront?.Trim(), StringComparison.OrdinalIgnoreCase));

                    var rearDevices = groupedByPosition
                        .FirstOrDefault(g => g.Key == true)? // Key == true, "Rear" grubudur
                        .Select(ParseDeviceData)
                        .ToList() ?? new List<object>(); // Eğer "Rear" grubu yoksa boş liste oluştur

                    var frontDevices = groupedByPosition
                        .FirstOrDefault(g => g.Key == false)? // Key == false, "Rear" olmayan her şeydir (Front, null, boş string vs.)
                        .Select(ParseDeviceData)
                        .ToList() ?? new List<object>(); // Eğer "Front" grubu yoksa boş liste oluştur

                    return new
                    {
                        KabinName = kabinGroup.Key,
                        FrontDevices = frontDevices,
                        RearDevices = rearDevices
                    };
                })
                .OrderBy(k => k.KabinName);

            return Ok(kabinler);
        }

        private object ParseDeviceData(Envanter device)
        {
            KabinUData uData = ParseKabinU(device.KabinU);

            return new
            {
                Id = device.ID,
                DeviceName = device.DeviceName,
                KabinU = device.KabinU,
                StartU = uData.StartU,
                HeightU = uData.HeightU,
                Tur = device.Tur
            };
        }

        private KabinUData ParseKabinU(string kabinU)
        {
            if (string.IsNullOrWhiteSpace(kabinU)) return new KabinUData { StartU = 1, HeightU = 1 };
            string trimmedKabinU = kabinU.Trim();
            if (trimmedKabinU.Contains('-'))
            {
                string[] parts = trimmedKabinU.Split('-');
                if (parts.Length == 2 && int.TryParse(parts[0].Trim(), out int u1) && int.TryParse(parts[1].Trim(), out int u2))
                {
                    int start = Math.Min(u1, u2);
                    int end = Math.Max(u1, u2);
                    return new KabinUData { StartU = start, HeightU = (end - start) + 1 };
                }
            }
            if (int.TryParse(trimmedKabinU, out int singleU))
            {
                return new KabinUData { StartU = singleU, HeightU = 1 };
            }
            return new KabinUData { StartU = 1, HeightU = 1 };
        }
    }
}