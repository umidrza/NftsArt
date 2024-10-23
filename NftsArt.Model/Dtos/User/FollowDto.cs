using System.ComponentModel.DataAnnotations;

namespace NftsArt.Model.Dtos.User;

public record class FollowDto
{
    [Required]
    public required string FollowingUserId { get; set; }
}
