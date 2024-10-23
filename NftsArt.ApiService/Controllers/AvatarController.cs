using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NftsArt.Model.Mapping;
using NftsArt.BL.Repositories;
using NftsArt.Model.Dtos.Avatar;
using NftsArt.Model.Helpers;

namespace NftsArt.ApiService.Controllers;

[Route("api/avatar")]
[ApiController]
public class AvatarController(IAvatarRepository avatarRepo) : ControllerBase
{

    [HttpGet]
    public async Task<ActionResult<Result>> GetAvatars()
    {
        var avatars = (await avatarRepo.GetAllAsync())
                        .Select(a => a.ToSummaryDto());

        return Ok(Result.Success(avatars));
    }


    [HttpGet("{id:int}")]
    public async Task<ActionResult<Result>> GetAvatar([FromRoute] int id)
    {
        var avatar = await avatarRepo.GetByIdAsync(id);
        if (avatar == null) 
            return NotFound(Result.Failure("Avatar not found"));

        return Ok(Result.Success(avatar));
    }

    [HttpPost]
    public async Task<ActionResult<Result>> CreateAvatar([FromBody] AvatarCreateDto createAvatarDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await avatarRepo.CreateAsync(createAvatarDto);
        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPut("{id:int}")]
    [Authorize]
    public async Task<ActionResult<Result>> UpdateAvatar([FromRoute] int id, [FromBody] AvatarCreateDto updateAvatarDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await avatarRepo.UpdateAsync(id, updateAvatarDto);
        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Result>> DeleteAvatar([FromRoute] int id)
    {
        var result = await avatarRepo.DeleteAsync(id);
        if (!result.IsSuccess)
            return NotFound(result);

        return Ok(result);
    }
}
