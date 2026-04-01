using BCrypt.Net;
using ECommerce.Application.Common.Interfaces;

namespace ECommerce.Infrastructure.Services;

public class PasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 12;

    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
        catch (SaltParseException)
        {
            return false;
        }
    }
}
