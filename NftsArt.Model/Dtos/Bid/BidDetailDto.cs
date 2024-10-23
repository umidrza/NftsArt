using NftsArt.Model.Dtos.Auction;
using NftsArt.Model.Dtos.User;

namespace NftsArt.Model.Dtos.Bid;

public record class BidDetailDto(
        int Id,
        decimal Amount,
        DateTime StartTime,
        DateTime EndTime,
        int Quantity,
        AuctionSummaryDto Auction,
        UserSummaryDto Bidder
    );
