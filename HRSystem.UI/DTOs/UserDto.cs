using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using HRSystem.UI.Models;

namespace HRSystem.UI.DTOs
{
  public class UserDto
  {
      public Guid Id { get; set; }
      public string Fullname { get; set; }
      public string Email { get; set; }
      public string PhoneNumber { get; set; }

      public string? GivenName { get; set; }
      public string? FamilyName { get; set; }
      public List<string> Roles { get; set; } = new();

      public string UserType { get; set; } // Use string, not enum
      public string AccessLevel { get; set; }
      public string Specialty { get; set; }

    //public Guid Id { get; set; }
    public string UserPrincipalName { get; set; } = string.Empty;
    //public string Email { get; set; } = string.Empty;
    //public string Fullname { get; set; } = string.Empty;
    public List<string?> AppRoles { get; set; } = new List<string?>();
    }
}

