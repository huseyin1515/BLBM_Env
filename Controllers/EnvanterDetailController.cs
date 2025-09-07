using BLBM_ENV.Data;
using BLBM_ENV.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;
using System.Text;

namespace BLBM_ENV.Controllers
{
    public class EnvanterDetailController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EnvanterDetailController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportVirtualFromExcel(IFormFile excelFile)
        {
            if (excelFile == null || excelFile.Length == 0)
            {
                TempData["ErrorMessage"] = "Lütfen bir Excel dosyası seçin.";
                return RedirectToAction("Index", "Envanter");
            }

            var detailsToAdd = new List<EnvanterDetail>();
            var errors = new StringBuilder();
            var successCount = 0;
            var allDevices = await _context.Envanterler.ToListAsync();

            using (var stream = new MemoryStream())
            {
                await excelFile.CopyToAsync(stream);
                using (var workbook = new XLWorkbook(stream))
                {
                    var worksheet = workbook.Worksheets.First();
                    var rows = worksheet.RowsUsed().Skip(1);

                    foreach (var row in rows)
                    {
                        var deviceName = row.Cell("C").Value.ToString().Trim();
                        var serviceTag = row.Cell("D").Value.ToString().Trim();
                        var connectionType = row.Cell("B").Value.ToString().Trim();

                        if (!connectionType.StartsWith("Virtual", StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        Envanter device = null;
                        if (!string.IsNullOrEmpty(deviceName))
                        {
                            device = allDevices.FirstOrDefault(d => d.DeviceName.Equals(deviceName, StringComparison.OrdinalIgnoreCase));
                        }
                        if (device == null && !string.IsNullOrEmpty(serviceTag))
                        {
                            device = allDevices.FirstOrDefault(d => d.ServiceTagSerialNumber != null && d.ServiceTagSerialNumber.Equals(serviceTag, StringComparison.OrdinalIgnoreCase));
                        }

                        if (device == null)
                        {
                            errors.AppendLine($"Satır {row.RowNumber()}: Cihaz '{deviceName}' veya '{serviceTag}' ile bulunamadı.");
                            continue;
                        }

                        var detail = new EnvanterDetail
                        {
                            EnvanterID = device.ID,
                            Turu = connectionType,
                            DeviceName = device.DeviceName,
                            DeviceServiceTag = device.ServiceTagSerialNumber,
                            DeviceModel = device.Model,
                            Lok = row.Cell("F").Value.ToString().Trim(),
                            LinkStatus = row.Cell("G").Value.ToString().Trim(),
                            LinkSpeed = row.Cell("H").Value.ToString().Trim(),
                            PortID = row.Cell("I").Value.ToString().Trim(),
                            NicID = row.Cell("J").Value.ToString().Trim(),
                            FiberMAC = row.Cell("K").Value.ToString().Trim(),
                            BakirMAC = row.Cell("L").Value.ToString().Trim(),
                            WWPN = row.Cell("M").Value.ToString().Trim(),
                        };
                        detailsToAdd.Add(detail);
                    }
                }
            }

            if (detailsToAdd.Any())
            {
                await _context.EnvanterDetails.AddRangeAsync(detailsToAdd);
                await _context.SaveChangesAsync();
                successCount = detailsToAdd.Count;
            }

            TempData["SuccessMessage"] = $"{successCount} adet sanal port detayı başarıyla eklendi/güncellendi.";
            if (errors.Length > 0)
            {
                TempData["ErrorMessage"] = "Bazı satırlar işlenemedi:<br>" + errors.ToString().Replace(Environment.NewLine, "<br>");
            }

            return RedirectToAction("Index", "Envanter");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddDetail([Bind("EnvanterID,Lok,LinkStatus,LinkSpeed,PortID,NicID,FiberMAC,BakirMAC,WWPN,SwName,SwPort")] EnvanterDetail envanterDetail)
        {
            if (ModelState.IsValid)
            {
                var sourceDevice = await _context.Envanterler.AsNoTracking().FirstOrDefaultAsync(e => e.ID == envanterDetail.EnvanterID);
                var targetSwitch = await _context.Envanterler.AsNoTracking().FirstOrDefaultAsync(e => e.DeviceName == envanterDetail.SwName);

                if (sourceDevice == null || targetSwitch == null)
                {
                    TempData["ErrorMessage"] = "Kaynak veya hedef cihaz bulunamadı!";
                    return RedirectToAction("Index", "Envanter");
                }

                envanterDetail.DeviceName = sourceDevice.DeviceName;
                envanterDetail.DeviceModel = sourceDevice.Model;
                envanterDetail.DeviceServiceTag = sourceDevice.ServiceTagSerialNumber;
                envanterDetail.Turu = sourceDevice.Tur;

                envanterDetail.SwModel = targetSwitch.Model;
                envanterDetail.SwServiceTag = targetSwitch.ServiceTagSerialNumber;

                _context.Add(envanterDetail);
                await _context.SaveChangesAsync();

                return RedirectToAction("Details", "Envanter", new { id = envanterDetail.EnvanterID });
            }

            TempData["ErrorMessage"] = "Bağlantı detayı eklenirken bir hata oluştu.";
            return RedirectToAction("Details", "Envanter", new { id = envanterDetail.EnvanterID });
        }
    }
}