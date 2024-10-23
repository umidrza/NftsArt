using Microsoft.EntityFrameworkCore;
using NftsArt.Database.Data;
using NftsArt.Model.Entities;
using NftsArt.Model.Helpers;

namespace NftsArt.BL.Repositories;


public interface IUserRepository
{
    Task<Result<Follow>> FollowUser(string followerId, string followingId);
    Task<bool> IsFollowing(string followerId, string followingId);
}

public class UserRepository(AppDbContext context) : IUserRepository
{
    public async Task<Result<Follow>> FollowUser(string followerId, string followingId)
    {
        var follow = await context.Follows
            .FirstOrDefaultAsync(f => f.FollowerId == followerId && f.FollowingId == followingId);

        if (follow == null || follow.IsDeleted)
        {
            if (follow == null)
            {
                follow = new Follow { FollowerId = followerId, FollowingId = followingId };
                context.Follows.Add(follow);
            }
            else
            {
                follow.IsDeleted = false;
            }

            await context.SaveChangesAsync();
            return Result<Follow>.Success(follow, "User followed successfully.");
        }
        else
        {
            follow.IsDeleted = true;

            await context.SaveChangesAsync();
            return Result<Follow>.Success(follow, "User unfollowed successfully.");
        }
    }

    public async Task<bool> IsFollowing(string followerId, string followingId)
    {
        return await context.Follows.AnyAsync(
            f => f.FollowerId == followerId && 
            f.FollowingId == followingId &&
            f.IsDeleted == false);
    }
}