using BLBM_ENV.Data;
using BLBM_ENV.Models;
using BLBM_ENV.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;
using System.Text;
using CsvHelper;
using System.Globalization;

namespace BLBM_ENV.Controllers
{
    [Authorize]
    public class BaglantiController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditLogger _auditLogger;

        public BaglantiController(ApplicationDbContext context, IAuditLogger auditLogger)
        {
            _context = context;
            _auditLogger = auditLogger;
        }

        public async Task<IActionResult> Index()
        {
            var allConnectionsQuery = _context.Baglantilar
                .Select(b => new BaglantiIndexViewModel
                {
                    BaglantiID = b.ID,
                    SourceDeviceID = b.SourceDeviceID,
                    SourceDeviceName = b.SourceDevice.DeviceName,
                    SourcePort = b.Source_Port,
                    TargetDeviceID = b.TargetDeviceID,
                    TargetDeviceName = b.TargetDevice.DeviceName,
                    TargetPort = b.Target_Port,
                    ConnectionType = b.ConnectionType,
                    LinkStatus = b.Source_LinkStatus,
                    LinkSpeed = b.Source_LinkSpeed,
                    NicID = b.Source_NicID,
                    BakirMAC = b.Source_BakirMAC,
                    FiberMAC = b.Source_FiberMAC,
                    WWPN = b.Source_WWPN
                });

            ViewBag.AllConnections = await allConnectionsQuery.ToListAsync();
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
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(
            [Bind("SourceDeviceID,TargetDeviceID,Source_Port,Target_Port,ConnectionType," +
                  "Source_LinkStatus,Source_LinkSpeed,Source_FiberMAC,Source_BakirMAC,Source_WWPN," +
                  "Target_FiberMAC,Target_BakirMAC,Target_WWPN")] Baglanti baglanti)
        {
            // 1. Kural: Kendine Bağlanma Kontrolü
            if (baglanti.SourceDeviceID == baglanti.TargetDeviceID)
            {
                bool isVirtual = (baglanti.ConnectionType ?? "").StartsWith("Virtual", StringComparison.OrdinalIgnoreCase);
                if (!isVirtual)
                {
                    TempData["ErrorMessage"] = "Hata: Fiziksel bağlantılarda cihaz kendisine bağlanamaz (Loop Hatası).";
                    return RedirectToAction("Details", "Envanter", new { id = baglanti.SourceDeviceID });
                }
            }

            // 2. Kural: Port Doluluk Kontrolü
            bool portDoluMu = await _context.Baglantilar.AnyAsync(b =>
                b.SourceDeviceID == baglanti.SourceDeviceID &&
                b.Source_Port == baglanti.Source_Port &&
                b.ID != baglanti.ID);

            if (portDoluMu)
            {
                TempData["ErrorMessage"] = $"Hata: Kaynak cihazın '{baglanti.Source_Port}' portu zaten dolu!";
                return RedirectToAction("Details", "Envanter", new { id = baglanti.SourceDeviceID });
            }

            // Eşitleme
            baglanti.Target_LinkStatus = baglanti.Source_LinkStatus;
            baglanti.Target_LinkSpeed = baglanti.Source_LinkSpeed;
            baglanti.Target_FiberMAC = baglanti.Source_FiberMAC;
            baglanti.Target_BakirMAC = baglanti.Source_BakirMAC;
            baglanti.Target_WWPN = baglanti.Source_WWPN;

            if (ModelState.IsValid)
            {
                _context.Add(baglanti);
                await _context.SaveChangesAsync();

                var sourceDevice = await _context.Envanterler.FindAsync(baglanti.SourceDeviceID);
                var targetDevice = await _context.Envanterler.FindAsync(baglanti.TargetDeviceID);
                await _auditLogger.LogAsync("Ekleme", "Bağlantı", $"Yeni bağlantı: {sourceDevice?.DeviceName}:{baglanti.Source_Port} -> {targetDevice?.DeviceName}:{baglanti.Target_Port}");

                TempData["SuccessMessage"] = "Yeni bağlantı başarıyla oluşturuldu.";
                return RedirectToAction("Details", "Envanter", new { id = baglanti.SourceDeviceID });
            }
            TempData["ErrorMessage"] = "Bağlantı oluşturulurken bir hata oluştu.";
            return RedirectToAction("Details", "Envanter", new { id = baglanti.SourceDeviceID });
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var baglanti = await _context.Baglantilar
                .Include(b => b.SourceDevice)
                .Include(b => b.TargetDevice)
                .FirstOrDefaultAsync(b => b.ID == id);

            if (baglanti == null) return NotFound();
            return View(baglanti);
        }

        // --- GÜNCELLENMİŞ EDİT METODU (YÖNLENDİRME DEĞİŞTİ) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("ID,SourceDeviceID,TargetDeviceID,Source_Port,Target_Port,ConnectionType,Source_LinkStatus,Source_LinkSpeed,Source_NicID,Source_FiberMAC,Source_BakirMAC,Source_WWPN,Target_FiberMAC,Target_BakirMAC,Target_WWPN,RowVersion")] Baglanti baglanti)
        {
            if (id != baglanti.ID) return NotFound();

            // Verileri Eşitle
            baglanti.Target_LinkStatus = baglanti.Source_LinkStatus;
            baglanti.Target_LinkSpeed = baglanti.Source_LinkSpeed;
            baglanti.Target_FiberMAC = baglanti.Source_FiberMAC;
            baglanti.Target_BakirMAC = baglanti.Source_BakirMAC;
            baglanti.Target_WWPN = baglanti.Source_WWPN;

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(baglanti);
                    await _context.SaveChangesAsync();

                    await _auditLogger.LogAsync("Güncelleme", "Bağlantı", $"Bağlantı güncellendi ID: {id}");

                    // --- DEĞİŞİKLİK BURADA ---
                    // SweetAlert yerine standart SuccessMessage kullanıyoruz çünkü sayfa değişecek.
                    // Envanter/Details sayfası bu mesajı otomatik gösterecektir.
                    TempData["SuccessMessage"] = "Bağlantı bilgileri başarıyla güncellendi.";

                    // Kaynak Cihazın detay sayfasına geri dön
                    return RedirectToAction("Details", "Envanter", new { id = baglanti.SourceDeviceID });
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    var entry = ex.Entries.Single();
                    var databaseValues = await entry.GetDatabaseValuesAsync();

                    if (databaseValues == null)
                    {
                        ModelState.AddModelError(string.Empty, "HATA: Bu kayıt silinmiş.");
                    }
                    else
                    {
                        var dbEntity = (Baglanti)databaseValues.ToObject();
                        ModelState.AddModelError(string.Empty, "HATA: Veri sizden önce başkası tarafından değiştirildi.");
                        baglanti.RowVersion = (byte[])dbEntity.RowVersion;
                    }
                }
            }

            // Hata varsa sayfayı tekrar yükle (İsimleri getirerek)
            var reloadedBaglanti = await _context.Baglantilar
                .Include(b => b.SourceDevice)
                .Include(b => b.TargetDevice)
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.ID == id);

            if (reloadedBaglanti != null)
            {
                baglanti.SourceDevice = reloadedBaglanti.SourceDevice;
                baglanti.TargetDevice = reloadedBaglanti.TargetDevice;
            }

            return View(baglanti);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var baglanti = await _context.Baglantilar.Include(b => b.SourceDevice).Include(b => b.TargetDevice).FirstOrDefaultAsync(m => m.ID == id);
            if (baglanti == null) return NotFound();
            return View(baglanti);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var baglanti = await _context.Baglantilar.Include(b => b.SourceDevice).Include(b => b.TargetDevice).FirstOrDefaultAsync(b => b.ID == id);
            if (baglanti != null)
            {
                var logDesc = $"Bağlantı silindi: {baglanti.SourceDevice?.DeviceName}:{baglanti.Source_Port} -> {baglanti.TargetDevice?.DeviceName}:{baglanti.Target_Port}";
                var sourceDeviceId = baglanti.SourceDeviceID;

                _context.Baglantilar.Remove(baglanti);
                await _context.SaveChangesAsync();

                await _auditLogger.LogAsync("Silme", "Bağlantı", logDesc);

                TempData["InfoMessage"] = "Bağlantı/Port başarıyla silindi.";
                return RedirectToAction("Details", "Envanter", new { id = sourceDeviceId });
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
                await _auditLogger.LogAsync("Güvenlik", "Bağlantı", "Geçersiz dosya yükleme girişimi.");
                return RedirectToAction(nameof(Index));
            }

            if (excelFile == null || excelFile.Length == 0) { TempData["ErrorMessage"] = "Lütfen bir dosya seçin."; return RedirectToAction(nameof(Index)); }

            var connectionsToAdd = new List<Baglanti>();
            var errors = new List<string>();
            var allDevices = await _context.Envanterler.AsNoTracking().ToListAsync();
            var extension = Path.GetExtension(excelFile.FileName).ToLower();

            using (var stream = new MemoryStream())
            {
                await excelFile.CopyToAsync(stream);
                stream.Position = 0;

                if (extension == ".xlsx")
                {
                    using (var workbook = new XLWorkbook(stream))
                    {
                        var worksheet = workbook.Worksheets.FirstOrDefault();
                        if (worksheet == null) { TempData["ErrorMessage"] = "Excel sayfası bulunamadı."; return RedirectToAction(nameof(Index)); }

                        var headerRow = worksheet.Row(1);
                        var headerMap = new Dictionary<string, int>();
                        foreach (var cell in headerRow.CellsUsed())
                        {
                            headerMap[cell.Value.ToString().Trim()] = cell.Address.ColumnNumber;
                        }

                        var rows = worksheet.RowsUsed().Skip(1);
                        foreach (var row in rows)
                        {
                            try
                            {
                                var rowData = new Dictionary<string, string>();
                                foreach (var header in headerMap)
                                {
                                    rowData[header.Key] = row.Cell(header.Value).Value.ToString();
                                }
                                ProcessRow(rowData, connectionsToAdd, errors, allDevices, row.RowNumber());
                            }
                            catch (Exception ex) { errors.Add($"Satır {row.RowNumber()}: İşlenirken hata - {ex.Message}"); }
                        }
                    }
                }
                else if (extension == ".csv")
                {
                    using (var reader = new StreamReader(stream))
                    using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                    {
                        csv.Read();
                        csv.ReadHeader();
                        int rowNumber = 2;
                        while (csv.Read())
                        {
                            try
                            {
                                var rowData = new Dictionary<string, string>();
                                if (csv.HeaderRecord != null)
                                {
                                    foreach (var header in csv.HeaderRecord)
                                    {
                                        rowData[header] = csv.GetField(header);
                                    }
                                }
                                ProcessRow(rowData, connectionsToAdd, errors, allDevices, rowNumber);
                            }
                            catch (Exception ex) { errors.Add($"Satır {rowNumber}: İşlenirken hata - {ex.Message}"); }
                            rowNumber++;
                        }
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "Desteklenmeyen format.";
                    return RedirectToAction(nameof(Index));
                }
            }

            int deletedCount = 0;
            if (deleteAll) { deletedCount = await _context.Database.ExecuteSqlRawAsync("DELETE FROM [Baglantilar]"); }
            if (connectionsToAdd.Any()) await _context.Baglantilar.AddRangeAsync(connectionsToAdd);
            await _context.SaveChangesAsync();

            await _auditLogger.LogAsync("Toplu Yükleme", "Bağlantı", $"Excel ile {connectionsToAdd.Count} bağlantı eklendi. (Silinen: {deletedCount})");

            TempData["SuccessMessage"] = $"{(deleteAll ? "Tüm kayıtlar silindi. " : "")}{connectionsToAdd.Count} kayıt işlendi.";
            if (errors.Any())
            {
                const int maxErrorsToShow = 20;
                var totalErrorCount = errors.Count;
                var errorMessages = errors.Take(maxErrorsToShow).ToList();
                if (totalErrorCount > maxErrorsToShow) { errorMessages.Add($"<br><b>... ve toplam {totalErrorCount - maxErrorsToShow} diğer hata daha bulundu.</b>"); }
                TempData["ErrorMessage"] = string.Join("<br>", errorMessages);
            }
            return RedirectToAction(nameof(Index));
        }

        private string SafeTruncate(string? value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return null;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }

        private void ProcessRow(Dictionary<string, string> rowData, List<Baglanti> connectionsToAdd, List<string> errors, List<Envanter> allDevices, int rowNumber)
        {
            string GetValue(string key) => rowData.ContainsKey(key) ? rowData[key].Trim() : string.Empty;

            var sourceIdentifier = GetValue("Device Service Tag");
            if (string.IsNullOrEmpty(sourceIdentifier)) { sourceIdentifier = GetValue("Device Name"); }
            if (string.IsNullOrEmpty(sourceIdentifier)) return;

            var sourceDevice = allDevices.FirstOrDefault(d => (d.ServiceTagSerialNumber != null && d.ServiceTagSerialNumber.Equals(sourceIdentifier, StringComparison.OrdinalIgnoreCase)) || d.DeviceName.Equals(sourceIdentifier, StringComparison.OrdinalIgnoreCase));
            if (sourceDevice == null) { errors.Add($"Satır {rowNumber}: Kaynak cihaz '{sourceIdentifier}' bulunamadı."); return; }

            var newConnection = new Baglanti
            {
                SourceDeviceID = sourceDevice.ID,
                ConnectionType = SafeTruncate(GetValue("Türü"), 50),
                Source_LinkStatus = SafeTruncate(GetValue("Link Status"), 20),
                Source_LinkSpeed = SafeTruncate(GetValue("Link Speed"), 20),
                Source_Port = SafeTruncate(GetValue("Port ID"), 50),
                Source_NicID = SafeTruncate(GetValue("NIC ID"), 50),
                Source_FiberMAC = SafeTruncate(GetValue("Fiber MAC"), 30),
                Source_BakirMAC = SafeTruncate(GetValue("Bakır MAC"), 30),
                Source_WWPN = SafeTruncate(GetValue("WWPN"), 60)
            };

            var targetIdentifier = GetValue("SW NAME");
            if (!string.IsNullOrEmpty(targetIdentifier))
            {
                var targetDevice = allDevices.FirstOrDefault(d => (d.ServiceTagSerialNumber != null && d.ServiceTagSerialNumber.Equals(targetIdentifier, StringComparison.OrdinalIgnoreCase)) || d.DeviceName.Equals(targetIdentifier, StringComparison.OrdinalIgnoreCase));
                if (targetDevice == null) { errors.Add($"Satır {rowNumber}: Hedef cihaz '{targetIdentifier}' bulunamadı."); return; }
                newConnection.TargetDeviceID = targetDevice.ID;
                newConnection.Target_Port = SafeTruncate(GetValue("SW PORT"), 50);
            }
            else { newConnection.Target_Port = string.Empty; }

            newConnection.Target_LinkStatus = newConnection.Source_LinkStatus;
            newConnection.Target_LinkSpeed = newConnection.Source_LinkSpeed;
            newConnection.Target_FiberMAC = newConnection.Source_FiberMAC;
            newConnection.Target_BakirMAC = newConnection.Source_BakirMAC;
            newConnection.Target_WWPN = newConnection.Source_WWPN;

            connectionsToAdd.Add(newConnection);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ImportVirtualPathFromExcel(IFormFile excelFile, bool deleteVirtuals)
        {
            if (!FileSecurityHelper.IsValidFile(excelFile))
            {
                TempData["ErrorMessage"] = "Güvenlik Uyarısı: Dosya geçersiz.";
                await _auditLogger.LogAsync("Güvenlik", "Bağlantı", "Geçersiz dosya yükleme (Sanal Yol).");
                return RedirectToAction(nameof(Index));
            }

            if (excelFile == null || excelFile.Length == 0) { TempData["ErrorMessage"] = "Lütfen bir Excel dosyası seçin."; return RedirectToAction(nameof(Index)); }
            if (deleteVirtuals)
            {
                var virtualsToDelete = _context.Baglantilar.Where(b => b.ConnectionType.StartsWith("Virtual"));
                if (await virtualsToDelete.AnyAsync()) { _context.Baglantilar.RemoveRange(virtualsToDelete); }
            }
            var connectionsToAdd = new List<Baglanti>();
            var errorList = new List<string>();
            var allDevices = await _context.Envanterler.AsNoTracking().ToListAsync();
            var allPhysicalConnections = await _context.Baglantilar.Where(b => !(b.ConnectionType ?? "").StartsWith("Virtual")).AsNoTracking().ToListAsync();

            using (var stream = new MemoryStream())
            {
                await excelFile.CopyToAsync(stream);
                using (var workbook = new XLWorkbook(stream))
                {
                    var worksheet = workbook.Worksheets.FirstOrDefault();
                    if (worksheet == null) { TempData["ErrorMessage"] = "Excel sayfası bulunamadı."; return RedirectToAction(nameof(Index)); }
                    var rows = worksheet.RowsUsed().Skip(1);
                    foreach (var row in rows)
                    {
                        try
                        {
                            var connectionType = row.Cell("B").Value.ToString().Trim();
                            if (!connectionType.StartsWith("Virtual", StringComparison.OrdinalIgnoreCase)) continue;

                            var sourceIdentifier = row.Cell("D").Value.ToString().Trim();
                            if (string.IsNullOrEmpty(sourceIdentifier)) { sourceIdentifier = row.Cell("C").Value.ToString().Trim(); }
                            if (string.IsNullOrEmpty(sourceIdentifier)) continue;

                            var sourceDevice = allDevices.FirstOrDefault(d => (d.ServiceTagSerialNumber != null && d.ServiceTagSerialNumber.Equals(sourceIdentifier, StringComparison.OrdinalIgnoreCase)) || d.DeviceName.Equals(sourceIdentifier, StringComparison.OrdinalIgnoreCase));
                            if (sourceDevice == null) { errorList.Add($"Satır {row.RowNumber()}: Başlangıç cihazı '{sourceIdentifier}' bulunamadı."); continue; }

                            var sourcePort = SafeTruncate(row.Cell("I").Value.ToString().Trim(), 50);
                            var swNameRaw = row.Cell("N").Value.ToString().Trim();
                            if (string.IsNullOrEmpty(swNameRaw)) continue;

                            var physicalPathReferences = swNameRaw.Split(',').Select(p => p.Trim()).ToList();
                            foreach (var pathRef in physicalPathReferences)
                            {
                                var pathParts = pathRef.Split(new[] { '_' }, 2);
                                if (pathParts.Length < 2) { errorList.Add($"Satır {row.RowNumber()}: SW NAME formatı hatalı: '{pathRef}'"); continue; }
                                var refIdentifier = pathParts[0];
                                var refPhysicalPort = pathParts[1];
                                var intermediateDevice = allDevices.FirstOrDefault(d => d.DeviceName.Equals(refIdentifier, StringComparison.OrdinalIgnoreCase) || (d.ServiceTagSerialNumber != null && d.ServiceTagSerialNumber.Equals(refIdentifier, StringComparison.OrdinalIgnoreCase)));
                                if (intermediateDevice == null) { errorList.Add($"Satır {row.RowNumber()}: Ara cihaz '{refIdentifier}' bulunamadı."); continue; }
                                var physicalConnection = allPhysicalConnections.FirstOrDefault(c => (c.SourceDeviceID == intermediateDevice.ID && c.Source_Port.Equals(refPhysicalPort, StringComparison.OrdinalIgnoreCase)) || (c.TargetDeviceID == intermediateDevice.ID && c.Target_Port.Equals(refPhysicalPort, StringComparison.OrdinalIgnoreCase)));
                                if (physicalConnection == null) { errorList.Add($"Satır {row.RowNumber()}: Ara cihaz '{intermediateDevice.DeviceName}' port '{refPhysicalPort}' için fiziksel bağlantı bulunamadı."); continue; }
                                var finalDeviceId = physicalConnection.SourceDeviceID == intermediateDevice.ID ? physicalConnection.TargetDeviceID : physicalConnection.SourceDeviceID;
                                var finalPort = physicalConnection.SourceDeviceID == intermediateDevice.ID ? physicalConnection.Target_Port : physicalConnection.Source_Port;

                                var newVirtualConnection = new Baglanti
                                {
                                    SourceDeviceID = sourceDevice.ID,
                                    TargetDeviceID = finalDeviceId,
                                    Source_Port = sourcePort,
                                    Target_Port = finalPort,
                                    ConnectionType = SafeTruncate($"{connectionType} (via {pathRef})", 50),
                                    Source_LinkStatus = SafeTruncate(row.Cell("G").Value.ToString().Trim(), 20),
                                    Source_LinkSpeed = SafeTruncate(row.Cell("H").Value.ToString().Trim(), 20),
                                    Source_NicID = SafeTruncate(row.Cell("J").Value.ToString().Trim(), 50),
                                    Source_FiberMAC = SafeTruncate(row.Cell("K").Value.ToString().Trim(), 30),
                                    Source_BakirMAC = SafeTruncate(row.Cell("L").Value.ToString().Trim(), 30),
                                    Source_WWPN = SafeTruncate(row.Cell("M").Value.ToString(), 60)
                                };

                                newVirtualConnection.Target_LinkStatus = newVirtualConnection.Source_LinkStatus;
                                newVirtualConnection.Target_LinkSpeed = newVirtualConnection.Source_LinkSpeed;
                                newVirtualConnection.Target_FiberMAC = newVirtualConnection.Source_FiberMAC;
                                newVirtualConnection.Target_BakirMAC = newVirtualConnection.Source_BakirMAC;
                                newVirtualConnection.Target_WWPN = newVirtualConnection.Source_WWPN;

                                bool alreadyExists = connectionsToAdd.Any(c => c.SourceDeviceID == newVirtualConnection.SourceDeviceID && c.Source_Port == newVirtualConnection.Source_Port && c.TargetDeviceID == newVirtualConnection.TargetDeviceID && c.Target_Port == newVirtualConnection.Target_Port);
                                if (!alreadyExists) { connectionsToAdd.Add(newVirtualConnection); }
                            }
                        }
                        catch (Exception ex) { errorList.Add($"Satır {row.RowNumber()}: İşlenirken hata - {ex.Message}"); }
                    }
                }
            }
            if (connectionsToAdd.Any()) await _context.Baglantilar.AddRangeAsync(connectionsToAdd);
            await _context.SaveChangesAsync();

            await _auditLogger.LogAsync("Sanal Yol Yükleme", "Bağlantı", $"Excel ile {connectionsToAdd.Count} sanal bağlantı eklendi.");

            var deleteMessage = deleteVirtuals ? "Mevcut sanal bağlantılar silindi. " : "";
            TempData["SuccessMessage"] = $"{deleteMessage}{connectionsToAdd.Count} sanal bağlantı işlendi.";
            if (errorList.Any())
            {
                const int maxErrorsToShow = 20;
                var totalErrorCount = errorList.Count;
                var errorMessages = errorList.Take(maxErrorsToShow).ToList();
                if (totalErrorCount > maxErrorsToShow) { errorMessages.Add($"<br><b>... ve toplam {totalErrorCount - maxErrorsToShow} diğer hata daha bulundu.</b>"); }
                TempData["ErrorMessage"] = string.Join("<br>", errorList);
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> ExportToExcel()
        {
            var baglantilar = await _context.Baglantilar
                .Include(b => b.SourceDevice)
                .Include(b => b.TargetDevice)
                .AsNoTracking()
                .ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Baglanti Listesi");
                var currentRow = 1;

                string[] headers = { "SN", "Türü", "Device Name", "Device Service Tag", "Device Model", "Lok", "Link Status", "Link Speed", "Port ID", "NIC ID", "Fiber MAC", "Bakır MAC", "WWPN", "SW NAME", "SW PORT" };
                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cell(currentRow, i + 1).Value = headers[i];
                }
                worksheet.Row(1).Style.Font.Bold = true;

                foreach (var b in baglantilar)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 2).Value = b.ConnectionType;
                    worksheet.Cell(currentRow, 3).Value = b.SourceDevice?.DeviceName;
                    worksheet.Cell(currentRow, 4).Value = b.SourceDevice?.ServiceTagSerialNumber;
                    worksheet.Cell(currentRow, 5).Value = b.SourceDevice?.Model;
                    worksheet.Cell(currentRow, 7).Value = b.Source_LinkStatus;
                    worksheet.Cell(currentRow, 8).Value = b.Source_LinkSpeed;
                    worksheet.Cell(currentRow, 9).Value = b.Source_Port;
                    worksheet.Cell(currentRow, 10).Value = b.Source_NicID;
                    worksheet.Cell(currentRow, 11).Value = b.Source_FiberMAC;
                    worksheet.Cell(currentRow, 12).Value = b.Source_BakirMAC;
                    worksheet.Cell(currentRow, 13).Value = b.Source_WWPN;
                    worksheet.Cell(currentRow, 14).Value = b.TargetDevice?.DeviceName;
                    worksheet.Cell(currentRow, 15).Value = b.Target_Port;
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"BaglantiListesi_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
                }
            }
        }

        public async Task<IActionResult> ExportToCsv()
        {
            var baglantilar = await _context.Baglantilar
                .Include(b => b.SourceDevice)
                .Include(b => b.TargetDevice)
                .AsNoTracking()
                .Select(b => new
                {
                    Türü = b.ConnectionType,
                    DeviceName = b.SourceDevice.DeviceName,
                    DeviceServiceTag = b.SourceDevice.ServiceTagSerialNumber,
                    DeviceModel = b.SourceDevice.Model,
                    LinkStatus = b.Source_LinkStatus,
                    LinkSpeed = b.Source_LinkSpeed,
                    PortID = b.Source_Port,
                    NicID = b.Source_NicID,
                    FiberMAC = b.Source_FiberMAC,
                    BakirMAC = b.Source_BakirMAC,
                    WWPN = b.Source_WWPN,
                    SW_NAME = b.TargetDevice.DeviceName,
                    SW_PORT = b.Target_Port
                })
                .ToListAsync();

            using (var memoryStream = new MemoryStream())
            using (var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8))
            using (var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture))
            {
                csvWriter.WriteRecords(baglantilar);
                streamWriter.Flush();
                var content = memoryStream.ToArray();
                return File(content, "text/csv", $"BaglantiListesi_{DateTime.Now:yyyyMMddHHmmss}.csv");
            }
        }
    }
}