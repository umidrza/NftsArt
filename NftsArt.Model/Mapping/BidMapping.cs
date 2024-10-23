using NftsArt.Model.Dtos.Bid;
using NftsArt.Model.Entities;

namespace NftsArt.Model.Mapping;

public static class BidMapping
{
    public static Bid ToEntity(this BidCreateDto bidCreateDto, string creatorId, int auctionId)
    {
        return new Bid
        {
            Amount = bidCreateDto.Amount,
            EndTime = bidCreateDto.EndTime,
            BidderId = creatorId,
            AuctionId = auctionId
        };
    }

    public static BidDetailDto ToDetailDto(this Bid bid)
    {
        return new BidDetailDto
            (
                bid.Id,
                bid.Amount,
                bid.StartTime,
                bid.EndTime,
                bid.Quantity,
                bid.Auction.ToSummaryDto(),
                bid.Bidder.ToSummaryDto()
            );
    }

    public static BidSummaryDto ToSummaryDto(this Bid bid)
    {
        return new BidSummaryDto
            (
                bid.Id,
                bid.Amount,
                bid.StartTime,
                bid.EndTime,
                bid.Quantity,
                bid.AuctionId,
                bid.Bidder.ToSummaryDto()
            );
    }
}

