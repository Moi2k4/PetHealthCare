using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Caching.Memory;

namespace PetCare.API.Security;

public class TokenBlacklistService : ITokenBlacklistService
{
    private readonly IMemoryCache _cache;

    public TokenBlacklistService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public void BlacklistToken(string rawToken, string? jti, DateTime expiresAtUtc)
    {
        var absoluteExpiration = expiresAtUtc > DateTime.UtcNow
            ? expiresAtUtc
            : DateTime.UtcNow.AddMinutes(5);

        if (!string.IsNullOrWhiteSpace(rawToken))
        {
            var tokenKey = BuildTokenKey(rawToken);
            _cache.Set(tokenKey, true, absoluteExpiration);
        }

        if (!string.IsNullOrWhiteSpace(jti))
        {
            var jtiKey = BuildJtiKey(jti);
            _cache.Set(jtiKey, true, absoluteExpiration);
        }
    }

    public bool IsBlacklisted(string? rawToken, string? jti)
    {
        if (!string.IsNullOrWhiteSpace(rawToken))
        {
            var tokenKey = BuildTokenKey(rawToken);
            if (_cache.TryGetValue(tokenKey, out _)) return true;
        }

        if (!string.IsNullOrWhiteSpace(jti))
        {
            var jtiKey = BuildJtiKey(jti);
            if (_cache.TryGetValue(jtiKey, out _)) return true;
        }

        return false;
    }

    private static string BuildTokenKey(string rawToken)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(rawToken));
        return $"blacklist:token:{Convert.ToHexString(bytes)}";
    }

    private static string BuildJtiKey(string jti)
    {
        return $"blacklist:jti:{jti}";
    }
}
