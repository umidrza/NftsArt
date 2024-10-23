using System.ComponentModel.DataAnnotations;

namespace NftsArt.Model.Dtos.User;

public record class UpdatePasswordDto
{
    [Required]
    public string CurrentPassword { get; set; }

    [Required]
    public string NewPassword { get; set; }

    [Compare("Password", ErrorMessage = "Passwords do not match")]
    public string ConfirmPassword { get; set; }
}