using NftsArt.Model.Dtos.Avatar;

namespace NftsArt.Model.Dtos.User;

public record class UserDetailDto(
        string Id,
        string UserName,
        string FullName,
        string Email,
        AvatarSummaryDto? Avatar,
        int FollowerCount,
        int CollectionCount,
        decimal AuctionsSumPrice
    );
