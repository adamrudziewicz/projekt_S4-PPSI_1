namespace OgloszeniaSytem.Models
{
    public class Error
    {
        public string? RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
        public string? ErrorMessage { get; set; }
        public string? ErrorDetails { get; set; }
        public int StatusCode { get; set; } = 500;
    }
}