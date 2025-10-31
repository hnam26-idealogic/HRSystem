using System.ComponentModel.DataAnnotations;

public class UpdateUserRequestDto
{
    public string Fullname { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string UserType { get; set; } = string.Empty;
    public string? AccessLevel { get; set; }
    public string? Specialty { get; set; }
}