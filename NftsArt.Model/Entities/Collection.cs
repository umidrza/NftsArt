using System.ComponentModel.DataAnnotations.Schema;
using NftsArt.Model.Enums;

namespace NftsArt.Model.Entities;

public class Collection
{
    public int Id { get; set; }
    public string Name { get; set; }
    public Blockchain Blockchain { get; set; }
    public Category Category { get; set; }

    [ForeignKey("Creator")]
    public string CreatorId { get; set; }
    public User Creator { get; set; }

    public List<CollectionNft> CollectionNfts { get; set; } = [];
}

