using NftsArt.Model.Enums;

namespace NftsArt.Model.Dtos.Auction;

public record class AuctionSummaryDto(
        int Id,
        decimal Price,
        decimal CurrentBid,
        DateTime StartTime,
        DateTime EndTime,
        int Quantity,
        Currency Currency,
        NftStatus NftStatus,
        int NftId,
        string SellerId
    );
