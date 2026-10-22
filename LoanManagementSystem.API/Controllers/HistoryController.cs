using LoanManagementSystem.Application.Dtos;
using LoanManagementSystem.Application.ServiceInterface;
using Microsoft.AspNetCore.Mvc;

namespace LoanManagementSystem.API.Controllers;

[ApiController]
[Route("api/emi/history")]
public sealed class HistoryController(ICalculationHistoryService historyService) : ControllerBase
{
    [HttpGet]
    public ActionResult<IReadOnlyList<CalculationHistoryResponse>> GetAll() =>
        Ok(historyService.GetAll());

    [HttpGet("{id:guid}")]
    public ActionResult<CalculationHistoryResponse> GetById(Guid id) =>
        Ok(historyService.GetById(id));
}