namespace EduTrack.Application.Interfaces;

public interface ITokenService
{
    (string Token, DateTime ExpiresAt) GenerateToken(string userId, string email, string fullName, string role);
}
