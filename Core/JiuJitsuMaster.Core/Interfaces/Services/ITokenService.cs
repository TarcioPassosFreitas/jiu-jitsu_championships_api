using JiuJitsuMaster.Core.Aggregates;

namespace JiuJitsuMaster.Core.Interfaces.Services;

public interface ITokenService
{
    string GenerateAccessToken(User user, string jwtKey);
    string GenerateAccessToken(Athlete athlete, string jwtKey);
    string GenerateRefreshToken();
    string GenerateUUIDToken();
}