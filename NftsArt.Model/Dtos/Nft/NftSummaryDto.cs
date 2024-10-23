using NftsArt.Model.Dtos.Auction;

namespace NftsArt.Model.Dtos.Nft;

public record class NftSummaryDto(
        int Id,
        string Name,
        string ImageUrl,
        string Creator,
        string Blockchain,
        AuctionSummaryDto? Auction
    );
