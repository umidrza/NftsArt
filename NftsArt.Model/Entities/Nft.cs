using System.ComponentModel.DataAnnotations.Schema;
using NftsArt.Model.Enums;

namespace NftsArt.Model.Entities;

public class Nft
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; }
    public Blockchain Blockchain { get; set; }

    [ForeignKey("Creator")]
    public string CreatorId { get; set; }
    public User Creator { get; set; }

    public Auction? Auction { get; set; }
    public List<CollectionNft> CollectionNfts { get; set; } = [];
    public List<NftCollector> NftCollectors { get; set; } = [];
    public List<Like> Likes { get; set; } = [];
}
