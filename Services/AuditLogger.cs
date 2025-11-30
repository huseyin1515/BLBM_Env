using BLBM_ENV.Data;
using BLBM_ENV.Models;
using Microsoft.AspNetCore.Identity;

namespace BLBM_ENV.Services
{
    // Bu arayüz (interface), servisi çağırmamızı kolaylaştırır.
    public interface IAuditLogger
    {
        Task LogAsync(string action, string module, string description);
    }

    public class AuditLogger : IAuditLogger
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;

        public AuditLogger(
            ApplicationDbContext context,
            IHttpContextAccessor httpContextAccessor,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
        }

        public async Task LogAsync(string action, string module, string description)
        {
            var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);

            var log = new AuditLog
            {
                UserEmail = user?.Email ?? "Bilinmeyen",
                UserName = user?.FullName ?? "Sistem", // Ad Soyad Burada Alınıyor
                Action = action,
                Module = module,
                Description = description,
                Timestamp = DateTime.Now
            };

            _context.AuditLogs.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}