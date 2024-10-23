using NftsArt.Model.Dtos.Auction;
using NftsArt.Model.Dtos.User;

namespace NftsArt.Model.Dtos.Nft;

public record class NftDetailDto(
        int Id,
        string Name,
        string Description,
        string ImageUrl,
        string BlockchainName,
        UserSummaryDto Creator,
        AuctionSummaryDto? Auction
    );

