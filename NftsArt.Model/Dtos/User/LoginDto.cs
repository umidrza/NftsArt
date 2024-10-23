using System.ComponentModel.DataAnnotations;

namespace NftsArt.Model.Dtos.User;

public record class LoginDto
{ 
    [Required(ErrorMessage = "Username is required")]
    public string UserName { get; set; }

    [Required(ErrorMessage = "Password is required")]
    public string Password { get; set; }
};