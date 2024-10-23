using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace NftsArt.Model.Entities;

public class User : IdentityUser
{
    public string FullName { get; set; }

    public string? ResetPasswordOtp { get; set; }
    public DateTime? OtpExpiryTime { get; set; }

    [ForeignKey("Avatar")]
    public int? AvatarId { get; set; }
    public Avatar? Avatar { get; set; }

    public List<Collection> Collections { get; set; } = [];
    public List<NftCollector> NftCollectors { get; set; } = [];
    public List<Nft> Nfts { get; set; } = [];
    public List<Auction> Auctions { get; set; } = [];
    public List<Bid> Bids { get; set; } = [];
    public List<Wallet> Wallets { get; set; } = [];
    public List<Follow> Followers { get; set; } = [];
}
