using NftsArt.Model.Dtos.Avatar;

namespace NftsArt.Model.Dtos.User;

public record class CollectorDto(
        string Id,
        string UserName,
        string FullName,
        AvatarSummaryDto? Avatar,
        int NftCount,
        List<FollowDto> Followers,
        decimal AuctionsSumPrice
);
