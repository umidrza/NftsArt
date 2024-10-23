using System.ComponentModel.DataAnnotations;

namespace NftsArt.Model.Dtos.Nft;

public record class NftUpdateDto
{
    [Required(ErrorMessage = "NFT Name is required.")]
    [MaxLength(50, ErrorMessage = "Name can't be longer than 50 characters.")]
    public string Name { get; set; }

    [MaxLength(1000, ErrorMessage = "Description can't be longer than 1000 characters.")]
    public string Description { get; set; } = string.Empty;

    public List<int> Collections { get; set; } = new List<int>();
}