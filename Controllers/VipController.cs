using BLBM_ENV.Data;
using BLBM_ENV.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace BLBM_ENV.Controllers
{
    public class HostResolveResult { public bool Success { get; set; } public string IpAddress { get; set; } }
    public class VipDetailDto { public string? MakineAdi { get; set; } public string? Durumu { get; set; } public string? Network { get; set; } public string? Cluster { get; set; } public string? Host { get; set; } public string? OS { get; set; } }

    public class VipController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VipController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var vips = await _context.Vips.ToListAsync();
            return View(vips);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportStandardExcel(IFormFile excelFile, bool deleteAll)
        {
            if (excelFile == null || excelFile.Length == 0) { TempData["ErrorMessage"] = "Lütfen bir Excel dosyası seçin."; return RedirectToAction(nameof(Index)); }
            if (deleteAll) { await _context.Database.ExecuteSqlRawAsync("DELETE FROM [Vips]"); }
            var vipsToAdd = new List<Vip>();
            using (var stream = new MemoryStream())
            {
                await excelFile.CopyToAsync(stream);
                using (var workbook = new XLWorkbook(stream))
                {
                    var worksheet = workbook.Worksheets.FirstOrDefault();
                    if (worksheet == null) { TempData["ErrorMessage"] = "Excel'de sayfa bulunamadı."; return RedirectToAction(nameof(Index)); }
                    var rows = worksheet.RowsUsed().Skip(1);
                    foreach (var row in rows)
                    {
                        try
                        {
                            vipsToAdd.Add(new Vip { Dns = row.Cell(1).Value.ToString().Trim(), VipIp = row.Cell(2).Value.ToString().Trim(), Port = row.Cell(3).Value.ToString().Trim(), MakineIp = row.Cell(4).Value.ToString().Trim(), MakineAdi = row.Cell(5).Value.ToString().Trim(), Durumu = row.Cell(6).Value.ToString().Trim(), Network = row.Cell(7).Value.ToString().Trim(), Cluster = row.Cell(8).Value.ToString().Trim(), Host = row.Cell(9).Value.ToString().Trim(), OS = row.Cell(10).Value.ToString().Trim() });
                        }
                        catch (Exception ex) { TempData["ErrorMessage"] = $"Hata: {row.RowNumber()}. satır okunamadı. Detay: {ex.Message}"; return RedirectToAction(nameof(Index)); }
                    }
                }
            }
            if (vipsToAdd.Any()) { await _context.Vips.AddRangeAsync(vipsToAdd); await _context.SaveChangesAsync(); TempData["SuccessMessage"] = $"{vipsToAdd.Count} adet VIP kaydı başarıyla yüklendi."; }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportAndResolveFromExcel(IFormFile excelFile, bool deleteAll)
        {
            if (excelFile == null || excelFile.Length == 0) { TempData["ErrorMessage"] = "Lütfen bir Excel dosyası seçin."; return RedirectToAction(nameof(Index)); }
            if (deleteAll) { await _context.Database.ExecuteSqlRawAsync("DELETE FROM [Vips]"); }
            var vipsToAdd = new List<Vip>();
            var errorList = new List<string>();
            using (var stream = new MemoryStream())
            {
                await excelFile.CopyToAsync(stream);
                using (var workbook = new XLWorkbook(stream))
                {
                    var worksheet = workbook.Worksheets.FirstOrDefault();
                    if (worksheet == null) { TempData["ErrorMessage"] = "Excel'de sayfa bulunamadı."; return RedirectToAction(nameof(Index)); }
                    var rows = worksheet.RowsUsed().Skip(1);
                    foreach (var row in rows)
                    {
                        try
                        {
                            string rawData = row.Cell(1).Value.ToString().Trim();
                            if (string.IsNullOrEmpty(rawData)) continue;
                            string[] mainParts = rawData.Split(new[] { ' ', '\t' }, 2, StringSplitOptions.RemoveEmptyEntries);
                            if (mainParts.Length < 2) { errorList.Add($"Satır {row.RowNumber()}: '{rawData}' formatı hatalı."); continue; }
                            string dnsPart = mainParts[0];
                            string machinePart = mainParts[1];
                            string dnsName = dnsPart;
                            string port = null;
                            if (dnsName.StartsWith("pool_")) { dnsName = dnsName.Substring(5); }
                            int lastUnderscore = dnsName.LastIndexOf('_');
                            if (lastUnderscore > 0 && int.TryParse(dnsName.Substring(lastUnderscore + 1), out _))
                            {
                                port = dnsName.Substring(lastUnderscore + 1);
                                dnsName = dnsName.Substring(0, lastUnderscore);
                            }
                            string machineIp = machinePart;
                            if (machinePart.Contains(':')) { machineIp = machinePart.Split(':')[0]; }
                            if (machineIp.StartsWith("DRC_")) { machineIp = machineIp.Substring(4); }
                            HostResolveResult result = await ResolveHostAsync(dnsName);
                            vipsToAdd.Add(new Vip { Dns = dnsName, VipIp = result.IpAddress, Port = port, MakineIp = machineIp, Durumu = result.Success ? "Up" : "Down", MakineAdi = null, Network = null, Cluster = null, Host = null, OS = null });
                        }
                        catch (Exception ex) { errorList.Add($"Satır {row.RowNumber()}: İşlenirken hata - {ex.Message}"); }
                    }
                }
            }
            if (vipsToAdd.Any()) { await _context.Vips.AddRangeAsync(vipsToAdd); await _context.SaveChangesAsync(); TempData["SuccessMessage"] = $"{vipsToAdd.Count} adet VIP kaydı Oto-DNS ile işlendi."; }
            if (errorList.Any()) { TempData["ErrorMessage"] = string.Join("<br/>", errorList); }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateVipsFromDetailExcel(IFormFile excelFile)
        {
            if (excelFile == null || excelFile.Length == 0) { TempData["ErrorMessage"] = "Lütfen bir detay Excel dosyası seçin."; return RedirectToAction(nameof(Index)); }
            var excelData = new Dictionary<string, VipDetailDto>();
            using (var stream = new MemoryStream())
            {
                await excelFile.CopyToAsync(stream);
                using (var workbook = new XLWorkbook(stream))
                {
                    var worksheet = workbook.Worksheets.FirstOrDefault();
                    if (worksheet == null) { TempData["ErrorMessage"] = "Detay Excel'de sayfa bulunamadı."; return RedirectToAction(nameof(Index)); }
                    var headerRow = worksheet.Row(1);
                    var headerMap = new Dictionary<string, int>();
                    foreach (var cell in headerRow.CellsUsed()) { headerMap[cell.Value.ToString().Trim()] = cell.Address.ColumnNumber; }
                    if (!headerMap.ContainsKey("Primary IP Address")) { TempData["ErrorMessage"] = "Excel'de 'Primary IP Address' sütunu bulunamadı."; return RedirectToAction(nameof(Index)); }
                    var rows = worksheet.RowsUsed().Skip(1);
                    foreach (var row in rows)
                    {
                        string makineIp = row.Cell(headerMap["Primary IP Address"]).Value.ToString().Trim();
                        if (!string.IsNullOrEmpty(makineIp) && !excelData.ContainsKey(makineIp))
                        {
                            excelData.Add(makineIp, new VipDetailDto { MakineAdi = headerMap.ContainsKey("VM") ? row.Cell(headerMap["VM"]).Value.ToString().Trim() : null, Durumu = headerMap.ContainsKey("Powerstate") ? row.Cell(headerMap["Powerstate"]).Value.ToString().Trim() : null, Network = headerMap.ContainsKey("Network #1") ? row.Cell(headerMap["Network #1"]).Value.ToString().Trim() : null, Cluster = headerMap.ContainsKey("Cluster") ? row.Cell(headerMap["Cluster"]).Value.ToString().Trim() : null, Host = headerMap.ContainsKey("Host") ? row.Cell(headerMap["Host"]).Value.ToString().Trim() : null, OS = headerMap.ContainsKey("OS according to the configuration file") ? row.Cell(headerMap["OS according to the configuration file"]).Value.ToString().Trim() : null });
                        }
                    }
                }
            }
            var vipsInDb = await _context.Vips.ToListAsync();
            int updatedCount = 0;
            foreach (var vip in vipsInDb)
            {
                if (!string.IsNullOrEmpty(vip.MakineIp) && excelData.TryGetValue(vip.MakineIp, out var excelDetail))
                {
                    vip.MakineAdi = excelDetail.MakineAdi ?? vip.MakineAdi; vip.Durumu = excelDetail.Durumu ?? vip.Durumu; vip.Network = excelDetail.Network ?? vip.Network; vip.Cluster = excelDetail.Cluster ?? vip.Cluster; vip.Host = excelDetail.Host ?? vip.Host; vip.OS = excelDetail.OS ?? vip.OS; updatedCount++;
                }
            }
            if (updatedCount > 0) { await _context.SaveChangesAsync(); TempData["SuccessMessage"] = $"{updatedCount} adet VIP kaydı güncellendi."; }
            else { TempData["InfoMessage"] = "Eşleşen kayıt bulunamadı."; }
            return RedirectToAction(nameof(Index));
        }

        private async Task<HostResolveResult> ResolveHostAsync(string hostName)
        {
            try
            {
                var process = new Process { StartInfo = new ProcessStartInfo { FileName = "nslookup.exe", Arguments = hostName, RedirectStandardOutput = true, UseShellExecute = false, CreateNoWindow = true, } };
                process.Start();
                string output = await process.StandardOutput.ReadToEndAsync();
                await process.WaitForExitAsync();
                var matches = Regex.Matches(output, @"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b");
                string ip = "Çözümlenemedi";
                bool success = false;
                if (matches.Count > 1) { ip = matches.Last().Value; success = true; }
                else if (matches.Count == 1) { ip = matches.First().Value; success = true; }
                return new HostResolveResult { Success = success, IpAddress = ip };
            }
            catch { return new HostResolveResult { Success = false, IpAddress = "DNS Hatası" }; }
        }
    }
}