// A token to send as "Authorization: Bearer <token>"
public record LoginResponse(string Token, DateTime ExpiresAt);
