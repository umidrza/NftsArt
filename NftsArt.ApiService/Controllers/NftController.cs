using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NftsArt.BL.Repositories;
using System.Security.Claims;
using NftsArt.Model.Mapping;
using NftsArt.Model.Dtos.Nft;
using NftsArt.Model.Helpers;
using NftsArt.Model.Entities;

namespace NftsArt.ApiService.Controllers;

[Route("api/nft")]
[ApiController]
public class NftController(INftRepository nftRepo) : ControllerBase
{

    [HttpGet]
    public async Task<ActionResult<Result>> GetNfts([FromQuery] NftQueryDto query)
    {
        var nfts = (await nftRepo.GetAllAsync(query))
                    .Select(c => c.ToSummaryDto());

        return Ok(Result.Success(nfts));
    }


    [HttpGet("{id:int}")]
    public async Task<ActionResult<Result>> GetNft([FromRoute] int id)
    {
        var nft = await nftRepo.GetByIdAsync(id);
        if (nft == null) 
            return NotFound(Result.Failure("NFT not found"));

        return Ok(Result.Success(nft.ToDetailDto()));
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<Result>> CreateNft([FromBody] NftCreateDto createNftDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized(Result.Failure("User not authenticated"));

        var result = await nftRepo.CreateAsync(createNftDto, userId);
        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPut("{id:int}")]
    [Authorize]
    public async Task<ActionResult<Result>> UpdateNft([FromRoute] int id, [FromBody] NftUpdateDto updateNftDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var nft = await nftRepo.GetByIdAsync(id);
        if (nft == null)
            return NotFound(Result.Failure("Nft not found"));

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized(Result.Failure("User not authenticated"));

        if (nft.CreatorId != userId)
            return BadRequest(Result.Failure("You do not have permission to update this Nft"));

        var result = await nftRepo.UpdateAsync(id, updateNftDto);
        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<ActionResult<Result>> DeleteNft([FromRoute] int id)
    {
        var nft = await nftRepo.GetByIdAsync(id);
        if (nft == null)
            return NotFound(Result.Failure("Nft not found"));

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized(Result.Failure("User not authenticated"));

        if (nft.CreatorId != userId)
            return BadRequest(Result.Failure("You do not have permission to delete this Nft"));

        var result = await nftRepo.DeleteAsync(id);
        if (!result.IsSuccess)
            return NotFound(result);

        return Ok(result);
    }

    [HttpGet("my-nfts")]
    [Authorize]
    public async Task<ActionResult<Result>> GetNftsByUser()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized(Result.Failure("User not authenticated"));

        var nfts = (await nftRepo.GetAllByUserAsync(userId))
                .Select(c => c.ToSummaryDto());

        return Ok(Result.Success(nfts));
    }

    [HttpPost("{id}/like")]
    [Authorize]
    public async Task<ActionResult<Result>> LikeNft([FromRoute] int id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) 
            return Unauthorized(Result.Failure("User not authenticated"));

        var result = await nftRepo.LikeAsync(userId, id);
        if (!result.IsSuccess)
            return NotFound(result);

        return Ok(result);
    }

    [HttpGet("{id}/likes")]
    public async Task<ActionResult<Result>> GetLikesCount([FromRoute] int id)
    {
        var count = await nftRepo.GetLikesCountAsync(id);
        return Ok(Result.Success(count));
    }

    [HttpGet("{id}/is-liked")]
    [Authorize]
    public async Task<ActionResult<Result>> IsNftLiked([FromRoute] int id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized(Result.Failure("User not authorized"));

        var isLiked = await nftRepo.HasUserLikedAsync(userId, id);

        return Ok(Result.Success(isLiked));
    }
}
