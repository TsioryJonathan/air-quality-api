namespace AirQuality.Application;

/// <summary>The request is well formed but breaks a business rule. Mapped to HTTP 400.</summary>
public sealed class ValidationException(string message) : Exception(message);

/// <summary>The requested resource does not exist. Mapped to HTTP 404.</summary>
public sealed class NotFoundException(string message) : Exception(message);
