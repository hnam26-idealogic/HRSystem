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
      public string UserType { get; set; } // Use string, not enum
      public string AccessLevel { get; set; }
      public string Specialty { get; set; }
  }
}