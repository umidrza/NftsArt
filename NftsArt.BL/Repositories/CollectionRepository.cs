using Microsoft.EntityFrameworkCore;
using NftsArt.Database.Data;
using NftsArt.Model.Dtos.Collection;
using NftsArt.Model.Dtos.Nft;
using NftsArt.Model.Entities;
using NftsArt.Model.Enums;
using NftsArt.Model.Helpers;
using NftsArt.Model.Mapping;

namespace NftsArt.BL.Repositories;


public interface ICollectionRepository
{
    Task<Pagination<CollectionDetailDto>> GetAllAsync(CollectionQueryDto query);
    Task<Collection?> GetByIdAsync(int id);
    Task<Result<CollectionSummaryDto>> CreateAsync(CollectionCreateDto newCollection, string userId);
    Task<Result<CollectionSummaryDto>> UpdateAsync(int id, CollectionUpdateDto updatedCollection);
    Task<Result<CollectionSummaryDto>> DeleteAsync(int id);
    Task<IEnumerable<CollectionDetailDto>> GetAllByUserAsync(string userId);
}

public class CollectionRepository(AppDbContext context) : ICollectionRepository
{
    public async Task<Pagination<CollectionDetailDto>> GetAllAsync(CollectionQueryDto query)
    {
        var collections = context.Collections.AsQueryable();

        collections = FilterCollections(collections, query);
       
        return new Pagination<CollectionDetailDto>
        {
            Count = await collections.CountAsync(),
            Data = await collections
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Include(c => c.Creator)
                    .ThenInclude(u => u.Avatar)
                 .Include(c => c.Creator)
                    .ThenInclude(u => u.Followers)
                .Include(c => c.Creator)
                    .ThenInclude(u => u.Nfts)
                .Include(c => c.CollectionNfts)
                    .ThenInclude(cn => cn.Nft)
                        .ThenInclude(n => n.Auction)
                            .ThenInclude(a => a.Seller)
                .AsNoTracking()
                .Select(c => c.ToDetailDto())
                .ToListAsync()
        };
    }

    public async Task<Collection?> GetByIdAsync(int id)
    {
        var collection = await context.Collections
            .Include(c => c.Creator)
                .ThenInclude(u => u.Avatar)
            .Include(c => c.Creator)
                .ThenInclude(u => u.Followers)
            .Include(c => c.Creator)
                .ThenInclude(u => u.Nfts)
            .Include(c => c.CollectionNfts)
                .ThenInclude(cn => cn.Nft)
                    .ThenInclude(n => n.Auction)
                        .ThenInclude(a => a.Seller)
            .FirstOrDefaultAsync(c => c.Id == id);

        return collection;
    }

    public async Task<Result<CollectionSummaryDto>> CreateAsync(CollectionCreateDto collectionCreateDto, string userId)
    {
        var collection = collectionCreateDto.ToEntity(userId);

        collection.CollectionNfts = collectionCreateDto.Nfts
            .Where(id => context.Nfts.Any(nft => nft.Id == id))
            .Select(id => new CollectionNft { NftId = id, CollectionId = collection.Id })
            .ToList();

        await context.Collections.AddAsync(collection);
        await context.SaveChangesAsync();

        return Result<CollectionSummaryDto>.Success(collection.ToSummaryDto(), "Collection created successfully");
    }

    public async Task<Result<CollectionSummaryDto>> UpdateAsync(int id, CollectionUpdateDto updatedCollection)
    {
        var collection = await context.Collections.FindAsync(id);
        if (collection == null)
            return Result<CollectionSummaryDto>.Failure("Collection not found.");

        collection.UpdateEntity(updatedCollection);

        collection.CollectionNfts = updatedCollection.Nfts
            .Where(nftId => context.Nfts.Any(nft => nft.Id == nftId))
            .Select(nftId => new CollectionNft { NftId = nftId, CollectionId = id })
            .ToList();

        await context.SaveChangesAsync();
        return Result<CollectionSummaryDto>.Success(collection.ToSummaryDto(), "Collection updated successfully.");
    }

    public async Task<Result<CollectionSummaryDto>> DeleteAsync(int id)
    {
        var collection = await context.Collections.FindAsync(id);
        if (collection == null) return Result<CollectionSummaryDto>.Failure("Collection not found.");

        context.Collections.Remove(collection);
        await context.SaveChangesAsync();

        return Result<CollectionSummaryDto>.Success(null!, "Collection deleted successfully.");
    }

    public async Task<IEnumerable<CollectionDetailDto>> GetAllByUserAsync(string userId)
    {
        return await context.Collections
            .Where(c => c.CreatorId == userId)
            .Include(c => c.Creator)
                .ThenInclude(u => u.Avatar)
            .Include(c => c.CollectionNfts)
                .ThenInclude(cn => cn.Nft)
                    .ThenInclude(n => n.Auction)
                        .ThenInclude(a => a.Seller)
            .AsNoTracking()
            .Select(c => c.ToDetailDto())
            .ToListAsync();
    }

    public static IQueryable<Collection> FilterCollections(IQueryable<Collection> collections, CollectionQueryDto query)
    {
        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            collections = collections.Where(c => c.Name.ToLower().Contains(query.SearchTerm.ToLower()));
        }

        if (!string.IsNullOrWhiteSpace(query.BlockchainName))
        {
            if (Enum.TryParse<Blockchain>(query.BlockchainName, true, out var blockchainEnum))
            {
                collections = collections.Where(c => c.Blockchain == blockchainEnum);
            }
        }

        if (!string.IsNullOrEmpty(query.Categories))
        {
            var categoryList = query.Categories.Split(',', StringSplitOptions.RemoveEmptyEntries);
            var validCategories = new List<Category>();

            foreach (var category in categoryList)
            {
                if (Enum.TryParse<Category>(category, true, out var categoryEnum))
                {
                    validCategories.Add(categoryEnum);
                }
            }

            if (validCategories.Count > 0)
            {
                collections = collections.Where(c => validCategories.Contains(c.Category));
            }
        }

        if (!string.IsNullOrWhiteSpace(query.SortBy))
        {
            collections = query.SortBy switch
            {
                "name-asc" => collections.OrderBy(c => c.Name),
                "name-desc" => collections.OrderByDescending(c => c.Name),
                _ => collections
            };
        }

        return collections;
    }
}
