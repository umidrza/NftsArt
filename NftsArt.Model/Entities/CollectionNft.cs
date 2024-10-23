using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace NftsArt.Model.Entities;

public class CollectionNft
{
    public int Id { get; set; }

    [ForeignKey("Collection")]
    public int CollectionId { get; set; }
    [JsonIgnore]
    public Collection Collection { get; set; }

    [ForeignKey("Nft")]
    public int NftId { get; set; }
    [JsonIgnore]
    public Nft Nft { get; set; }
}
