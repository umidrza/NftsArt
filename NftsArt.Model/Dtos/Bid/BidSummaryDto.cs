using NftsArt.Model.Dtos.Nft;
using NftsArt.Model.Dtos.User;

namespace NftsArt.Model.Dtos.Bid;

public record class BidSummaryDto(
    int Id,
    decimal Price,
    DateTime StartTime,
    DateTime EndTime,
    int Quantity,
    int NftId,
    UserSummaryDto Bidder
);
