using NftsArt.Model.Dtos.Nft;
using NftsArt.Model.Dtos.User;

namespace NftsArt.Model.Dtos.Auction;

public record class AuctionDetailDto(
    int Id,
    decimal Price,
    DateTime StartTime,
    DateTime EndTime,
    int Quantity,
    string Currency,
    NftSummaryDto Nft,
    UserSummaryDto Seller
);