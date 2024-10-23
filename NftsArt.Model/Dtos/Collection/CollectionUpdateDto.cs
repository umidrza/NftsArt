using NftsArt.Model.Enums;
using System.ComponentModel.DataAnnotations;

namespace NftsArt.Model.Dtos.Collection;

public record class CollectionUpdateDto
{
    [Required(ErrorMessage = "Collection name is required.")]
    [MaxLength(50, ErrorMessage = "Name can't be longer than 50 characters.")]
    public string Name { get; set; }

    [Required(ErrorMessage = "Blockchain is required.")]
    [EnumDataType(typeof(Blockchain))]
    public Blockchain Blockchain { get; set; }

    [Required(ErrorMessage = "Category is required.")]
    [EnumDataType(typeof(Category))]
    public Category Category { get; set; }

    public List<int> Nfts { get; set; } = new List<int>();
}
