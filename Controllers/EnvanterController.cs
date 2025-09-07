using BLBM_ENV.Data;
using BLBM_ENV.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace BLBM_ENV.Controllers
{
    public class EnvanterController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EnvanterController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var envanterler = await _context.Envanterler.Include(e => e.AsSourceConnections).Include(e => e.AsTargetConnections).OrderBy(e => e.DeviceName).ToListAsync();
            return View(envanterler);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var envanter = await _context.Envanterler.FirstOrDefaultAsync(m => m.ID == id);
            if (envanter == null) return NotFound();

            ViewBag.AllDevicesForSelectList = await _context.Envanterler.Where(e => e.ID != id).OrderBy(e => e.DeviceName).ToListAsync();
            ViewBag.AllConnections = await _context.Baglantilar.Select(b => new { SourceDeviceName = b.SourceDevice != null ? b.SourceDevice.DeviceName : "", SourcePort = b.Source_Port, TargetDeviceName = b.TargetDevice != null ? b.TargetDevice.DeviceName : "", TargetPort = b.Target_Port }).ToListAsync();

            // --- YENİ VE SADELEŞTİRİLMİŞ MANTIK ---
            var unifiedPortList = new List<UnifiedPortViewModel>();
            var processedPortNames = new HashSet<string>();

            var allConnectionsForDevice = await _context.Baglantilar
                .Include(b => b.SourceDevice)
                .Include(b => b.TargetDevice)
                .Where(b => b.SourceDeviceID == id || b.TargetDeviceID == id)
                .ToListAsync();

            var allDetailsForDevice = await _context.EnvanterDetails
                .Where(d => d.EnvanterID == id)
                .ToListAsync();

            // 1. Adım: Bağlantısı olan tüm portları işle
            foreach (var connection in allConnectionsForDevice)
            {
                var isSource = connection.SourceDeviceID == id;
                var portName = isSource ? connection.Source_Port : connection.Target_Port;
                if (string.IsNullOrEmpty(portName))
                {
                    portName = "[Port Adı Eksik]";
                }

                var detail = allDetailsForDevice.FirstOrDefault(d => d.PortID == portName);

                unifiedPortList.Add(new UnifiedPortViewModel
                {
                    Port = portName,
                    DetailID = detail?.ID ?? 0,
                    ConnectionID = connection.ID,
                    Type = connection.ConnectionType,
                    LinkStatus = detail?.LinkStatus,
                    LinkSpeed = detail?.LinkSpeed,
                    MacAddress = detail?.BakirMAC ?? detail?.FiberMAC,
                    WWPN = detail?.WWPN,
                    IsConnected = true,
                    RemoteDevice = isSource ? connection.TargetDevice : connection.SourceDevice,
                    RemotePort = isSource ? connection.Target_Port : connection.Source_Port,
                    IsVirtual = (connection.ConnectionType ?? "").StartsWith("Virtual")
                });
                processedPortNames.Add(portName);
            }

            // 2. Adım: Bağlantısı olmayan, sadece detayı olan portları ekle
            foreach (var detail in allDetailsForDevice)
            {
                if (!string.IsNullOrEmpty(detail.PortID) && !processedPortNames.Contains(detail.PortID))
                {
                    unifiedPortList.Add(new UnifiedPortViewModel
                    {
                        Port = detail.PortID,
                        DetailID = detail.ID,
                        Type = detail.Turu,
                        LinkStatus = detail.LinkStatus,
                        LinkSpeed = detail.LinkSpeed,
                        MacAddress = detail.BakirMAC ?? detail.FiberMAC,
                        WWPN = detail.WWPN,
                        IsConnected = false,
                        IsVirtual = (detail.Turu ?? "").StartsWith("Virtual")
                    });
                }
            }

            ViewBag.UnifiedPortList = unifiedPortList.OrderBy(p => p.Port, StringComparer.OrdinalIgnoreCase).ToList();
            // --- MANTIK SONU ---

            return View(envanter);
        }

        #region Diğer Metotlar
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,DeviceName,Tur,ServiceTagSerialNumber,Model,IpAddress,VcenterAddress,ClusterName,Location,OperatingSystem,IloIdracIp,Kabin,RearFront,KabinU")] Envanter envanter)
        {
            if (ModelState.IsValid)
            {
                _context.Add(envanter);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"{envanter.DeviceName} adlı cihaz başarıyla oluşturuldu.";
                return RedirectToAction(nameof(Index));
            }
            return View(envanter);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var envanter = await _context.Envanterler.FindAsync(id);
            if (envanter == null) return NotFound();
            return View(envanter);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,DeviceName,Tur,ServiceTagSerialNumber,Model,IpAddress,VcenterAddress,ClusterName,Location,OperatingSystem,IloIdracIp,Kabin,RearFront,KabinU")] Envanter envanter)
        {
            if (id != envanter.ID) return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(envanter);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EnvanterExists(envanter.ID)) return NotFound();
                    else throw;
                }
                TempData["SuccessMessage"] = $"{envanter.DeviceName} adlı cihaz başarıyla güncellendi.";
                return RedirectToAction(nameof(Index));
            }
            return View(envanter);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var envanter = await _context.Envanterler.FirstOrDefaultAsync(m => m.ID == id);
            if (envanter == null) return NotFound();
            return View(envanter);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var envanter = await _context.Envanterler.FindAsync(id);
            if (envanter != null)
            {
                var relatedDetails = await _context.EnvanterDetails.Where(d => d.EnvanterID == id).ToListAsync();
                if (relatedDetails.Any()) _context.EnvanterDetails.RemoveRange(relatedDetails);

                _context.Envanterler.Remove(envanter);
                await _context.SaveChangesAsync();
                TempData["InfoMessage"] = $"{envanter.DeviceName} adlı cihaz ve ilişkili port detayları silindi.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportFromExcel(IFormFile excelFile, bool deleteAll)
        {
            if (excelFile == null || excelFile.Length == 0)
            {
                TempData["ErrorMessage"] = "Lütfen bir Excel dosyası seçin.";
                return RedirectToAction(nameof(Index));
            }

            int deletedCount = 0;
            if (deleteAll)
            {
                await _context.Database.ExecuteSqlRawAsync("DELETE FROM [Baglantilar]");
                await _context.Database.ExecuteSqlRawAsync("DELETE FROM [EnvanterDetails]");

                var allRecords = await _context.Envanterler.ToListAsync();
                deletedCount = allRecords.Count;
                if (deletedCount > 0)
                {
                    _context.Envanterler.RemoveRange(allRecords);
                    await _context.SaveChangesAsync();

                    await _context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('Envanterler', RESEED, 0)");
                }
            }

            var envanterList = new List<Envanter>();
            using (var stream = new MemoryStream())
            {
                await excelFile.CopyToAsync(stream);
                using (var workbook = new ClosedXML.Excel.XLWorkbook(stream))
                {
                    var worksheet = workbook.Worksheets.First();
                    var rows = worksheet.RowsUsed().Skip(1);
                    foreach (var row in rows)
                    {
                        try
                        {
                            var deviceName = row.Cell(2).Value.ToString().Trim();
                            if (string.IsNullOrEmpty(deviceName)) continue;
                            var envanter = new Envanter
                            {
                                DeviceName = deviceName,
                                IpAddress = row.Cell(3).Value.ToString().Trim(),
                                Model = row.Cell(4).Value.ToString().Trim(),
                                ServiceTagSerialNumber = row.Cell(5).Value.ToString().Trim(),
                                VcenterAddress = row.Cell(6).Value.ToString().Trim(),
                                ClusterName = row.Cell(7).Value.ToString().Trim(),
                                Location = row.Cell(8).Value.ToString().Trim(),
                                OperatingSystem = row.Cell(9).Value.ToString().Trim(),
                                IloIdracIp = row.Cell(10).Value.ToString().Trim(),
                                Kabin = row.Cell(11).Value.ToString().Trim(),
                                RearFront = row.Cell(12).Value.ToString().Trim(),
                                KabinU = row.Cell(13).Value.ToString().Trim(),
                                Tur = row.Cell(14).Value.ToString().Trim()
                            };
                            if (envanter.IpAddress?.ToUpper() == "N/A") envanter.IpAddress = null;
                            if (envanter.VcenterAddress?.ToUpper() == "N/A") envanter.VcenterAddress = null;
                            if (envanter.ClusterName?.ToUpper() == "N/A") envanter.ClusterName = null;
                            if (string.IsNullOrWhiteSpace(envanter.Tur)) envanter.Tur = "Belirsiz";
                            if (!await _context.Envanterler.AnyAsync(e => e.DeviceName == envanter.DeviceName))
                            {
                                envanterList.Add(envanter);
                            }
                        }
                        catch (Exception ex)
                        {
                            TempData["ErrorMessage"] = $"Hata: Excel dosyasının {row.RowNumber()}. satırı okunurken bir sorun oluştu. Detay: {ex.Message}";
                            return RedirectToAction(nameof(Index));
                        }
                    }
                }
            }

            if (envanterList.Any())
            {
                await _context.Envanterler.AddRangeAsync(envanterList);
                await _context.SaveChangesAsync();
            }

            var successMessage = new StringBuilder();
            if (deleteAll) successMessage.Append($"{deletedCount} envanter ve ilişkili tüm kayıtlar silindi. Veritabanı sıfırlandı. ");
            successMessage.Append($"{envanterList.Count} yeni kayıt başarıyla eklendi.");
            TempData["SuccessMessage"] = successMessage.ToString();

            return RedirectToAction(nameof(Index));
        }

        private bool EnvanterExists(int id)
        {
            return _context.Envanterler.Any(e => e.ID == id);
        }
        #endregion
    }
}