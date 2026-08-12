using System.Security.Claims;
using System.Text;
using EduTrack.Application.Common;
using EduTrack.Application.Interfaces;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace EduTrack.Infrastructure.Identity;

public class TokenService : ITokenService
{
    private readonly JwtSettings _jwt;

    public TokenService(JwtSettings jwt) => _jwt = jwt;

    public (string Token, DateTime ExpiresAt) GenerateToken(string userId, string email, string fullName, string role)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(_jwt.ExpiryMinutes);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Name, fullName),
            new(ClaimTypes.Role, role),
        };

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiresAt,
            Issuer = _jwt.Issuer,
            Audience = _jwt.Audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key)),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = new JsonWebTokenHandler().CreateToken(descriptor);
        return (token, expiresAt);
    }
}
