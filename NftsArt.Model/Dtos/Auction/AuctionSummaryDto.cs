using NftsArt.Model.Dtos.User;

namespace NftsArt.Model.Dtos.Auction;

public record class AuctionSummaryDto(
        int Id,
        decimal Price,
        decimal CurrentBid,
        DateTime StartTime,
        DateTime EndTime,
        int Quantity,
        string Currency,
        int NftId,
        UserSummaryDto Seller
    );
