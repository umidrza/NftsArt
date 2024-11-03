using System.ComponentModel.DataAnnotations;

namespace NftsArt.Model.Dtos.User;

public record class ResetPasswordDto
{
    [Required]
    public string NewPassword { get; set; }

    [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
    public string ConfirmPassword { get; set; }
}
