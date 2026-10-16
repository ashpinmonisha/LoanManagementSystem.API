using System;
using System.Collections.Generic;
using System.Text;

namespace LoanManagementSystem.Application.Service;

public sealed class NotFoundException(string message) : Exception(message);