using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace NftsArt.Model.Entities;

public class Bid
{
    public int Id { get; set; }

    [Precision(18, 2)]
    public decimal Amount { get; set; }
    public DateTime StartTime { get; set; } = DateTime.UtcNow;
    public DateTime EndTime { get; set; }
    public int Quantity { get; set; } = 1;

    [ForeignKey("Auction")]
    public int AuctionId { get; set; }
    public Auction Auction { get; set; }

    [ForeignKey("Bidder")]
    public string BidderId { get; set; }
    public User Bidder { get; set; }
}
