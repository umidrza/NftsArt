using NftsArt.Model.Enums;

namespace NftsArt.Model.Dtos.Wallet;

public record class WalletSummaryDto(
    int Id,
    string Key,
    decimal Balance,
    DateTime Expiration,
    Blockchain Blockchain,
    Currency Currency,
    string UserId,
    int ProviderId
);
