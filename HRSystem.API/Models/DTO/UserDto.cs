using System;
using System.Collections.Generic;
using HRSystem.API.Models.Domain;

namespace HRSystem.API.Models.DTO
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string UserPrincipalName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Fullname { get; set; } = string.Empty;
        public List<string?> AppRoles { get; set; } = new List<string?>();
    }
}