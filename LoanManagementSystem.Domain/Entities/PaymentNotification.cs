namespace LoanManagementSystem.Domain.Entities;

public class PaymentNotification
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public int PaymentId { get; set; }
    public int CustomerId { get; set; }

    public string Channel { get; set; } = "";
    public string Destination { get; set; } = "";
    public string Subject { get; set; } = "";
    public string Message { get; set; } = "";

    public string Status { get; set; } = "Pending";
    public int Attempts { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? NextAttemptAt { get; set; }
    public DateTime? SentAt { get; set; }
    public string? LastError { get; set; }
}