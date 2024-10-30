using NftsArt.Model.Dtos.Nft;
using NftsArt.Model.Entities;
using NftsArt.Model.Enums;

namespace NftsArt.Model.Mapping;

public static class NftMapping
{
    public static Nft ToEntity(this NftCreateDto nftCreateDto, string creatorId)
    {
        return new Nft
        {
            Name = nftCreateDto.Name,
            Description = nftCreateDto.Description,
            ImageUrl = nftCreateDto.ImageUrl,
            Blockchain = nftCreateDto.Blockchain,
            CreatorId = creatorId,
        };
    }

    public static NftDetailDto ToDetailDto(this Nft nft)
    {
        return new NftDetailDto
            (
                nft.Id,
                nft.Name,
                nft.Description,
                nft.ImageUrl,
                nft.Blockchain,
                nft.GetAuctionStatus(),
                nft.CreatorId,
                nft.Creator.ToSummaryDto(),
                nft.Auction?.ToSummaryDto()
            );
    }

    public static NftSummaryDto ToSummaryDto(this Nft nft)
    {
        return new NftSummaryDto
            (
                nft.Id,
                nft.Name,
                nft.ImageUrl,
                nft.CreatorId,
                nft.Blockchain,
                nft.GetAuctionStatus(),
                nft.Auction?.ToSummaryDto()
            );
    }

    public static void UpdateEntity(this Nft nft, NftUpdateDto updatedNft)
    {
        nft.Name = updatedNft.Name;
        nft.Description = updatedNft.Description;
    }

    public static NftStatus GetAuctionStatus(this Nft nft)
    {
        if (nft.Auction == null)
            return NftStatus.Not_On_Sale;

        if (DateTime.Now < nft.Auction.StartTime)
            return NftStatus.Not_Started;

        if (DateTime.Now > nft.Auction.EndTime || nft.Auction.Quantity <= 0)
            return NftStatus.Expired;

        return NftStatus.Listed;
    }

    public static NftLikeDto ToLikeDto(this Like like)
    {
        return new NftLikeDto
            (
                like.NftId,
                like.UserId,
                like.LikedOn
            );
    }
}

