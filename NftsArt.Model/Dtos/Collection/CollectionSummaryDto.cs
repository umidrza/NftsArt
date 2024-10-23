using NftsArt.Model.Dtos.Nft;

namespace NftsArt.Model.Dtos.Collection;

public record class CollectionSummaryDto(
        int Id,
        string Name,
        string Creator,
        string Blockchain,
        string Category
    );