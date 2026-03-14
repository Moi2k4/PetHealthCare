using PetCare.Application.Common;
using PetCare.Application.DTOs.Auth;

namespace PetCare.Application.Services.Interfaces;

public interface IAuthService
{
    Task<ServiceResult<AuthResponseDto>> RegisterAsync(RegisterUserDto registerDto);
    Task<ServiceResult<AuthResponseDto>> LoginAsync(LoginRequestDto loginDto);
    Task<ServiceResult<AuthResponseDto>> GoogleLoginAsync(string idToken);
}
