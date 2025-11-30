using BLBM_ENV.Data;
using BLBM_ENV.Models;
using BLBM_ENV.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;
using CsvHelper;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Collections.Concurrent;
using System.Text;
using System.Diagnostics;
using System.Text.RegularExpressions; // --- DÜZELTME: Bu kütüphane eklendi ---

namespace BLBM_ENV.Controllers
{
    [Authorize]
    public class HostResolveResult
    {
        public bool Success { get; set; }
        public string? IpAddress { get; set; }
    }

    public class VipDetailDto
    {
        public string? MakineAdi { get; set; }
        public string? Durumu { get; set; }
        public string? Network { get; set; }
        public string? Cluster { get; set; }
        public string? Host { get; set; }
        public string? OS { get; set; }
    }

    public class RawVipData
    {
        public int RowNumber { get; set; }
        public string RawInput { get; set; } = string.Empty;
    }

    [Authorize]
    public class VipController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditLogger _auditLogger;

        // --- AYAR: Şirket Domain Uzantıları ---
        private readonly string[] _domainSuffixes = { ".belbim.local", ".local", ".belbim.com.tr" };

        public VipController(ApplicationDbContext context, IAuditLogger auditLogger)
        {
            _context = context;
            _auditLogger = auditLogger;
        }

        public async Task<IActionResult> Index()
        {
            var vips = await _context.Vips.ToListAsync();
            return View(vips);
        }

        public async Task<IActionResult> ExportToExcel()
        {
            var vips = await _context.Vips.AsNoTracking().ToListAsync();
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("VIP Listesi");
                var currentRow = 1;

                worksheet.Cell(currentRow, 1).Value = "Dns";
                worksheet.Cell(currentRow, 2).Value = "VipIp";
                worksheet.Cell(currentRow, 3).Value = "Port";
                worksheet.Cell(currentRow, 4).Value = "MakineIp";
                worksheet.Cell(currentRow, 5).Value = "MakineAdi";
                worksheet.Cell(currentRow, 6).Value = "Durumu";
                worksheet.Cell(currentRow, 7).Value = "Network";
                worksheet.Cell(currentRow, 8).Value = "Cluster";
                worksheet.Cell(currentRow, 9).Value = "Host";
                worksheet.Cell(currentRow, 10).Value = "OS";
                worksheet.Row(1).Style.Font.Bold = true;

                foreach (var vip in vips)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = vip.Dns;
                    worksheet.Cell(currentRow, 2).Value = vip.VipIp;
                    worksheet.Cell(currentRow, 3).Value = vip.Port;
                    worksheet.Cell(currentRow, 4).Value = vip.MakineIp;
                    worksheet.Cell(currentRow, 5).Value = vip.MakineAdi;
                    worksheet.Cell(currentRow, 6).Value = vip.Durumu;
                    worksheet.Cell(currentRow, 7).Value = vip.Network;
                    worksheet.Cell(currentRow, 8).Value = vip.Cluster;
                    worksheet.Cell(currentRow, 9).Value = vip.Host;
                    worksheet.Cell(currentRow, 10).Value = vip.OS;
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"VIP_Listesi_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
                }
            }
        }

        public async Task<IActionResult> ExportToCsv()
        {
            var vips = await _context.Vips.AsNoTracking().ToListAsync();
            using (var memoryStream = new MemoryStream())
            using (var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8))
            using (var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture))
            {
                csvWriter.WriteRecords(vips);
                streamWriter.Flush();
                var content = memoryStream.ToArray();
                return File(content, "text/csv", $"VIP_Listesi_{DateTime.Now:yyyyMMddHHmmss}.csv");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ImportStandardExcel(IFormFile excelFile, bool deleteAll)
        {
            if (!FileSecurityHelper.IsValidFile(excelFile))
            {
                TempData["ErrorMessage"] = "Güvenlik Uyarısı: Dosya geçersiz.";
                await _auditLogger.LogAsync("Güvenlik", "VIP", "Geçersiz dosya yükleme girişimi.");
                return RedirectToAction(nameof(Index));
            }

            if (excelFile == null || excelFile.Length == 0) { TempData["ErrorMessage"] = "Lütfen bir dosya seçin."; return RedirectToAction(nameof(Index)); }

            int deletedCount = 0;
            if (deleteAll)
            {
                deletedCount = await _context.Database.ExecuteSqlRawAsync("DELETE FROM [Vips]");
            }

            var vipsToAdd = new List<Vip>();
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
                        if (worksheet == null) { TempData["ErrorMessage"] = "Excel'de sayfa bulunamadı."; return RedirectToAction(nameof(Index)); }
                        var rows = worksheet.RowsUsed().Skip(1);
                        foreach (var row in rows)
                        {
                            try { vipsToAdd.Add(new Vip { Dns = row.Cell(1).Value.ToString().Trim(), VipIp = row.Cell(2).Value.ToString().Trim(), Port = row.Cell(3).Value.ToString().Trim(), MakineIp = row.Cell(4).Value.ToString().Trim(), MakineAdi = row.Cell(5).Value.ToString().Trim(), Durumu = row.Cell(6).Value.ToString().Trim(), Network = row.Cell(7).Value.ToString().Trim(), Cluster = row.Cell(8).Value.ToString().Trim(), Host = row.Cell(9).Value.ToString().Trim(), OS = row.Cell(10).Value.ToString().Trim() }); }
                            catch (Exception ex) { TempData["ErrorMessage"] = $"Hata: {row.RowNumber()}. satır okunamadı. Detay: {ex.Message}"; return RedirectToAction(nameof(Index)); }
                        }
                    }
                }
                else if (extension == ".csv")
                {
                    using (var reader = new StreamReader(stream))
                    using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                    {
                        var records = csv.GetRecords<Vip>().ToList();
                        vipsToAdd.AddRange(records);
                    }
                }
            }
            if (vipsToAdd.Any())
            {
                await _context.Vips.AddRangeAsync(vipsToAdd);
                await _context.SaveChangesAsync();
                await _auditLogger.LogAsync("Toplu Yükleme", "VIP", $"Standart Excel ile {vipsToAdd.Count} VIP eklendi. (Silinen: {deletedCount})");
                TempData["SuccessMessage"] = $"{vipsToAdd.Count} adet VIP kaydı başarıyla yüklendi.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ImportAndResolveFromExcel(IFormFile excelFile, bool deleteAll)
        {
            if (!FileSecurityHelper.IsValidFile(excelFile))
            {
                TempData["ErrorMessage"] = "Güvenlik Uyarısı: Dosya geçersiz.";
                return RedirectToAction(nameof(Index));
            }

            var rawDataList = new List<RawVipData>();
            using (var stream = new MemoryStream())
            {
                await excelFile.CopyToAsync(stream);
                using (var workbook = new XLWorkbook(stream))
                {
                    var worksheet = workbook.Worksheets.FirstOrDefault();
                    if (worksheet == null) { TempData["ErrorMessage"] = "Sayfa bulunamadı."; return RedirectToAction(nameof(Index)); }

                    var rows = worksheet.RowsUsed().Skip(1);
                    foreach (var row in rows)
                    {
                        rawDataList.Add(new RawVipData
                        {
                            RowNumber = row.RowNumber(),
                            RawInput = row.Cell(1).Value.ToString().Trim()
                        });
                    }
                }
            }

            if (deleteAll)
            {
                await _context.Database.ExecuteSqlRawAsync("DELETE FROM [Vips]");
            }

            var vipsToAdd = new ConcurrentBag<Vip>();
            var errorList = new ConcurrentBag<string>();
            var semaphore = new SemaphoreSlim(20);

            var tasks = rawDataList.Select(async item =>
            {
                await semaphore.WaitAsync();
                try
                {
                    if (string.IsNullOrEmpty(item.RawInput)) return;

                    string[] mainParts = item.RawInput.Split(new[] { ' ', '\t' }, 2, StringSplitOptions.RemoveEmptyEntries);
                    if (mainParts.Length < 2)
                    {
                        errorList.Add($"Satır {item.RowNumber}: Format hatalı.");
                        return;
                    }

                    string dnsName = mainParts[0];
                    string machinePart = mainParts[1];
                    string? port = null;

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

                    // Akıllı Çözümleme
                    HostResolveResult result = await ResolveHostSmartAsync(dnsName);

                    vipsToAdd.Add(new Vip
                    {
                        Dns = dnsName,
                        VipIp = result.IpAddress,
                        Port = port,
                        MakineIp = machineIp,
                        Durumu = result.Success ? "Up" : "Down"
                    });
                }
                catch (Exception ex)
                {
                    errorList.Add($"Satır {item.RowNumber}: Hata - {ex.Message}");
                }
                finally
                {
                    semaphore.Release();
                }
            });

            await Task.WhenAll(tasks);

            if (!vipsToAdd.IsEmpty)
            {
                await _context.Vips.AddRangeAsync(vipsToAdd);
                await _context.SaveChangesAsync();
                await _auditLogger.LogAsync("DNS Çözümleme", "VIP", $"Akıllı DNS Resolver ile {vipsToAdd.Count} VIP eklendi.");
                TempData["SuccessMessage"] = $"{vipsToAdd.Count} adet VIP kaydı işlendi.";
            }

            if (!errorList.IsEmpty)
            {
                TempData["ErrorMessage"] = string.Join("<br/>", errorList.Take(20));
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateVipsFromDetailExcel(IFormFile excelFile)
        {
            if (excelFile == null || excelFile.Length == 0) { TempData["ErrorMessage"] = "Lütfen dosya seçin."; return RedirectToAction(nameof(Index)); }

            var excelData = new Dictionary<string, VipDetailDto>();
            using (var stream = new MemoryStream())
            {
                await excelFile.CopyToAsync(stream);
                using (var workbook = new XLWorkbook(stream))
                {
                    var worksheet = workbook.Worksheets.FirstOrDefault();
                    if (worksheet == null) { TempData["ErrorMessage"] = "Sayfa bulunamadı."; return RedirectToAction(nameof(Index)); }
                    var headerRow = worksheet.Row(1);
                    var headerMap = new Dictionary<string, int>();
                    foreach (var cell in headerRow.CellsUsed()) { headerMap[cell.Value.ToString().Trim()] = cell.Address.ColumnNumber; }
                    if (!headerMap.ContainsKey("Primary IP Address")) { TempData["ErrorMessage"] = "'Primary IP Address' sütunu bulunamadı."; return RedirectToAction(nameof(Index)); }
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
            int unmatchedCount = 0;

            foreach (var vip in vipsInDb)
            {
                if (!string.IsNullOrEmpty(vip.MakineIp) && excelData.TryGetValue(vip.MakineIp, out var excelDetail))
                {
                    vip.MakineAdi = excelDetail.MakineAdi ?? vip.MakineAdi;
                    vip.Durumu = excelDetail.Durumu ?? vip.Durumu;
                    vip.Network = excelDetail.Network ?? vip.Network;
                    vip.Cluster = excelDetail.Cluster ?? vip.Cluster;
                    vip.Host = excelDetail.Host ?? vip.Host;
                    vip.OS = excelDetail.OS ?? vip.OS;
                    updatedCount++;
                }
                else if (vip.Durumu == "Up" || vip.Durumu == "Down")
                {
                    vip.Durumu = "-";
                    unmatchedCount++;
                }
            }

            if (updatedCount > 0 || unmatchedCount > 0)
            {
                await _context.SaveChangesAsync();
                await _auditLogger.LogAsync("Zenginleştirme", "VIP", $"{updatedCount} VIP güncellendi.");
                TempData["SuccessMessage"] = $"{updatedCount} VIP güncellendi.";
            }
            else
            {
                TempData["InfoMessage"] = "Eşleşen kayıt bulunamadı.";
            }
            return RedirectToAction(nameof(Index));
        }

        // --- AKILLI ÇÖZÜMLEME ---
        private async Task<HostResolveResult> ResolveHostSmartAsync(string hostName)
        {
            var result = await TryResolveNative(hostName);
            if (result.Success) return result;

            if (!hostName.Contains('.'))
            {
                foreach (var suffix in _domainSuffixes)
                {
                    var suffixResult = await TryResolveNative(hostName + suffix);
                    if (suffixResult.Success) return suffixResult;
                }
            }

            return await TryResolveNslookup(hostName);
        }

        private async Task<HostResolveResult> TryResolveNative(string host)
        {
            try
            {
                var entry = await Dns.GetHostEntryAsync(host);
                var ipv4 = entry.AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
                if (ipv4 != null) return new HostResolveResult { Success = true, IpAddress = ipv4.ToString() };

                var anyIp = entry.AddressList.FirstOrDefault();
                return new HostResolveResult { Success = (anyIp != null), IpAddress = anyIp?.ToString() ?? "Çözümlenemedi" };
            }
            catch
            {
                return new HostResolveResult { Success = false, IpAddress = null };
            }
        }

        private async Task<HostResolveResult> TryResolveNslookup(string hostName)
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "nslookup.exe",
                        Arguments = hostName,
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                process.Start();
                string output = await process.StandardOutput.ReadToEndAsync();
                await process.WaitForExitAsync();

                var matches = Regex.Matches(output, @"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b");

                if (matches.Count > 1)
                {
                    return new HostResolveResult { Success = true, IpAddress = matches.LastOrDefault()?.Value };
                }
                else if (matches.Count == 1)
                {
                    return new HostResolveResult { Success = true, IpAddress = matches.FirstOrDefault()?.Value };
                }

                return new HostResolveResult { Success = false, IpAddress = "Çözümlenemedi" };
            }
            catch
            {
                return new HostResolveResult { Success = false, IpAddress = "DNS Hatası" };
            }
        }
    }
}