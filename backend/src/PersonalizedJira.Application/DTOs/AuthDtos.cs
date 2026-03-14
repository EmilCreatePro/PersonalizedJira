namespace PersonalizedJira.Application.DTOs;

public sealed record LoginRequest(string Email, string Password);

public sealed record AuthResponse(string Token, string DisplayName);
