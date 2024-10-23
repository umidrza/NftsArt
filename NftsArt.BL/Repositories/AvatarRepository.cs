using Microsoft.EntityFrameworkCore;
using NftsArt.Database.Data;
using NftsArt.Model.Dtos.Avatar;
using NftsArt.Model.Entities;
using NftsArt.Model.Mapping;
using NftsArt.Model.Helpers;

namespace NftsArt.BL.Repositories;

public interface IAvatarRepository
{
    Task<List<Avatar>> GetAllAsync();
    Task<Avatar?> GetByIdAsync(int id);
    Task<Result<AvatarSummaryDto>> CreateAsync(AvatarCreateDto avatarDto);
    Task<Result<AvatarSummaryDto>> UpdateAsync(int id, AvatarCreateDto avatarDto);
    Task<Result<AvatarSummaryDto>> DeleteAsync(int id);
}

public class AvatarRepository(AppDbContext context) : IAvatarRepository
{
    public async Task<List<Avatar>> GetAllAsync()
    {
        return await context.Avatars
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Avatar?> GetByIdAsync(int id)
    {
        return await context.Avatars
            .FirstOrDefaultAsync(n => n.Id == id);
    }

    public async Task<Result<AvatarSummaryDto>> CreateAsync(AvatarCreateDto avatarCreateDto)
    {
        Avatar avatar = new Avatar() { Url = avatarCreateDto.ImageUrl };

        await context.Avatars.AddAsync(avatar);
        await context.SaveChangesAsync();

        return Result<AvatarSummaryDto>.Success(avatar.ToSummaryDto(), "Avatar created successfully");
    }

    public async Task<Result<AvatarSummaryDto>> UpdateAsync(int id, AvatarCreateDto updatedAvatar)
    {
        var avatar = await context.Avatars.FindAsync(id);
        if (avatar == null)
            return Result<AvatarSummaryDto>.Failure("Avatar not found.");

        avatar.UpdateEntity(updatedAvatar);

        await context.SaveChangesAsync();

        return Result<AvatarSummaryDto>.Success(avatar.ToSummaryDto(), "Avatar updated successfully.");
    }

    public async Task<Result<AvatarSummaryDto>> DeleteAsync(int id)
    {
        var avatar = await context.Avatars.FindAsync(id);
        if (avatar == null)
            return Result<AvatarSummaryDto>.Failure("Avatar not found.");

        context.Avatars.Remove(avatar);
        await context.SaveChangesAsync();

        return Result<AvatarSummaryDto>.Success(null!, "Avatar deleted successfully.");
    }
}