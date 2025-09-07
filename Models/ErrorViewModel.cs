// Konum: Models/ErrorViewModel.cs

namespace BLBM_ENV.Models // AD ALANININ DOÐRU OLDUÐUNDAN EMÝN OLUN
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}