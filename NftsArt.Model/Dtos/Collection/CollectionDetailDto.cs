using NftsArt.Model.Dtos.Nft;
using NftsArt.Model.Dtos.User;
using NftsArt.Model.Enums;

namespace NftsArt.Model.Dtos.Collection;

public record class CollectionDetailDto(
        int Id,
        string Name,
        UserSummaryDto Creator,
        Blockchain Blockchain,
        Category Category,
        List<NftSummaryDto> Nfts
    );