using NftsArt.Model.Dtos.Auction;
using NftsArt.Model.Dtos.User;
using NftsArt.Model.Enums;

namespace NftsArt.Model.Dtos.Nft;

public record class NftDetailDto(
        int Id,
        string Name,
        string Description,
        string ImageUrl,
        Blockchain Blockchain,
        NftStatus NftStatus,
        string CreatorId,
        UserSummaryDto Creator,
        AuctionSummaryDto? Auction
    );

