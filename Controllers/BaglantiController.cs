using BLBM_ENV.Data;
using BLBM_ENV.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;
using System.Text;
using System.Linq;


namespace BLBM_ENV.Controllers
{
    public class BaglantiController : Controller
    {
        private readonly ApplicationDbContext _context;
        // Bu sabit artık doğrudan kullanılmıyor, kod ilk sayfayı alacak şekilde güncellendi.
        private const string BaglantiSayfaAdi = "Sayfa1";


        public BaglantiController(ApplicationDbContext context)
        {
            _context = context;
        }


        public async Task<IActionResult> Index()
        {
            var allConnectionsQuery = from b in _context.Baglantilar
                                      join ed in _context.EnvanterDetails
                                      on new { EnvanterID = (int)b.SourceDeviceID, PortID = b.Source_Port }
                                      equals new { EnvanterID = ed.EnvanterID, PortID = ed.PortID } into details
                                      from ed in details.DefaultIfEmpty()
                                      select new BaglantiIndexViewModel
                                      {
                                          BaglantiID = b.ID,
                                          SourceDeviceID = b.SourceDeviceID,
                                          SourceDeviceName = b.SourceDevice.DeviceName,
                                          SourcePort = b.Source_Port,
                                          TargetDeviceID = b.TargetDeviceID,
                                          TargetDeviceName = b.TargetDevice.DeviceName,
                                          TargetPort = b.Target_Port,
                                          ConnectionType = b.ConnectionType,
                                          LinkStatus = ed.LinkStatus,
                                          LinkSpeed = ed.LinkSpeed,
                                          NicID = ed.NicID,
                                          BakirMAC = ed.BakirMAC,
                                          FiberMAC = ed.FiberMAC,
                                          WWPN = ed.WWPN
                                      };


            var allConnections = await allConnectionsQuery.ToListAsync();


            ViewBag.PhysicalConnections = allConnections
                .Where(c => c.ConnectionType != null && !c.ConnectionType.StartsWith("Virtual", StringComparison.OrdinalIgnoreCase))
                .ToList();


            ViewBag.VirtualConnections = allConnections
                .Where(c => c.ConnectionType != null && c.ConnectionType.StartsWith("Virtual", StringComparison.OrdinalIgnoreCase))
                .ToList();


            ViewBag.AllDevices = await _context.Envanterler.OrderBy(d => d.DeviceName).ToListAsync();


            return View();
        }


        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var baglanti = await _context.Baglantilar.Include(b => b.SourceDevice).Include(b => b.TargetDevice).FirstOrDefaultAsync(m => m.ID == id);
            if (baglanti == null) return NotFound();
            return View(baglanti);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SourceDeviceID,TargetDeviceID,Source_Port,Target_Port,ConnectionType,LinkStatus,LinkSpeed")] Baglanti baglanti)
        {
            if (ModelState.IsValid)
            {
                var sourceDevice = await _context.Envanterler.AsNoTracking().FirstOrDefaultAsync(e => e.ID == baglanti.SourceDeviceID);
                var targetDevice = await _context.Envanterler.AsNoTracking().FirstOrDefaultAsync(e => e.ID == baglanti.TargetDeviceID);
                if (sourceDevice == null || targetDevice == null) { TempData["ErrorMessage"] = "Kaynak veya hedef cihaz bulunamadı!"; return RedirectToAction(nameof(Index)); }
                baglanti.Source_DeviceName = sourceDevice.DeviceName;
                baglanti.Target_DeviceName = targetDevice.DeviceName;
                _context.Add(baglanti);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"{sourceDevice.DeviceName} -> {targetDevice.DeviceName} bağlantısı başarıyla oluşturuldu.";
                return RedirectToAction(nameof(Index));
            }
            TempData["ErrorMessage"] = "Bağlantı oluşturulurken bir hata oluştu. Lütfen bilgileri kontrol edin.";
            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var baglanti = await _context.Baglantilar.Include(b => b.SourceDevice).Include(b => b.TargetDevice).FirstOrDefaultAsync(m => m.ID == id);
            if (baglanti == null) return NotFound();
            return View(baglanti);
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var baglanti = await _context.Baglantilar.FindAsync(id);
            if (baglanti != null)
            {
                _context.Baglantilar.Remove(baglanti);
                await _context.SaveChangesAsync();
                TempData["InfoMessage"] = "Bağlantı başarıyla silindi.";
            }
            return RedirectToAction(nameof(Index));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportFromExcel(IFormFile excelFile, bool deleteAll)
        {
            if (excelFile == null || excelFile.Length == 0) { TempData["ErrorMessage"] = "Lütfen bir Excel dosyası seçin."; return RedirectToAction(nameof(Index)); }
            var connectionsToAdd = new List<Baglanti>();
            var detailsToAdd = new List<EnvanterDetail>();
            var errors = new List<string>();
            if (deleteAll)
            {
                await _context.Database.ExecuteSqlRawAsync("DELETE FROM [Baglantilar]");
                await _context.Database.ExecuteSqlRawAsync("DELETE FROM [EnvanterDetails]");
            }
            var allDevices = await _context.Envanterler.AsNoTracking().ToListAsync();
            using (var stream = new MemoryStream())
            {
                await excelFile.CopyToAsync(stream); using (var workbook = new XLWorkbook(stream))
                {
                    // --- DÜZELTME BURADA: Belirli bir sayfa adı yerine, dosyadaki İLK sayfayı al ---
                    var worksheet = workbook.Worksheets.FirstOrDefault();
                    if (worksheet == null) { TempData["ErrorMessage"] = "Excel dosyasında okunacak bir sayfa bulunamadı."; return RedirectToAction(nameof(Index)); }

                    var rows = worksheet.RowsUsed().Skip(1); foreach (var row in rows)
                    {
                        try
                        {
                            var sourceIdentifier = row.Cell("D").Value.ToString().Trim();
                            if (string.IsNullOrEmpty(sourceIdentifier))
                            {
                                sourceIdentifier = row.Cell("C").Value.ToString().Trim();
                            }
                            if (string.IsNullOrEmpty(sourceIdentifier)) continue;
                            var sourceDevice = allDevices.FirstOrDefault(d =>
                                (d.ServiceTagSerialNumber != null && d.ServiceTagSerialNumber.Equals(sourceIdentifier, StringComparison.OrdinalIgnoreCase)) ||
                                d.DeviceName.Equals(sourceIdentifier, StringComparison.OrdinalIgnoreCase));
                            if (sourceDevice == null) { errors.Add($"Satır {row.RowNumber()}: Kaynak cihaz '{sourceIdentifier}' bulunamadı."); continue; }

                            detailsToAdd.Add(new EnvanterDetail { EnvanterID = sourceDevice.ID, Turu = row.Cell("B").Value.ToString().Trim(), DeviceName = sourceDevice.DeviceName, LinkStatus = row.Cell("G").Value.ToString().Trim(), LinkSpeed = row.Cell("H").Value.ToString().Trim(), PortID = row.Cell("I").Value.ToString().Trim(), NicID = row.Cell("J").Value.ToString().Trim(), FiberMAC = row.Cell("K").Value.ToString().Trim(), BakirMAC = row.Cell("L").Value.ToString().Trim(), WWPN = row.Cell("M").Value.ToString() });

                            var targetIdentifier = row.Cell("N").Value.ToString().Trim();
                            if (!string.IsNullOrEmpty(targetIdentifier))
                            {
                                var targetDevice = allDevices.FirstOrDefault(d =>
                                    (d.ServiceTagSerialNumber != null && d.ServiceTagSerialNumber.Equals(targetIdentifier, StringComparison.OrdinalIgnoreCase)) ||
                                    d.DeviceName.Equals(targetIdentifier, StringComparison.OrdinalIgnoreCase));
                                if (targetDevice == null) { errors.Add($"Satır {row.RowNumber()}: Hedef cihaz '{targetIdentifier}' bulunamadı."); continue; }

                                connectionsToAdd.Add(new Baglanti { SourceDeviceID = sourceDevice.ID, TargetDeviceID = targetDevice.ID, Source_Port = row.Cell("I").Value.ToString().Trim(), Target_Port = row.Cell("O").Value.ToString().Trim(), ConnectionType = row.Cell("B").Value.ToString().Trim() });
                            }
                        }
                        catch (Exception ex) { errors.Add($"Satır {row.RowNumber()}: İşlenirken hata - {ex.Message}"); }
                    }
                }
            }
            if (connectionsToAdd.Any()) await _context.Baglantilar.AddRangeAsync(connectionsToAdd); if (detailsToAdd.Any()) await _context.EnvanterDetails.AddRangeAsync(detailsToAdd); await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"{(deleteAll ? "Tüm kayıtlar silindi. " : "")}{connectionsToAdd.Count} bağlantı ve {detailsToAdd.Count} port detayı eklendi.";
            if (errors.Any())
            {
                const int maxErrorsToShow = 20;
                var totalErrorCount = errors.Count;
                var errorMessages = errors.Take(maxErrorsToShow).ToList();
                if (totalErrorCount > maxErrorsToShow)
                {
                    errorMessages.Add($"<br><b>... ve toplam {totalErrorCount - maxErrorsToShow} diğer hata daha bulundu.</b>");
                }
                TempData["ErrorMessage"] = string.Join("<br>", errorMessages);
            }
            return RedirectToAction(nameof(Index));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportVirtualPathFromExcel(IFormFile excelFile, bool deleteVirtuals)
        {
            if (excelFile == null || excelFile.Length == 0) { TempData["ErrorMessage"] = "Lütfen bir Excel dosyası seçin."; return RedirectToAction(nameof(Index)); }

            if (deleteVirtuals)
            {
                var virtualsToDelete = _context.Baglantilar.Where(b => b.ConnectionType.StartsWith("Virtual"));
                if (await virtualsToDelete.AnyAsync())
                {
                    _context.Baglantilar.RemoveRange(virtualsToDelete);
                }
            }

            var connectionsToAdd = new List<Baglanti>(); var detailsToAdd = new List<EnvanterDetail>(); var errorList = new List<string>(); var allDevices = await _context.Envanterler.AsNoTracking().ToListAsync(); var allConnections = await _context.Baglantilar.AsNoTracking().ToListAsync();
            using (var stream = new MemoryStream())
            {
                await excelFile.CopyToAsync(stream); using (var workbook = new XLWorkbook(stream))
                {
                    // --- DÜZELTME BURADA: Belirli bir sayfa adı yerine, dosyadaki İLK sayfayı al ---
                    var worksheet = workbook.Worksheets.FirstOrDefault();
                    if (worksheet == null) { TempData["ErrorMessage"] = "Excel dosyasında okunacak bir sayfa bulunamadı."; return RedirectToAction(nameof(Index)); }

                    var rows = worksheet.RowsUsed().Skip(1); foreach (var row in rows)
                    {
                        try
                        {
                            var connectionType = row.Cell("B").Value.ToString().Trim(); if (!connectionType.StartsWith("Virtual", StringComparison.OrdinalIgnoreCase)) continue;

                            var sourceIdentifier = row.Cell("D").Value.ToString().Trim();
                            if (string.IsNullOrEmpty(sourceIdentifier))
                            {
                                sourceIdentifier = row.Cell("C").Value.ToString().Trim();
                            }
                            if (string.IsNullOrEmpty(sourceIdentifier)) continue;
                            var sourceDevice = allDevices.FirstOrDefault(d =>
                                (d.ServiceTagSerialNumber != null && d.ServiceTagSerialNumber.Equals(sourceIdentifier, StringComparison.OrdinalIgnoreCase)) ||
                                d.DeviceName.Equals(sourceIdentifier, StringComparison.OrdinalIgnoreCase));
                            if (sourceDevice == null) { errorList.Add($"Satır {row.RowNumber()}: Başlangıç cihazı '{sourceIdentifier}' bulunamadı."); continue; }

                            var sourcePort = row.Cell("I").Value.ToString().Trim(); var swNameRaw = row.Cell("N").Value.ToString().Trim(); detailsToAdd.Add(new EnvanterDetail { EnvanterID = sourceDevice.ID, Turu = connectionType, DeviceName = sourceDevice.DeviceName, LinkStatus = row.Cell("G").Value.ToString().Trim(), LinkSpeed = row.Cell("H").Value.ToString().Trim(), PortID = sourcePort, NicID = row.Cell("J").Value.ToString().Trim(), FiberMAC = row.Cell("K").Value.ToString().Trim(), BakirMAC = row.Cell("L").Value.ToString().Trim(), WWPN = row.Cell("M").Value.ToString() });
                            if (string.IsNullOrEmpty(swNameRaw)) continue; var physicalPathReferences = swNameRaw.Split(',').Select(p => p.Trim()).ToList(); foreach (var pathRef in physicalPathReferences)
                            {
                                var pathParts = pathRef.Split(new[] { '_' }, 2); if (pathParts.Length < 2) { errorList.Add($"Satır {row.RowNumber()}: SW NAME formatı hatalı: '{pathRef}'"); continue; }
                                var refIdentifier = pathParts[0]; var refPhysicalPort = pathParts[1];
                                var intermediateDevice = allDevices.FirstOrDefault(d => d.DeviceName.Equals(refIdentifier, StringComparison.OrdinalIgnoreCase) || (d.ServiceTagSerialNumber != null && d.ServiceTagSerialNumber.Equals(refIdentifier, StringComparison.OrdinalIgnoreCase)));
                                if (intermediateDevice == null) { errorList.Add($"Satır {row.RowNumber()}: Ara cihaz '{refIdentifier}' bulunamadı."); continue; }
                                var physicalConnection = allConnections.FirstOrDefault(c => (c.SourceDeviceID == intermediateDevice.ID && c.Source_Port.Equals(refPhysicalPort, StringComparison.OrdinalIgnoreCase)) || (c.TargetDeviceID == intermediateDevice.ID && c.Target_Port.Equals(refPhysicalPort, StringComparison.OrdinalIgnoreCase))); if (physicalConnection == null) { errorList.Add($"Satır {row.RowNumber()}: Ara cihaz '{intermediateDevice.DeviceName}' port '{refPhysicalPort}' için bağlantı bulunamadı."); continue; }
                                var finalDeviceId = physicalConnection.SourceDeviceID == intermediateDevice.ID ? physicalConnection.TargetDeviceID : physicalConnection.SourceDeviceID; var finalPort = physicalConnection.SourceDeviceID == intermediateDevice.ID ? physicalConnection.Target_Port : physicalConnection.Source_Port;

                                var newVirtualConnection = new Baglanti
                                {
                                    SourceDeviceID = sourceDevice.ID,
                                    TargetDeviceID = finalDeviceId,
                                    Source_Port = sourcePort,
                                    Target_Port = finalPort,
                                    ConnectionType = $"{connectionType} (via {pathRef})"
                                };

                                bool alreadyExists = connectionsToAdd.Any(c =>
                                    c.SourceDeviceID == newVirtualConnection.SourceDeviceID &&
                                    c.Source_Port == newVirtualConnection.Source_Port &&
                                    c.TargetDeviceID == newVirtualConnection.TargetDeviceID &&
                                    c.Target_Port == newVirtualConnection.Target_Port);

                                if (!alreadyExists)
                                {
                                    connectionsToAdd.Add(newVirtualConnection);
                                }
                            }
                        }
                        catch (Exception ex) { errorList.Add($"Satır {row.RowNumber()}: İşlenirken hata - {ex.Message}"); }
                    }
                }
            }
            if (connectionsToAdd.Any()) await _context.Baglantilar.AddRangeAsync(connectionsToAdd); if (detailsToAdd.Any()) await _context.EnvanterDetails.AddRangeAsync(detailsToAdd);

            await _context.SaveChangesAsync();

            var deleteMessage = deleteVirtuals ? "Mevcut sanal bağlantılar silindi. " : "";
            TempData["SuccessMessage"] = $"{deleteMessage}{connectionsToAdd.Count} sanal bağlantı ve {detailsToAdd.Count} port detayı işlendi.";

            if (errorList.Any())
            {
                const int maxErrorsToShow = 20;
                var totalErrorCount = errorList.Count;
                var errorMessages = errorList.Take(maxErrorsToShow).ToList();
                if (totalErrorCount > maxErrorsToShow)
                {
                    errorMessages.Add($"<br><b>... ve toplam {totalErrorCount - maxErrorsToShow} diğer hata daha bulundu.</b>");
                }
                TempData["ErrorMessage"] = string.Join("<br>", errorMessages);
            }
            return RedirectToAction(nameof(Index));
        }
    }
}