using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using NftsArt.Model.Enums;

namespace NftsArt.Model.Entities;

public class Auction
{
    public int Id { get; set; }

    [Precision(18, 2)]
    public decimal Price { get; set; }
    [Precision(18, 2)]
    public decimal CurrentBid { get; set; } = 0;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int Quantity { get; set; } = 1;
    public Currency Currency { get; set; }

    [ForeignKey("Nft")]
    public int NftId { get; set; }
    public Nft Nft { get; set; }

    [ForeignKey("Seller")]
    public string SellerId { get; set; }
    public User Seller { get; set; }

    public List<Bid> Bids { get; set; } = [];
}
