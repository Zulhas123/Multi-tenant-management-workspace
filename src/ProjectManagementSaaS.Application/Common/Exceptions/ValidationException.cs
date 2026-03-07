namespace ProjectManagementSaaS.Application.Common.Exceptions;

public sealed class ValidationException(string message) : AppException(message);
