using System.ComponentModel.DataAnnotations;

namespace NftsArt.Model.Dtos.Provider;

public record class ProviderCreateDto
{
    [Required(ErrorMessage = "Provider Name is required.")]
    [MaxLength(20, ErrorMessage = "Name can't be longer than 20 characters.")]
    public required string Name { get; set; }

    [Required(ErrorMessage = "Provider Image is required.")]
    [Url(ErrorMessage = "Please enter a valid URL.")]
    public required string ImageUrl { get; set; }
}
