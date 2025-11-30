using BLBM_ENV.Data;
using BLBM_ENV.Models;
using BLBM_ENV.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using ClosedXML.Excel;
using CsvHelper;
using System.Globalization;
using System.Text;

namespace BLBM_ENV.Controllers
{
    public class NaturalStringComparer : IComparer<string>
    {
        public int Compare(string? x, string? y)
        {
            if (x == null || y == null) return 0;
            var regex = new Regex("([0-9]+)");
            var xParts = regex.Split(x);
            var yParts = regex.Split(y);
            int partCount = Math.Min(xParts.Length, yParts.Length);
            for (int i = 0; i < partCount; i++)
            {
                if (xParts[i] == yParts[i]) continue;
                bool xIsNumeric = int.TryParse(xParts[i], out int xNum);
                bool yIsNumeric = int.TryParse(yParts[i], out int yNum);
                if (xIsNumeric && yIsNumeric)
                {
                    if (xNum != yNum) return xNum.CompareTo(yNum);
                }
                else
                {
                    int stringCompare = string.Compare(xParts[i], yParts[i], StringComparison.OrdinalIgnoreCase);
                    if (stringCompare != 0) return stringCompare;
                }
            }
            return x.Length.CompareTo(y.Length);
        }
    }

    [Authorize]
    public class EnvanterController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditLogger _auditLogger;

        public EnvanterController(ApplicationDbContext context, IAuditLogger auditLogger)
        {
            _context = context;
            _auditLogger = auditLogger;
        }

        public async Task<IActionResult> Index()
        {
            var envanterler = await _context.Envanterler
                .Include(e => e.AsSourceConnections)
                .Include(e => e.AsTargetConnections)
                .OrderBy(e => e.DeviceName)
                .ToListAsync();
            return View(envanterler);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var envanter = await _context.Envanterler.FirstOrDefaultAsync(m => m.ID == id);
            if (envanter == null) return NotFound();

            ViewBag.AllDevicesForSelectList = await _context.Envanterler.Where(e => e.ID != id).OrderBy(e => e.DeviceName).ToListAsync();
            ViewBag.AllConnections = await _context.Baglantilar.Select(b => new { SourceDeviceName = b.SourceDevice.DeviceName, SourcePort = b.Source_Port, TargetDeviceName = b.TargetDevice.DeviceName, TargetPort = b.Target_Port }).ToListAsync();

            var unifiedPortList = new List<UnifiedPortViewModel>();
            var processedConnections = new HashSet<Tuple<int, int>>();

            var sourceConnections = await _context.Baglantilar
                .Include(b => b.SourceDevice)
                .Include(b => b.TargetDevice)
                .Where(b => b.SourceDeviceID == id)
                .ToListAsync();

            foreach (var c in sourceConnections)
            {
                unifiedPortList.Add(new UnifiedPortViewModel
                {
                    Port = c.Source_Port,
                    ConnectionID = c.ID,
                    Type = c.ConnectionType,
                    LinkStatus = c.Source_LinkStatus,
                    LinkSpeed = c.Source_LinkSpeed,
                    BakirMAC = c.Source_BakirMAC,
                    FiberMAC = c.Source_FiberMAC,
                    WWPN = c.Source_WWPN,
                    IsConnected = c.TargetDeviceID != null,
                    RemoteDevice = c.TargetDevice,
                    RemotePort = c.Target_Port,
                    IsVirtual = (c.ConnectionType ?? "").StartsWith("Virtual")
                });
                if (c.TargetDeviceID.HasValue)
                {
                    processedConnections.Add(new Tuple<int, int>(Math.Min(c.SourceDeviceID.Value, c.TargetDeviceID.Value), Math.Max(c.SourceDeviceID.Value, c.TargetDeviceID.Value)));
                }
            }

            var targetConnections = await _context.Baglantilar
                .Include(b => b.SourceDevice)
                .Include(b => b.TargetDevice)
                .Where(b => b.TargetDeviceID == id)
                .ToListAsync();

            foreach (var c in targetConnections)
            {
                if (c.SourceDeviceID.HasValue)
                {
                    var connectionTuple = new Tuple<int, int>(Math.Min(c.SourceDeviceID.Value, c.TargetDeviceID.Value), Math.Max(c.SourceDeviceID.Value, c.TargetDeviceID.Value));
                    if (processedConnections.Contains(connectionTuple)) continue;
                }

                unifiedPortList.Add(new UnifiedPortViewModel
                {
                    Port = c.Target_Port,
                    ConnectionID = c.ID,
                    Type = c.ConnectionType,
                    LinkStatus = c.Target_LinkStatus,
                    LinkSpeed = c.Target_LinkSpeed,
                    BakirMAC = c.Target_BakirMAC,
                    FiberMAC = c.Target_FiberMAC,
                    WWPN = c.Target_WWPN,
                    IsConnected = true,
                    RemoteDevice = c.SourceDevice,
                    RemotePort = c.Source_Port,
                    IsVirtual = (c.ConnectionType ?? "").StartsWith("Virtual")
                });
            }

            var remoteDeviceIds = unifiedPortList.Where(p => p.IsConnected && !p.IsVirtual && p.RemoteDevice != null).Select(p => p.RemoteDevice.ID).Distinct();
            if (remoteDeviceIds.Any())
            {
                var relevantVirtualConnections = await _context.Baglantilar
                    .Include(b => b.SourceDevice)
                    .Where(b => b.ConnectionType.StartsWith("Virtual") && b.SourceDeviceID.HasValue && remoteDeviceIds.Contains(b.SourceDeviceID.Value))
                    .ToListAsync();

                foreach (var portModel in unifiedPortList.Where(p => p.IsConnected && !p.IsVirtual && p.RemoteDevice != null))
                {
                    var expectedViaText = $"_{portModel.RemotePort}";
                    portModel.PassthroughConnections = relevantVirtualConnections
                        .Where(vc => vc.SourceDeviceID == portModel.RemoteDevice.ID && (vc.ConnectionType ?? "").Contains(expectedViaText))
                        .ToList();
                }
            }

            ViewBag.UnifiedPortList = unifiedPortList.OrderBy(p => p.Port, new NaturalStringComparer()).ToList();
            return View(envanter);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            // Mevcut lokasyonları listele (Datalist için)
            ViewBag.ExistingLocations = await _context.Envanterler
                .Where(e => !string.IsNullOrEmpty(e.Location))
                .Select(e => e.Location).Distinct().OrderBy(l => l).ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("ID,DeviceName,Tur,ServiceTagSerialNumber,Model,IpAddress,VcenterAddress,ClusterName,Location,OperatingSystem,IloIdracIp,Kabin,RearFront,KabinU")] Envanter envanter)
        {
            if (ModelState.IsValid)
            {
                _context.Add(envanter);
                await _context.SaveChangesAsync();
                await _auditLogger.LogAsync("Ekleme", "Envanter", $"Yeni cihaz eklendi: {envanter.DeviceName}");
                TempData["SuccessMessage"] = $"{envanter.DeviceName} adlı cihaz başarıyla oluşturuldu.";
                return RedirectToAction(nameof(Index));
            }
            return View(envanter);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var envanter = await _context.Envanterler.FindAsync(id);
            if (envanter == null) return NotFound();

            // --- Datalist İçin Lokasyonları Çek ---
            ViewBag.ExistingLocations = await _context.Envanterler
                .Where(e => !string.IsNullOrEmpty(e.Location))
                .Select(e => e.Location).Distinct().OrderBy(l => l).ToListAsync();

            return View(envanter);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("ID,DeviceName,Tur,ServiceTagSerialNumber,Model,IpAddress,VcenterAddress,ClusterName,Location,OperatingSystem,IloIdracIp,Kabin,RearFront,KabinU,RowVersion")] Envanter envanter)
        {
            if (id != envanter.ID) return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(envanter);
                    await _context.SaveChangesAsync();
                    await _auditLogger.LogAsync("Güncelleme", "Envanter", $"{envanter.DeviceName} güncellendi.");
                    TempData["SuccessMessage"] = $"{envanter.DeviceName} başarıyla güncellendi.";
                    return RedirectToAction(nameof(Details), new { id = envanter.ID });
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    var entry = ex.Entries.Single();
                    var databaseValues = await entry.GetDatabaseValuesAsync();
                    if (databaseValues == null)
                    {
                        ModelState.AddModelError(string.Empty, "HATA: Bu kayıt siz düzenlerken silinmiş.");
                    }
                    else
                    {
                        var dbEntity = (Envanter)databaseValues.ToObject();
                        ModelState.AddModelError(string.Empty, "HATA: Bu kayıt başka bir kullanıcı tarafından değiştirildi.");
                        ModelState.AddModelError(string.Empty, $"Güncel Değerler -> Ad: {dbEntity.DeviceName}, IP: {dbEntity.IpAddress}");
                        envanter.RowVersion = (byte[])dbEntity.RowVersion;
                    }
                }
            }
            return View(envanter);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var envanter = await _context.Envanterler.FirstOrDefaultAsync(m => m.ID == id);
            if (envanter == null) return NotFound();
            return View(envanter);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var envanter = await _context.Envanterler.FindAsync(id);
            if (envanter != null)
            {
                string deviceName = envanter.DeviceName;
                var relatedConnections = _context.Baglantilar.Where(b => b.SourceDeviceID == id || b.TargetDeviceID == id);
                if (await relatedConnections.AnyAsync())
                {
                    _context.Baglantilar.RemoveRange(relatedConnections);
                }
                _context.Envanterler.Remove(envanter);
                await _context.SaveChangesAsync();
                await _auditLogger.LogAsync("Silme", "Envanter", $"{deviceName} ve bağlantıları silindi.");
                TempData["InfoMessage"] = $"{deviceName} silindi.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ImportFromExcel(IFormFile excelFile, bool deleteAll)
        {
            if (!FileSecurityHelper.IsValidFile(excelFile))
            {
                TempData["ErrorMessage"] = "Güvenlik Uyarısı: Dosya geçersiz.";
                await _auditLogger.LogAsync("Güvenlik", "Envanter", "Geçersiz dosya yükleme girişimi.");
                return RedirectToAction(nameof(Index));
            }

            int deletedCount = 0;
            if (deleteAll)
            {
                await _context.Database.ExecuteSqlRawAsync("DELETE FROM [Baglantilar]");
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
                using (var workbook = new XLWorkbook(stream))
                {
                    var worksheet = workbook.Worksheets.First();
                    var rows = worksheet.RowsUsed().Skip(1);
                    foreach (var row in rows)
                    {
                        try
                        {
                            var deviceName = row.Cell(2).Value.ToString().Trim();
                            if (string.IsNullOrEmpty(deviceName)) continue;
                            var envanter = new Envanter { DeviceName = deviceName, IpAddress = row.Cell(3).Value.ToString().Trim(), Model = row.Cell(4).Value.ToString().Trim(), ServiceTagSerialNumber = row.Cell(5).Value.ToString().Trim(), VcenterAddress = row.Cell(6).Value.ToString().Trim(), ClusterName = row.Cell(7).Value.ToString().Trim(), Location = row.Cell(8).Value.ToString().Trim(), OperatingSystem = row.Cell(9).Value.ToString().Trim(), IloIdracIp = row.Cell(10).Value.ToString().Trim(), Kabin = row.Cell(11).Value.ToString().Trim(), RearFront = row.Cell(12).Value.ToString().Trim(), KabinU = row.Cell(13).Value.ToString().Trim(), Tur = row.Cell(14).Value.ToString().Trim() };
                            if (envanter.IpAddress?.ToUpper() == "N/A") envanter.IpAddress = null;
                            if (envanter.VcenterAddress?.ToUpper() == "N/A") envanter.VcenterAddress = null;
                            if (envanter.ClusterName?.ToUpper() == "N/A") envanter.ClusterName = null;
                            if (string.IsNullOrWhiteSpace(envanter.Tur)) envanter.Tur = "Belirsiz";
                            if (!await _context.Envanterler.AnyAsync(e => e.DeviceName == envanter.DeviceName)) { envanterList.Add(envanter); }
                        }
                        catch (Exception ex) { TempData["ErrorMessage"] = $"Hata: {row.RowNumber()}. satır okunamadı."; return RedirectToAction(nameof(Index)); }
                    }
                }
            }

            if (envanterList.Any())
            {
                await _context.Envanterler.AddRangeAsync(envanterList);
                await _context.SaveChangesAsync();
            }

            await _auditLogger.LogAsync("Toplu Yükleme", "Envanter", $"Excel ile {envanterList.Count} cihaz eklendi.");
            var successMessage = new StringBuilder();
            if (deleteAll) successMessage.Append($"{deletedCount} kayıt silindi. ");
            successMessage.Append($"{envanterList.Count} yeni kayıt eklendi.");
            TempData["SuccessMessage"] = successMessage.ToString();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> ExportToExcel()
        {
            var envanterler = await _context.Envanterler.AsNoTracking().ToListAsync();
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Envanter Listesi");
                var currentRow = 1;
                worksheet.Cell(currentRow, 1).Value = "ID";
                worksheet.Cell(currentRow, 2).Value = "DeviceName";
                // ... Diğer başlıklar ...
                worksheet.Row(1).Style.Font.Bold = true;

                foreach (var envanter in envanterler)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = envanter.ID;
                    worksheet.Cell(currentRow, 2).Value = envanter.DeviceName;
                    // ... Diğer veriler ...
                }
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"EnvanterListesi_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
                }
            }
        }

        public async Task<IActionResult> ExportToCsv()
        {
            var envanterler = await _context.Envanterler.AsNoTracking().ToListAsync();
            using (var memoryStream = new MemoryStream())
            using (var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8))
            using (var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture))
            {
                csvWriter.WriteRecords(envanterler);
                streamWriter.Flush();
                return File(memoryStream.ToArray(), "text/csv", $"EnvanterListesi_{DateTime.Now:yyyyMMddHHmmss}.csv");
            }
        }

        private bool EnvanterExists(int id)
        {
            return _context.Envanterler.Any(e => e.ID == id);
        }
    }
}