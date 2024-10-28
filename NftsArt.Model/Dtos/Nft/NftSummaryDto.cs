using NftsArt.Model.Dtos.Auction;
using NftsArt.Model.Enums;

namespace NftsArt.Model.Dtos.Nft;

public record class NftSummaryDto(
        int Id,
        string Name,
        string ImageUrl,
        string CreatorId,
        Blockchain Blockchain, 
        NftStatus NftStatus,
        AuctionSummaryDto? Auction
    );
