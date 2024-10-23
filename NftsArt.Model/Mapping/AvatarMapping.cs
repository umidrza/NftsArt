using NftsArt.Model.Dtos.Avatar;
using NftsArt.Model.Entities;

namespace NftsArt.Model.Mapping;

public static class AvatarMapping
{
    public static Avatar ToEntity(this AvatarCreateDto newAvatar)
    {
        return new Avatar { Url = newAvatar.ImageUrl };
    }

    public static AvatarSummaryDto ToSummaryDto(this Avatar avatar)
    {
        return new AvatarSummaryDto(
                avatar.Id,
                avatar.Url
            );
    }

    public static void UpdateEntity(this Avatar avatar, AvatarCreateDto updatedAvatar)
    {
        avatar.Url = updatedAvatar.ImageUrl;
    }
}
