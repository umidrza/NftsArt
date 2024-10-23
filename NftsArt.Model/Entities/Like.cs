using System.ComponentModel.DataAnnotations.Schema;

namespace NftsArt.Model.Entities;

public class Like
{
    public int Id { get; set; }

    [ForeignKey("Nft")]
    public int NftId { get; set; }
    public Nft Nft { get; set; }

    [ForeignKey("User")]
    public string UserId { get; set; }
    public User User { get; set; } 
    public DateTime LikedOn { get; set; } = DateTime.Now;
}
