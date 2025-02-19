﻿using Microsoft.AspNetCore.Authorization;
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
    public async Task<ActionResult<Result<Pagination<NftSummaryDto>>>> GetNfts([FromQuery] NftQueryDto query)
    {
        var nfts = await nftRepo.GetAllAsync(query);

        return Ok(Result<Pagination<NftSummaryDto>>.Success(nfts));
    }

    [HttpGet("collection/{collectionId:int}")]
    public async Task<ActionResult<Result<Pagination<NftSummaryDto>>>> GetCollectionNfts([FromRoute] int collectionId, [FromQuery] NftQueryDto query)
    {
        var nfts = await nftRepo.GetAllByCollectionIdAsync(collectionId, query);
        if (nfts == null)
            return NotFound(Result<Pagination<NftSummaryDto>>.Failure("Collection not found"));

        return Ok(Result<Pagination<NftSummaryDto>>.Success(nfts));
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
    public async Task<ActionResult<Result<IEnumerable<NftSummaryDto>>>> GetHomeNfts()
    {
        var nfts = await nftRepo.GetPopularsAsync();

        return Ok(Result<IEnumerable<NftSummaryDto>>.Success(nfts));
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
        return Ok(result);
    }

    [HttpGet("my-nfts")]
    [Authorize]
    public async Task<ActionResult<Result<IEnumerable<NftSummaryDto>>>> GetNftsByUser()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized(Result<IEnumerable<NftSummaryDto>>.Failure("User not authenticated"));

        var nfts = await nftRepo.GetAllByUserAsync(userId);

        return Ok(Result<IEnumerable<NftSummaryDto>>.Success(nfts));
    }

    [HttpPost("{id}/like")]
    [Authorize]
    public async Task<ActionResult<Result<Like>>> LikeNft([FromRoute] int id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) 
            return Unauthorized(Result<Like>.Failure("User not authenticated"));

        var result = await nftRepo.LikeAsync(userId, id);
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


    [HttpPost("upload")]
    public async Task<ActionResult<Result<string>>> UploadNftImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(Result<string>.Failure("Upload a file."));

        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        string fileUrl = $"{Request.Scheme}://{Request.Host}/uploads/{fileName}";
        return Ok(Result<string>.Success(fileUrl));
    }
}
