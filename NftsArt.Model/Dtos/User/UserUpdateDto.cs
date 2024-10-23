using System.ComponentModel.DataAnnotations;

namespace NftsArt.Model.Dtos.User;

public record class UserUpdateDto
{ 
    [Required(ErrorMessage = "Username is required")]
    [MinLength(3, ErrorMessage = "Username must be at least 3 characters long")]
    public string UserName { get; set; }

    [Required(ErrorMessage = "Full name is required")]
    [MinLength(3, ErrorMessage = "Full name must be at least 3 characters long")]
    public string FullName { get; set; }

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Please select a valid avatar")]
    public int? AvatarId { get; set; }
}