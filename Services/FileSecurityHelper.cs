namespace BLBM_ENV.Services
{
    public static class FileSecurityHelper
    {
        // Excel (XLSX) ve CSV dosyalarının Hex imzaları
        private static readonly Dictionary<string, List<byte[]>> _fileSignatures = new Dictionary<string, List<byte[]>>
        {
            { ".xlsx", new List<byte[]>
                {
                    new byte[] { 0x50, 0x4B, 0x03, 0x04 },
                    new byte[] { 0x50, 0x4B, 0x05, 0x06 },
                    new byte[] { 0x50, 0x4B, 0x07, 0x08 }
                }
            },
            // CSV'nin kesin bir imzası yoktur ama metin olduğunu kontrol edebiliriz.
            // CSV için daha çok uzantı ve içerik kontrolü yapılır.
        };

        public static bool IsValidFile(IFormFile file)
        {
            // 1. Dosya boş mu?
            if (file == null || file.Length == 0) return false;

            // 2. Boyut Kontrolü (Örn: Max 5 MB)
            if (file.Length > 5 * 1024 * 1024) return false;

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();

            // 3. Uzantı Kontrolü
            if (string.IsNullOrEmpty(ext) || (ext != ".xlsx" && ext != ".csv")) return false;

            // 4. İmza (Magic Number) Kontrolü - Sadece XLSX için
            if (ext == ".xlsx")
            {
                using (var reader = new BinaryReader(file.OpenReadStream()))
                {
                    var headerBytes = reader.ReadBytes(4); // İlk 4 byte'ı oku

                    // Dosyanın başlığı, izin verilen imzalardan biriyle eşleşiyor mu?
                    var signatures = _fileSignatures[ext];
                    bool isMatch = signatures.Any(signature =>
                        headerBytes.Take(signature.Length).SequenceEqual(signature));

                    return isMatch;
                }
            }

            return true; // CSV ise (imza kontrolü yok) kabul et
        }
    }
}