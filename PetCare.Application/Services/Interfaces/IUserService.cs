using PetCare.Application.DTOs.User;
using PetCare.Application.Common;

namespace PetCare.Application.Services.Interfaces;

public interface IUserService
{
    Task<ServiceResult<UserDto>> GetUserByIdAsync(Guid userId);
    Task<ServiceResult<UserDto>> GetUserByEmailAsync(string email);
    Task<ServiceResult<PagedResult<UserDto>>> GetUsersAsync(int page, int pageSize);
    Task<ServiceResult<UserDto>> CreateUserAsync(CreateUserDto createUserDto);
    Task<ServiceResult<UserDto>> UpdateUserAsync(Guid userId, UpdateUserDto updateUserDto);
    Task<ServiceResult<UserDto>> SetUserRoleAsync(Guid userId, SetUserRoleDto setUserRoleDto);
    Task<ServiceResult<bool>> DeleteUserAsync(Guid userId);
    Task<ServiceResult<IEnumerable<UserDto>>> GetUsersByRoleAsync(string roleName);
}
