using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NftsArt.BL.Repositories;
using System.Security.Claims;
using NftsArt.Model.Mapping;
using NftsArt.Model.Dtos.Collection;
using NftsArt.Model.Dtos.Nft;
using NftsArt.Model.Helpers;

namespace NftsArt.ApiService.Controllers;

[Route("api/collection")]
[ApiController]
public class CollectionController(ICollectionRepository collectionRepo) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<Result<List<CollectionDetailDto>>>> GetCollections([FromQuery] CollectionQueryDto query)
    {
        var collections = (await collectionRepo.GetAllAsync(query))
                .Select(c => c.ToDetailDto()).ToList();

        return Ok(Result<List<CollectionDetailDto>>.Success(collections));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Result<CollectionDetailDto>>> GetCollection([FromRoute] int id)
    {
        var collection = await collectionRepo.GetByIdAsync(id);
        if (collection == null) 
            return Ok(Result<CollectionDetailDto>.Failure("Collection not found"));

        return Ok(Result<CollectionDetailDto>.Success(collection.ToDetailDto()));
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<Result<CollectionSummaryDto>>> CreateCollection([FromBody] CollectionCreateDto createCollectionDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized(Result<CollectionSummaryDto>.Failure("User not authenticated"));

        var result = await collectionRepo.CreateAsync(createCollectionDto, userId);
        return Ok(result);
    }

    [HttpPut("{id:int}")]
    [Authorize]
    public async Task<ActionResult<Result<CollectionSummaryDto>>> UpdateCollection([FromRoute] int id, [FromBody] CollectionUpdateDto updateCollectionDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var collection = await collectionRepo.GetByIdAsync(id);
        if (collection == null)
            return NotFound(Result<CollectionSummaryDto>.Failure("Collection not found"));

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized(Result<CollectionSummaryDto>.Failure("User not authenticated"));

        if (collection.CreatorId != userId)
            return BadRequest(Result<CollectionSummaryDto>.Failure("You do not have permission to update this collection"));

        var result = await collectionRepo.UpdateAsync(id, updateCollectionDto);
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<ActionResult<Result<CollectionSummaryDto>>> DeleteCollection([FromRoute] int id)
    {
        var collection = await collectionRepo.GetByIdAsync(id);
        if (collection == null)
            return NotFound(Result<CollectionSummaryDto>.Failure("Collection not found"));

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized(Result<CollectionSummaryDto>.Failure("User not authenticated"));

        if (collection.CreatorId != userId)
            return BadRequest(Result<CollectionSummaryDto>.Failure("You do not have permission to delete this collection"));

        var result = await collectionRepo.DeleteAsync(id);
        return Ok(result);
    }

    [HttpGet("{id:int}/nfts")]
    public async Task<ActionResult<Result<List<NftSummaryDto>>>> GetCollectionNfts([FromRoute] int id, [FromQuery] NftQueryDto query)
    {
        var nfts = await collectionRepo.GetNftsAsync(id, query);
        if (nfts == null)
            return NotFound(Result<List<NftSummaryDto>>.Failure("Collection not found"));

        var nftsDtos = nfts.Select(n => n.ToSummaryDto()).ToList();

        return Ok(Result<List<NftSummaryDto>>.Success(nftsDtos));
    }

    [HttpGet("my-collections")]
    [Authorize]
    public async Task<ActionResult<Result<List<CollectionSummaryDto>>>> GetCollectionsByUser()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized(Result<List<CollectionSummaryDto>>.Failure("User not authenticated"));

        var collections = (await collectionRepo.GetAllByUserAsync(userId))
                .Select(c => c.ToSummaryDto()).ToList();

        return Ok(Result<List<CollectionSummaryDto>>.Success(collections));
    }
}
