using NftsArt.Model.Dtos.Avatar;

namespace NftsArt.Model.Dtos.User;

public record class UserSummaryDto(
        string Id,
        string UserName,
        string FullName,
        string Email,
        AvatarSummaryDto? Avatar,
        int FollowerCount,
        int CollectionCount
    );
