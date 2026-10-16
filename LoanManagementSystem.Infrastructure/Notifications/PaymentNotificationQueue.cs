using LoanManagementSystem.Application.ServiceInterface;
using LoanManagementSystem.Domain.Entities;
using LoanManagementSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LoanManagementSystem.Infrastructure.Notifications
{
    public class PaymentNotificationQueue : IPaymentNotificationQueue
    {
        private readonly ApplicationDbContext _db;
        private readonly ICustomerContactClient _customers;

        public PaymentNotificationQueue(
            ApplicationDbContext db,
            ICustomerContactClient customers)
        {
            _db = db;
            _customers = customers;
        }

        public async Task QueueAsync(
            int paymentId,
            int customerId,
            int loanId,
            decimal amount,
            string reference,
            CancellationToken cancellationToken = default)
        {
            var contact = await _customers.GetAsync(
                customerId,
                cancellationToken);

            if (contact == null)
                return;

            var message =
                $"Payment received successfully.\n" +
                $"Amount: INR {amount:N2}\n" +
                $"Loan ID: {loanId}\n" +
                $"Reference: {reference}\n" +
                "Thank you. Loan Management System.";

            // Add notification only if not already queued
            async Task AddAsync(
                string channel,
                string destination)
            {
                bool exists = await _db.PaymentNotifications
                    .AnyAsync(x =>
                        x.PaymentId == paymentId &&
                        x.Channel == channel,
                        cancellationToken);

                if (exists)
                    return;

                _db.PaymentNotifications.Add(
                    new PaymentNotification
                    {
                        PaymentId = paymentId,
                        CustomerId = customerId,
                        Channel = channel,
                        Destination = destination,
                        Subject = "Loan Payment Confirmation",
                        Message = message,
                        Status = "Pending"
                    });
            }

            // SMS
            if (contact.SmsOptIn &&
                !string.IsNullOrWhiteSpace(contact.PhoneNumber))
            {
                await AddAsync("sms", contact.PhoneNumber);
            }

            // WhatsApp
            if (contact.WhatsAppOptIn &&
                !string.IsNullOrWhiteSpace(contact.PhoneNumber))
            {
                await AddAsync("whatsapp", contact.PhoneNumber);
            }

            // Email
            if (contact.EmailOptIn &&
                !string.IsNullOrWhiteSpace(contact.Email))
            {
                await AddAsync("email", contact.Email);
            }

            // Save all notification records
            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}