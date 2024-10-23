namespace NftsArt.Model.Dtos.Nft;

public record class NftLikeDto
{
    public int LikeCount { get; set; }
    public bool HasUserLiked { get; set; }
};
