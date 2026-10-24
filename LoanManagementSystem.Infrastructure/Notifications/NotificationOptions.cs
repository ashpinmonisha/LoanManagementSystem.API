namespace LoanManagementSystem.Infrastructure.Notifications
{
    public class NotificationOptions
    {
        public string TwilioAccountSid { get; set; } = "";
        public string TwilioAuthToken { get; set; } = "";

        public string SmsFrom { get; set; } = "";
        public string WhatsAppFrom { get; set; } = "";
        public string WhatsAppContentSid { get; set; } = "";

        public string SmtpHost { get; set; } = "";
        public int SmtpPort { get; set; } = 587;

        public string SmtpUser { get; set; } = "";
        public string SmtpPassword { get; set; } = "";
        public string EmailFrom { get; set; } = "";
    }
}