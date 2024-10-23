using NftsArt.Model.Dtos.User;
using NftsArt.Model.Entities;

namespace NftsArt.Model.Mapping;

public static class UserMapping
{
    public static User ToEntity(this RegisterDto registerDto)
    {
        return new User
        {
            UserName = registerDto.UserName,
            FullName = registerDto.FullName,
            Email = registerDto.Email,
            AvatarId = registerDto.AvatarId
        };
    }

    public static UserDetailDto ToDetailDto(this User user)
    {
        return new UserDetailDto(
                user.Id,
                user.UserName!,
                user.FullName,
                user.Email!,
                user.Avatar?.ToSummaryDto(),
                user.Followers.Count,
                user.Collections.Count,
                user.Auctions.Sum(a => a.Price)
            );
    }

    public static UserSummaryDto ToSummaryDto(this User user)
    {
        return new UserSummaryDto(
                user.Id,
                user.UserName!,
                user.FullName,
                user.Email!,
                user.Avatar?.ToSummaryDto(),
                user.Followers.Count,
                user.Collections.Count
            );
    }

    public static void UpdateEntity(this User user, UserUpdateDto updatedUser)
    {
        user.UserName = updatedUser.UserName;
        user.FullName = updatedUser.FullName;
        user.Email = updatedUser.Email;
        user.AvatarId = updatedUser.AvatarId;
    }

    public static UserUpdateDto ToUpdateDto(this UserDetailDto user)
    {
        return new UserUpdateDto()
        {
            FullName = user.FullName,
            UserName = user.UserName,
            Email = user.Email,
            AvatarId = user.Avatar?.Id,
        };
    }
}

