using NftsArt.Model.Dtos.Wallet;
using NftsArt.Model.Entities;

namespace NftsArt.Model.Mapping;

public static class WalletMapping
{
    public static Wallet ToEntity(this WalletCreateDto walletCreateDto, string creatorId)
    {
        return new Wallet
        {
            Balance = walletCreateDto.Balance,
            Blockchain = walletCreateDto.Blockchain,
            Currency = walletCreateDto.Currency,
            ProviderId = walletCreateDto.ProviderId,
            UserId = creatorId,
        };
    }

    public static WalletDetailDto ToDetailDto(this Wallet wallet)
    {
        return new WalletDetailDto
            (
                wallet.Id,
                wallet.Key,
                wallet.Balance,
                wallet.Expiration,
                wallet.Blockchain,
                wallet.Currency,
                wallet.User.ToSummaryDto(),
                wallet.Provider.ToSummaryDto()
            );
    }

    public static WalletSummaryDto ToSummaryDto(this Wallet wallet)
    {
        return new WalletSummaryDto
            (
                wallet.Id,
                wallet.Key,
                wallet.Balance,
                wallet.Expiration,
                wallet.Blockchain,
                wallet.Currency,
                wallet.UserId,
                wallet.ProviderId
            );
    }

    public static void UpdateEntity(this Wallet wallet, WalletUpdateDto updatedWallet)
    {
        wallet.Balance = updatedWallet.Balance;
    }
}
