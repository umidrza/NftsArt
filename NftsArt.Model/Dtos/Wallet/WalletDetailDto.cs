using NftsArt.Model.Dtos.Provider;
using NftsArt.Model.Dtos.User;
using NftsArt.Model.Enums;

namespace NftsArt.Model.Dtos.Wallet;

public record class WalletDetailDto(
    int Id,
    string Key,
    decimal Balance,
    DateTime Expiration,
    Blockchain Blockchain,
    Currency Currency,
    UserSummaryDto User,
    ProviderSummaryDto Provider
);
