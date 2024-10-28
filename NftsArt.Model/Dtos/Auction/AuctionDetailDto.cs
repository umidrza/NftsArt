using NftsArt.Model.Dtos.Nft;
using NftsArt.Model.Dtos.User;
using NftsArt.Model.Enums;

namespace NftsArt.Model.Dtos.Auction;

public record class AuctionDetailDto(
    int Id,
    decimal Price,
    decimal CurrentBid,
    DateTime StartTime,
    DateTime EndTime,
    int Quantity,
    Currency Currency,
    NftStatus NftStatus,
    NftSummaryDto Nft,
    UserSummaryDto Seller
);