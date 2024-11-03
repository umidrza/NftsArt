using System.ComponentModel.DataAnnotations;

namespace NftsArt.Model.Dtos.User;

public record class ForgotPasswordDto
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string Email { get; set; }
}
