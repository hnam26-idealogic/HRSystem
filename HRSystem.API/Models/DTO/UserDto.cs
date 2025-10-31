using System;
using System.Collections.Generic;
using HRSystem.API.Models.Domain;

namespace HRSystem.API.Models.DTO
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string Fullname { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public UserType UserType { get; set; }
        public string? AccessLevel { get; set; }
        public string? Specialty { get; set; }
        public List<UserRoleDto> UserRoles { get; set; } = new();
    }
}