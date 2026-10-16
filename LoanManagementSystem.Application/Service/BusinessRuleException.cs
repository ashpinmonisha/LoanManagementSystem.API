using System;
using System.Collections.Generic;
using System.Text;

namespace LoanManagementSystem.Application.Service;

/// <summary>Thrown when a request is valid in shape but breaks a loan business rule.</summary>
public sealed class BusinessRuleException(string message) : Exception(message);