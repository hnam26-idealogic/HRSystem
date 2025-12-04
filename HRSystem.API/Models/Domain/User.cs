using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRSystem.API.Models.Domain
{
    public class User
    {
        [Required]
        public Guid Id { get; set; }
        public string UserPrincipalName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Fullname { get; set; } = string.Empty;
        public List<string?> AppRoles { get; set; } = new List<string?>();
    }
}