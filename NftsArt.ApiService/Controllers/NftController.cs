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
    public async Task<ActionResult<Result<List<NftSummaryDto>>>> GetNfts([FromQuery] NftQueryDto query)
    {
        var nfts = (await nftRepo.GetAllAsync(query))
                    .Select(c => c.ToSummaryDto()).ToList();

        return Ok(Result<List<NftSummaryDto>>.Success(nfts));
    }


    [HttpGet("{id:int}")]
    public async Task<ActionResult<Result<NftDetailDto>>> GetNft([FromRoute] int id)
    {
        var nft = await nftRepo.GetByIdAsync(id);
        if (nft == null) 
            return NotFound(Result<NftDetailDto>.Failure("NFT not found"));

        return Ok(Result<NftDetailDto>.Success(nft.ToDetailDto()));
    }

    [HttpGet("popular")]
    public async Task<ActionResult<Result<List<NftSummaryDto>>>> GetHomeNfts()
    {
        var nfts = (await nftRepo.GetPopularsAsync())
                    .Select(c => c.ToSummaryDto()).ToList();

        return Ok(Result<List<NftSummaryDto>>.Success(nfts));
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<Result<NftSummaryDto>>> CreateNft([FromBody] NftCreateDto createNftDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized(Result<NftSummaryDto>.Failure("User not authenticated"));

        var result = await nftRepo.CreateAsync(createNftDto, userId);
        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPut("{id:int}")]
    [Authorize]
    public async Task<ActionResult<Result<NftSummaryDto>>> UpdateNft([FromRoute] int id, [FromBody] NftUpdateDto updateNftDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var nft = await nftRepo.GetByIdAsync(id);
        if (nft == null)
            return NotFound(Result<NftSummaryDto>.Failure("Nft not found"));

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized(Result<NftSummaryDto>.Failure("User not authenticated"));

        if (nft.CreatorId != userId)
            return BadRequest(Result<NftSummaryDto>.Failure("You do not have permission to update this Nft"));

        var result = await nftRepo.UpdateAsync(id, updateNftDto);
        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<ActionResult<Result<NftSummaryDto>>> DeleteNft([FromRoute] int id)
    {
        var nft = await nftRepo.GetByIdAsync(id);
        if (nft == null)
            return NotFound(Result<NftSummaryDto>.Failure("Nft not found"));

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized(Result<NftSummaryDto>.Failure("User not authenticated"));

        if (nft.CreatorId != userId)
            return BadRequest(Result<NftSummaryDto>.Failure("You do not have permission to delete this Nft"));

        var result = await nftRepo.DeleteAsync(id);
        if (!result.IsSuccess)
            return NotFound(result);

        return Ok(result);
    }

    [HttpGet("my-nfts")]
    [Authorize]
    public async Task<ActionResult<Result<List<NftSummaryDto>>>> GetNftsByUser()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized(Result<List<NftSummaryDto>>.Failure("User not authenticated"));

        var nfts = (await nftRepo.GetAllByUserAsync(userId))
                .Select(c => c.ToSummaryDto()).ToList();

        return Ok(Result<List<NftSummaryDto>>.Success(nfts));
    }

    [HttpPost("{id}/like")]
    [Authorize]
    public async Task<ActionResult<Result<Like>>> LikeNft([FromRoute] int id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) 
            return Unauthorized(Result<Like>.Failure("User not authenticated"));

        var result = await nftRepo.LikeAsync(userId, id);
        if (!result.IsSuccess)
            return NotFound(result);

        return Ok(result);
    }

    [HttpGet("{id}/likes")]
    public async Task<ActionResult<Result<int>>> GetLikesCount([FromRoute] int id)
    {
        var count = await nftRepo.GetLikesCountAsync(id);
        return Ok(Result<int>.Success(count));
    }

    [HttpGet("{id}/is-liked")]
    [Authorize]
    public async Task<ActionResult<Result<bool>>> IsNftLiked([FromRoute] int id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized(Result<bool>.Failure("User not authorized"));

        var isLiked = await nftRepo.HasUserLikedAsync(userId, id);

        return Ok(Result<bool>.Success(isLiked));
    }
}
