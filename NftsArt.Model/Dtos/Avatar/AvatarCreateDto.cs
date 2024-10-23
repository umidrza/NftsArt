using System.ComponentModel.DataAnnotations;

namespace NftsArt.Model.Dtos.Avatar;

public record class AvatarCreateDto
{
    [Required(ErrorMessage = "Avatar Image is required.")]
    [Url(ErrorMessage = "Please enter a valid URL.")]
    public required string ImageUrl { get; set; }
}
