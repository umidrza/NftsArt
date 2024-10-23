using NftsArt.Model.Enums;
using System.ComponentModel.DataAnnotations;

namespace NftsArt.Model.Dtos.Nft;

public record class NftCreateDto
{
    [Required(ErrorMessage = "NFT Name is required.")]
    [MaxLength(50, ErrorMessage = "Name can't be longer than 50 characters.")]
    public string Name { get; set; }

    [MaxLength(1000, ErrorMessage = "Description can't be longer than 1000 characters.")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "NFT Image is required.")]
    //[Url(ErrorMessage = "Please enter a valid URL.")]
    public string ImageUrl { get; set; }

    [Required(ErrorMessage = "Blockchain is required.")]
    [EnumDataType(typeof(Blockchain))]
    public Blockchain Blockchain { get; set; }

    public List<int> Collections { get; set; } = new List<int>();
}
