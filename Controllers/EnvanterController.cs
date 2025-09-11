using BLBM_ENV.Data;
using BLBM_ENV.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.RegularExpressions;
using ClosedXML.Excel;
using CsvHelper;
using System.Globalization;

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
                return RedirectToAction(nameof(Details), new { id = envanter.ID });
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
                var relatedConnections = _context.Baglantilar.Where(b => b.SourceDeviceID == id || b.TargetDeviceID == id);
                if (await relatedConnections.AnyAsync())
                {
                    _context.Baglantilar.RemoveRange(relatedConnections);
                }

                _context.Envanterler.Remove(envanter);
                await _context.SaveChangesAsync();
                TempData["InfoMessage"] = $"{envanter.DeviceName} adlı cihaz ve ilişkili tüm bağlantıları silindi.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportFromExcel(IFormFile excelFile, bool deleteAll)
        {
            if (excelFile == null || excelFile.Length == 0) { TempData["ErrorMessage"] = "Lütfen bir Excel dosyası seçin."; return RedirectToAction(nameof(Index)); }
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
                        catch (Exception ex) { TempData["ErrorMessage"] = $"Hata: Excel dosyasının {row.RowNumber()}. satırı okunurken bir sorun oluştu. Detay: {ex.Message}"; return RedirectToAction(nameof(Index)); }
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

        public async Task<IActionResult> ExportToExcel()
        {
            var envanterler = await _context.Envanterler.AsNoTracking().ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Envanter Listesi");
                var currentRow = 1;

                worksheet.Cell(currentRow, 1).Value = "ID";
                worksheet.Cell(currentRow, 2).Value = "DeviceName";
                worksheet.Cell(currentRow, 3).Value = "IpAddress";
                worksheet.Cell(currentRow, 4).Value = "Model";
                worksheet.Cell(currentRow, 5).Value = "ServiceTagSerialNumber";
                worksheet.Cell(currentRow, 6).Value = "VcenterAddress";
                worksheet.Cell(currentRow, 7).Value = "ClusterName";
                worksheet.Cell(currentRow, 8).Value = "Location";
                worksheet.Cell(currentRow, 9).Value = "OperatingSystem";
                worksheet.Cell(currentRow, 10).Value = "IloIdracIp";
                worksheet.Cell(currentRow, 11).Value = "Kabin";
                worksheet.Cell(currentRow, 12).Value = "RearFront";
                worksheet.Cell(currentRow, 13).Value = "KabinU";
                worksheet.Cell(currentRow, 14).Value = "Tur";
                worksheet.Row(1).Style.Font.Bold = true;

                foreach (var envanter in envanterler)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = envanter.ID;
                    worksheet.Cell(currentRow, 2).Value = envanter.DeviceName;
                    worksheet.Cell(currentRow, 3).Value = envanter.IpAddress;
                    worksheet.Cell(currentRow, 4).Value = envanter.Model;
                    worksheet.Cell(currentRow, 5).Value = envanter.ServiceTagSerialNumber;
                    worksheet.Cell(currentRow, 6).Value = envanter.VcenterAddress;
                    worksheet.Cell(currentRow, 7).Value = envanter.ClusterName;
                    worksheet.Cell(currentRow, 8).Value = envanter.Location;
                    worksheet.Cell(currentRow, 9).Value = envanter.OperatingSystem;
                    worksheet.Cell(currentRow, 10).Value = envanter.IloIdracIp;
                    worksheet.Cell(currentRow, 11).Value = envanter.Kabin;
                    worksheet.Cell(currentRow, 12).Value = envanter.RearFront;
                    worksheet.Cell(currentRow, 13).Value = envanter.KabinU;
                    worksheet.Cell(currentRow, 14).Value = envanter.Tur;
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    var fileName = $"EnvanterListesi_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                    return File(content, contentType, fileName);
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

                var content = memoryStream.ToArray();
                var contentType = "text/csv";
                var fileName = $"EnvanterListesi_{DateTime.Now:yyyyMMddHHmmss}.csv";
                return File(content, contentType, fileName);
            }
        }

        private bool EnvanterExists(int id)
        {
            return _context.Envanterler.Any(e => e.ID == id);
        }
    }
}