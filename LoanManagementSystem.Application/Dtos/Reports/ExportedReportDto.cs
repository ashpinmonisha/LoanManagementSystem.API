namespace LoanManagementSystem.Application.DTOs.Reports;

public class ExportedReportDto
{
    public byte[] Content { get; set; } = [];
    public string FileName { get; set; } = "";
    public string ContentType { get; set; } = "";
}