using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace NftsArt.Model.Entities;

public class NftCollector
{
    public int Id { get; set; }

    [ForeignKey("Nft")]
    public int NftId { get; set; }
    [JsonIgnore]
    public Nft Nft { get; set; }

    [ForeignKey("Collector")]
    public string CollectorId { get; set; }
    [JsonIgnore]
    public User Collector { get; set; }

    public int Quantity { get; set; } = 1;
}
