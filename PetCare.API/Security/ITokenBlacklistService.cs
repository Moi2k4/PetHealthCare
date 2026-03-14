namespace PetCare.API.Security;

public interface ITokenBlacklistService
{
    void BlacklistToken(string rawToken, string? jti, DateTime expiresAtUtc);
    bool IsBlacklisted(string? rawToken, string? jti);
}
