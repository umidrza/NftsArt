namespace NftsArt.Model.Dtos.User;

public record class LoginResponseDto (
    string UserName,
    string Email,
    string Token
);

