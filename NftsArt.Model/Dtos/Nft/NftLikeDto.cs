namespace NftsArt.Model.Dtos.Nft;

public record class NftLikeDto(
     int NftId,
     string UserId,
     DateTime LikedOn
);
