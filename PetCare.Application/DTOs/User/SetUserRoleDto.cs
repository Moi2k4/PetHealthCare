using System;

namespace PetCare.Application.DTOs.User;

public class SetUserRoleDto
{
    public Guid? RoleId { get; set; }
    public string? RoleName { get; set; }
}
