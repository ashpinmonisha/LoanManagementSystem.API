using LoanManagementSystem.Application.ServiceInterface;
using Microsoft.Extensions.Options;
using MimeKit;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;


namespace LoanManagementSystem.Infrastructure.Notifications
{
    public class PaymentNotificationSender
        : IPaymentNotificationSender
    {
        private readonly NotificationOptions _options;

        public PaymentNotificationSender(
            IOptions<NotificationOptions> options)
        {
            _options = options.Value;
        }

        public async Task SendAsync(
            string channel,
            string destination,
            string subject,
            string message,
            CancellationToken cancellationToken = default)
        {
            switch (channel.ToLowerInvariant())
            {
                case "sms":
                    await SendSmsAsync(destination, message);
                    break;

                case "whatsapp":
                    await SendWhatsAppAsync(destination);
                    break;

                case "email":
                    await SendEmailAsync(
                        destination,
                        subject,
                        message,
                        cancellationToken);
                    break;

                default:
                    throw new ArgumentException(
                        $"Invalid notification channel: {channel}");
            }
        }

        private async Task SendSmsAsync(
            string phoneNumber,
            string message)
        {
            TwilioClient.Init(
                _options.TwilioAccountSid,
                _options.TwilioAuthToken);

            await MessageResource.CreateAsync(
                to: new PhoneNumber(phoneNumber),
                from: new PhoneNumber(_options.SmsFrom),
                body: message);
        }

        private async Task SendWhatsAppAsync(
            string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(
                _options.WhatsAppContentSid))
            {
                throw new InvalidOperationException(
                    "WhatsApp template SID is missing.");
            }

            TwilioClient.Init(
                _options.TwilioAccountSid,
                _options.TwilioAuthToken);

            await MessageResource.CreateAsync(
                to: new PhoneNumber(
                    $"whatsapp:{phoneNumber}"),
                from: new PhoneNumber(
                    _options.WhatsAppFrom),
                contentSid: _options.WhatsAppContentSid);
        }

        private async Task SendEmailAsync(
            string email,
            string subject,
            string message,
            CancellationToken cancellationToken)
        {
            var mail = new MimeMessage();

            mail.From.Add(
                MailboxAddress.Parse(_options.EmailFrom));

            mail.To.Add(
                MailboxAddress.Parse(email));

            mail.Subject = subject;

            mail.Body = new TextPart("plain")
            {
                Text = message
            };

            using var client =
                new MailKit.Net.Smtp.SmtpClient();

            await client.ConnectAsync(
                _options.SmtpHost,
                _options.SmtpPort,
                MailKit.Security.SecureSocketOptions.StartTls,
                cancellationToken);

            await client.AuthenticateAsync(
                _options.SmtpUser,
                _options.SmtpPassword,
                cancellationToken);

            await client.SendAsync(
                mail,
                cancellationToken);

            await client.DisconnectAsync(
                true,
                cancellationToken);
        }
    }
}