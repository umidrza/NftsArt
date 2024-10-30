namespace NftsArt.Model.Dtos.User;

public record class FollowDto(
    string FollowerId,
    string FollowingId,
    DateTime FollowedOn,
    bool IsDeleted
);
