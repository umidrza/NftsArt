using NftsArt.Model.Dtos.Auction;
using NftsArt.Model.Entities;

namespace NftsArt.Model.Mapping;

public static class AuctionMapping
{
    public static Auction ToEntity(this AuctionCreateDto auctionCreateDto, string creatorId, int nftId)
    {
        return new Auction
        {
            Price = auctionCreateDto.Price,
            StartTime = auctionCreateDto.StartTime,
            EndTime = auctionCreateDto.EndTime,
            Currency = auctionCreateDto.Currency,
            Quantity = auctionCreateDto.Quantity,
            SellerId = creatorId,
            NftId = nftId
        };
    }

    public static AuctionDetailDto ToDetailDto(this Auction auction)
    {
        return new AuctionDetailDto
            (
                auction.Id,
                auction.Price,
                auction.CurrentBid,
                auction.StartTime,
                auction.EndTime,
                auction.Quantity,
                auction.Currency.ToString(),
                auction.Nft.ToSummaryDto(),
                auction.Seller.ToSummaryDto()
            );
    }

    public static AuctionSummaryDto ToSummaryDto(this Auction auction)
    {
        return new AuctionSummaryDto
            (
                auction.Id,
                auction.Price,
                auction.CurrentBid,
                auction.StartTime,
                auction.EndTime,
                auction.Quantity,
                auction.Currency.ToString(),
                auction.NftId,
                auction.Seller.ToSummaryDto()
            );
    }

    public static void UpdateEntity(this Auction auction, AuctionUpdateDto updatedAuction)
    {
        auction.Price = updatedAuction.Price;
        auction.StartTime = updatedAuction.StartTime;
        auction.EndTime = updatedAuction.EndTime;
        auction.Quantity = updatedAuction.Quantity;
    }
}

