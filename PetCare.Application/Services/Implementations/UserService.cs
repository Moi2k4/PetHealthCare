using AutoMapper;
using PetCare.Application.DTOs.User;
using PetCare.Application.Common;
using PetCare.Application.Services.Interfaces;
using PetCare.Infrastructure.Repositories.Interfaces;
using PetCare.Domain.Entities;
using BCryptNet = BCrypt.Net.BCrypt;

namespace PetCare.Application.Services.Implementations;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UserService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ServiceResult<UserDto>> GetUserByIdAsync(Guid userId)
    {
        try
        {
            var user = await _unitOfWork.Users.GetUserWithRoleAsync(userId);
            
            if (user == null)
            {
                return ServiceResult<UserDto>.FailureResult("User not found");
            }

            var userDto = _mapper.Map<UserDto>(user);
            return ServiceResult<UserDto>.SuccessResult(userDto);
        }
        catch (Exception ex)
        {
            return ServiceResult<UserDto>.FailureResult($"Error retrieving user: {ex.Message}");
        }
    }

    public async Task<ServiceResult<UserDto>> GetUserByEmailAsync(string email)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByEmailAsync(email);
            
            if (user == null)
            {
                return ServiceResult<UserDto>.FailureResult("User not found");
            }

            var userDto = _mapper.Map<UserDto>(user);
            return ServiceResult<UserDto>.SuccessResult(userDto);
        }
        catch (Exception ex)
        {
            return ServiceResult<UserDto>.FailureResult($"Error retrieving user: {ex.Message}");
        }
    }

    public async Task<ServiceResult<PagedResult<UserDto>>> GetUsersAsync(int page, int pageSize)
    {
        try
        {
            var (users, totalCount) = await _unitOfWork.Users.GetPagedAsync(
                page, 
                pageSize,
                orderBy: q => q.OrderBy(u => u.FullName),
                includes: u => u.Role!
            );

            var userDtos = _mapper.Map<IEnumerable<UserDto>>(users);
            
            var pagedResult = new PagedResult<UserDto>
            {
                Items = userDtos,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };

            return ServiceResult<PagedResult<UserDto>>.SuccessResult(pagedResult);
        }
        catch (Exception ex)
        {
            return ServiceResult<PagedResult<UserDto>>.FailureResult($"Error retrieving users: {ex.Message}");
        }
    }

    public async Task<ServiceResult<UserDto>> CreateUserAsync(CreateUserDto createUserDto)
    {
        try
        {
            // Check if email already exists
            if (await _unitOfWork.Users.EmailExistsAsync(createUserDto.Email))
            {
                return ServiceResult<UserDto>.FailureResult("Email already exists");
            }

            if (string.IsNullOrWhiteSpace(createUserDto.Password))
            {
                return ServiceResult<UserDto>.FailureResult("Password is required");
            }

            var user = _mapper.Map<User>(createUserDto);
            user.PasswordHash = BCryptNet.HashPassword(createUserDto.Password);
            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            var userDto = _mapper.Map<UserDto>(user);
            return ServiceResult<UserDto>.SuccessResult(userDto, "User created successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult<UserDto>.FailureResult($"Error creating user: {ex.Message}");
        }
    }

    public async Task<ServiceResult<UserDto>> UpdateUserAsync(Guid userId, UpdateUserDto updateUserDto)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            
            if (user == null)
            {
                return ServiceResult<UserDto>.FailureResult("User not found");
            }

            _mapper.Map(updateUserDto, user);

            if (!string.IsNullOrWhiteSpace(updateUserDto.NewPassword))
            {
                user.PasswordHash = BCryptNet.HashPassword(updateUserDto.NewPassword);
            }
            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            var userDto = _mapper.Map<UserDto>(user);
            return ServiceResult<UserDto>.SuccessResult(userDto, "User updated successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult<UserDto>.FailureResult($"Error updating user: {ex.Message}");
        }
    }

    public async Task<ServiceResult<UserDto>> SetUserRoleAsync(Guid userId, SetUserRoleDto setUserRoleDto)
    {
        try
        {
            if (!setUserRoleDto.RoleId.HasValue && string.IsNullOrWhiteSpace(setUserRoleDto.RoleName))
            {
                return ServiceResult<UserDto>.FailureResult("RoleId or RoleName must be provided");
            }

            var user = await _unitOfWork.Users.GetUserWithRoleAsync(userId);

            if (user == null)
            {
                return ServiceResult<UserDto>.FailureResult("User not found");
            }

            var roleRepository = _unitOfWork.Repository<Role>();
            Role? role = null;

            if (setUserRoleDto.RoleId.HasValue)
            {
                role = await roleRepository.GetByIdAsync(setUserRoleDto.RoleId.Value);
            }

            if (role == null && !string.IsNullOrWhiteSpace(setUserRoleDto.RoleName))
            {
                var normalizedRoleName = setUserRoleDto.RoleName.Trim();
                role = await roleRepository.FirstOrDefaultAsync(r => r.RoleName == normalizedRoleName);
            }

            if (role == null)
            {
                return ServiceResult<UserDto>.FailureResult("Specified role not found");
            }

            if (user.RoleId == role.Id)
            {
                var existingRoleDto = _mapper.Map<UserDto>(user);
                return ServiceResult<UserDto>.SuccessResult(existingRoleDto, "User already has this role");
            }

            user.RoleId = role.Id;
            user.Role = role;

            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            var userDto = _mapper.Map<UserDto>(user);
            return ServiceResult<UserDto>.SuccessResult(userDto, "User role updated successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult<UserDto>.FailureResult($"Error updating user role: {ex.Message}");
        }
    }

    public async Task<ServiceResult<bool>> DeleteUserAsync(Guid userId)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            
            if (user == null)
            {
                return ServiceResult<bool>.FailureResult("User not found");
            }

            await _unitOfWork.Users.DeleteAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return ServiceResult<bool>.SuccessResult(true, "User deleted successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.FailureResult($"Error deleting user: {ex.Message}");
        }
    }

    public async Task<ServiceResult<IEnumerable<UserDto>>> GetUsersByRoleAsync(string roleName)
    {
        try
        {
            var users = await _unitOfWork.Users.GetUsersByRoleAsync(roleName);
            var userDtos = _mapper.Map<IEnumerable<UserDto>>(users);
            
            return ServiceResult<IEnumerable<UserDto>>.SuccessResult(userDtos);
        }
        catch (Exception ex)
        {
            return ServiceResult<IEnumerable<UserDto>>.FailureResult($"Error retrieving users by role: {ex.Message}");
        }
    }

    // User profile operations
    public async Task<ServiceResult<UserDto>> GetMyProfileAsync(Guid userId)
    {
        try
        {
            var user = await _unitOfWork.Users.GetUserWithRoleAsync(userId);
            
            if (user == null)
            {
                return ServiceResult<UserDto>.FailureResult("User not found");
            }

            var userDto = _mapper.Map<UserDto>(user);
            return ServiceResult<UserDto>.SuccessResult(userDto);
        }
        catch (Exception ex)
        {
            return ServiceResult<UserDto>.FailureResult($"Error retrieving profile: {ex.Message}");
        }
    }

    public async Task<ServiceResult<UserDto>> UpdateMyProfileAsync(Guid userId, UpdateProfileDto updateProfileDto)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            
            if (user == null)
            {
                return ServiceResult<UserDto>.FailureResult("User not found");
            }

            // Update only allowed fields (AvatarUrl is NOT included - use UploadAvatarAsync for that)
            if (!string.IsNullOrEmpty(updateProfileDto.FullName))
                user.FullName = updateProfileDto.FullName;
            
            if (!string.IsNullOrEmpty(updateProfileDto.Phone))
                user.Phone = updateProfileDto.Phone;
            
            // NOTE: AvatarUrl is intentionally NOT updated here
            // Use POST /api/profile/avatar endpoint to update avatar
            
            if (!string.IsNullOrEmpty(updateProfileDto.Address))
                user.Address = updateProfileDto.Address;
            
            if (!string.IsNullOrEmpty(updateProfileDto.City))
                user.City = updateProfileDto.City;
            
            if (!string.IsNullOrEmpty(updateProfileDto.District))
                user.District = updateProfileDto.District;

            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            // Reload with role information
            user = await _unitOfWork.Users.GetUserWithRoleAsync(userId);
            var userDto = _mapper.Map<UserDto>(user);
            
            return ServiceResult<UserDto>.SuccessResult(userDto);
        }
        catch (Exception ex)
        {
            return ServiceResult<UserDto>.FailureResult($"Error updating profile: {ex.Message}");
        }
    }

    public async Task<ServiceResult<bool>> ChangePasswordAsync(Guid userId, ChangePasswordDto changePasswordDto)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            
            if (user == null)
            {
                return ServiceResult<bool>.FailureResult("User not found");
            }

            // Verify current password
            if (!BCryptNet.Verify(changePasswordDto.CurrentPassword, user.PasswordHash))
            {
                return ServiceResult<bool>.FailureResult("Current password is incorrect");
            }

            // Hash and update new password
            user.PasswordHash = BCryptNet.HashPassword(changePasswordDto.NewPassword);
            
            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();
            
            return ServiceResult<bool>.SuccessResult(true, "Password changed successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.FailureResult($"Error changing password: {ex.Message}");
        }
    }

    public async Task<ServiceResult<bool>> UploadAvatarAsync(Guid userId, string avatarUrl)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            
            if (user == null)
            {
                return ServiceResult<bool>.FailureResult("User not found");
            }

            user.AvatarUrl = avatarUrl;
            
            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();
            
            return ServiceResult<bool>.SuccessResult(true, "Avatar uploaded successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.FailureResult($"Error uploading avatar: {ex.Message}");
        }
    }
}
