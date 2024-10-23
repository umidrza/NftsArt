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
    public async Task<ActionResult<Result>> GetCollections([FromQuery] CollectionQueryDto query)
    {
        var collections = (await collectionRepo.GetAllAsync(query))
                .Select(c => c.ToDetailDto());

        return Ok(Result.Success(collections));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Result>> GetCollection([FromRoute] int id)
    {
        var collection = await collectionRepo.GetByIdAsync(id);
        if (collection == null) 
            return NotFound(Result.Failure("Collection not found"));

        return Ok(Result.Success(collection.ToDetailDto()));
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<Result>> CreateCollection([FromBody] CollectionCreateDto createCollectionDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized(Result.Failure("User not authenticated"));

        var result = await collectionRepo.CreateAsync(createCollectionDto, userId);
        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPut("{id:int}")]
    [Authorize]
    public async Task<ActionResult<Result>> UpdateCollection([FromRoute] int id, [FromBody] CollectionUpdateDto updateCollectionDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var collection = await collectionRepo.GetByIdAsync(id);
        if (collection == null)
            return NotFound(Result.Failure("Collection not found"));

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized(Result.Failure("User not authenticated"));

        if (collection.CreatorId != userId)
            return BadRequest(Result.Failure("You do not have permission to update this collection"));

        var result = await collectionRepo.UpdateAsync(id, updateCollectionDto);
        if (!result.IsSuccess) 
            return BadRequest(result);

        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<ActionResult<Result>> DeleteCollection([FromRoute] int id)
    {
        var collection = await collectionRepo.GetByIdAsync(id);
        if (collection == null)
            return NotFound(Result.Failure("Collection not found"));

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized(Result.Failure("User not authenticated"));

        if (collection.CreatorId != userId)
            return BadRequest(Result.Failure("You do not have permission to delete this collection"));

        var result = await collectionRepo.DeleteAsync(id);
        if (!result.IsSuccess) 
            return NotFound(result);

        return Ok(result);
    }

    [HttpGet("{id:int}/nfts")]
    public async Task<ActionResult<Result>> GetCollectionNfts([FromRoute] int id, [FromQuery] NftQueryDto query)
    {
        var nfts = await collectionRepo.GetNftsAsync(id, query);
        if (nfts == null)
            return NotFound(Result.Failure("Collection not found"));

        var nftsDtos = nfts.Select(n => n.ToSummaryDto());

        return Ok(Result.Success(nftsDtos));
    }

    [HttpGet("my-collections")]
    [Authorize]
    public async Task<ActionResult<Result>> GetCollectionsByUser()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized(Result.Failure("User not authenticated"));

        var collections = (await collectionRepo.GetAllByUserAsync(userId))
                .Select(c => c.ToSummaryDto());

        return Ok(Result.Success(collections));
    }
}
