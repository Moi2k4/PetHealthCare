using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PetCare.Application.Common;
using PetCare.Application.DTOs.Auth;
using PetCare.Application.DTOs.User;
using PetCare.Application.Services.Interfaces;
using PetCare.Domain.Entities;
using PetCare.Infrastructure.Repositories.Interfaces;
using BCryptNet = BCrypt.Net.BCrypt;

namespace PetCare.Application.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly JwtSettings _jwtSettings;

    public AuthService(IUnitOfWork unitOfWork, IMapper mapper, IOptions<JwtSettings> jwtOptions)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _jwtSettings = jwtOptions.Value;
    }

    public async Task<ServiceResult<AuthResponseDto>> RegisterAsync(RegisterUserDto registerDto)
    {
        try
        {
            if (await _unitOfWork.Users.EmailExistsAsync(registerDto.Email))
            {
                return ServiceResult<AuthResponseDto>.FailureResult("Email already in use");
            }

            if (string.IsNullOrWhiteSpace(registerDto.Password))
            {
                return ServiceResult<AuthResponseDto>.FailureResult("Password is required");
            }

            const string roleName = "Customer";

            var roleRepository = _unitOfWork.Repository<Role>();
            var role = await roleRepository.FirstOrDefaultAsync(r => r.RoleName == roleName);

            if (role == null)
            {
                role = new Role
                {
                    RoleName = roleName,
                    Description = $"Auto-generated role {roleName}"
                };

                await roleRepository.AddAsync(role);
                await _unitOfWork.SaveChangesAsync();
            }

            var user = new User
            {
                Email = registerDto.Email.Trim(),
                FullName = registerDto.FullName.Trim(),
                Phone = registerDto.Phone,
                RoleId = role.Id,
                Role = role,
                PasswordHash = BCryptNet.HashPassword(registerDto.Password)
            };

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            // Reload user with Role to ensure navigation property is populated
            var userWithRole = await _unitOfWork.Users.GetByEmailAsync(user.Email);
            if (userWithRole == null)
            {
                return ServiceResult<AuthResponseDto>.FailureResult("Failed to retrieve user after registration");
            }

            var response = await BuildAuthResponseAsync(userWithRole);
            return ServiceResult<AuthResponseDto>.SuccessResult(response, "Registration successful");
        }
        catch (Exception ex)
        {
            return ServiceResult<AuthResponseDto>.FailureResult($"Registration failed: {ex.Message}");
        }
    }

    public async Task<ServiceResult<AuthResponseDto>> LoginAsync(LoginRequestDto loginDto)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByEmailAsync(loginDto.Email.Trim());
            if (user == null)
            {
                return ServiceResult<AuthResponseDto>.FailureResult("Invalid email or password");
            }

            if (!user.IsActive)
            {
                return ServiceResult<AuthResponseDto>.FailureResult("User account is inactive");
            }

            if (!BCryptNet.Verify(loginDto.Password, user.PasswordHash))
            {
                return ServiceResult<AuthResponseDto>.FailureResult("Invalid email or password");
            }

            var response = await BuildAuthResponseAsync(user);
            return ServiceResult<AuthResponseDto>.SuccessResult(response, "Login successful");
        }
        catch (Exception ex)
        {
            return ServiceResult<AuthResponseDto>.FailureResult($"Login failed: {ex.Message}");
        }
    }

    private Task<AuthResponseDto> BuildAuthResponseAsync(User user)
    {
        var userDto = _mapper.Map<UserDto>(user);
        var token = GenerateJwtToken(user);

        var response = new AuthResponseDto
        {
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiresInMinutes),
            User = userDto
        };

        return Task.FromResult(response);
    }

    private string GenerateJwtToken(User user)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.FullName)
        };

        // Ensure Role is always added to claims
        if (user.Role != null && !string.IsNullOrWhiteSpace(user.Role.RoleName))
        {
            // Normalize to PascalCase so "admin" -> "Admin" matches [Authorize(Roles = "Admin,...")]
            var normalizedRole = char.ToUpper(user.Role.RoleName[0]) + user.Role.RoleName.Substring(1).ToLower();
            claims.Add(new Claim(ClaimTypes.Role, normalizedRole));
        }
        else
        {
            // If Role is null, this is a critical error that should be logged
            // For now, throw an exception to make it obvious
            throw new InvalidOperationException($"User {user.Email} does not have a valid role assigned");
        }

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiresInMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

