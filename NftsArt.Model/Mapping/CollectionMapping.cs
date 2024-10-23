using NftsArt.Model.Dtos.Collection;
using NftsArt.Model.Entities;

namespace NftsArt.Model.Mapping;

public static class CollectionMapping
{
    public static Collection ToEntity(this CollectionCreateDto collectionCreateDto, string creatorId)
    {
        return new Collection
        {
            Name = collectionCreateDto.Name,
            CreatorId = creatorId,
            Blockchain = collectionCreateDto.Blockchain,
            Category = collectionCreateDto.Category,
        };
    }

    public static CollectionDetailDto ToDetailDto(this Collection collection)
    {
        return new CollectionDetailDto
            (
                collection.Id,
                collection.Name,
                collection.Creator.ToSummaryDto(),
                collection.Blockchain,
                collection.Category,
                collection.CollectionNfts.Select(cn => cn.Nft.ToSummaryDto()).ToList()
            );
    }

    public static CollectionSummaryDto ToSummaryDto(this Collection collection)
    {
        return new CollectionSummaryDto
            (
                collection.Id,
                collection.Name,
                collection.CreatorId,
                collection.Blockchain.ToString(),
                collection.Category.ToString()
            );
    }

    public static void UpdateEntity(this Collection collection, CollectionUpdateDto updatedCollection)
    {
        collection.Name = updatedCollection.Name;
        collection.Blockchain = updatedCollection.Blockchain;
        collection.Category = updatedCollection.Category;
    }

    public static CollectionUpdateDto ToUpdateDto(this CollectionDetailDto collectionDetailDto)
    {
        return new CollectionUpdateDto
        {
            Name = collectionDetailDto.Name,
            Blockchain = collectionDetailDto.Blockchain,
            Category = collectionDetailDto.Category,
            Nfts = collectionDetailDto.Nfts.Select(c => c.Id).ToList(),
        };
    }
}
