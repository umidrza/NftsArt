using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NftsArt.BL.Repositories;
using NftsArt.BL.Services;
using NftsArt.Model.Dtos.User;
using NftsArt.Model.Entities;
using System.Security.Claims;
using System.Web;
using NftsArt.Model.Mapping;
using NftsArt.Model.Helpers;
using Microsoft.EntityFrameworkCore;

namespace NftsArt.ApiService.Controllers;

[Route("api/auth")]
[ApiController]
public class AuthController(
        UserManager<User> userManager,
        ITokenService tokenService,
        IEmailService emailService,
        IUserRepository userRepo) : ControllerBase
{

    [HttpPost("login")]
    public async Task<ActionResult<Result>> Login([FromBody] LoginDto loginDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await userManager.FindByNameAsync(loginDto.UserName.ToLower());
        if (user == null)
            return BadRequest(Result.Failure("Invalid username!"));

        var isPasswordValid = await userManager.CheckPasswordAsync(user, loginDto.Password);
        if (!isPasswordValid)
        {
            await userManager.AccessFailedAsync(user);

            if (await userManager.IsLockedOutAsync(user))
                return BadRequest(Result.Failure($"User is locked out due to multiple failed login attempts. Please try again after {user.LockoutEnd?.LocalDateTime}"));

            return BadRequest(Result.Failure("Invalid username or password!"));
        }

        await userManager.ResetAccessFailedCountAsync(user);

        if (user.LockoutEnd.HasValue && user.LockoutEnd.Value <= DateTime.UtcNow)
        {
            user.LockoutEnd = null;
            await userManager.UpdateAsync(user);
        }

        var jwtToken = await tokenService.CreateTokenAsync(user);

        return Ok(Result.Success(new LoginResponseDto(user.UserName!, user.Email!, jwtToken)));
    }


    [HttpPost("register")]
    public async Task<ActionResult<Result>> Register([FromBody] RegisterDto registerDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = registerDto.ToEntity();

        var createdUser = await userManager.CreateAsync(user, registerDto.Password);
        if (!createdUser.Succeeded)
            return BadRequest(Result.Failure(createdUser.Errors.ToString() ?? "Unknown error"));

        var roleResult = await userManager.AddToRoleAsync(user, "User");
        if (!roleResult.Succeeded)
            return BadRequest(Result.Failure(roleResult.Errors.ToString() ?? "Unknown error"));

        string emailToken = await userManager.GenerateEmailConfirmationTokenAsync(user);

        string? link = Url.Action(nameof(VerifyEmail), "auth", new { email = user.Email!, token = emailToken },
            Request.Scheme, Request.Host.ToString());

        var verifyUrl = $"http://localhost:5179/api/auth/verify-email?email={user.Email}&token={HttpUtility.UrlEncode(emailToken)}";
        await Console.Out.WriteLineAsync(verifyUrl);

        string subject = "Verify Email";
        string message = $"Please verify your email by clicking here: {verifyUrl}";
        var emailResult = await emailService.SendEmailAsync(user.Email!, subject, message);

        var jwtToken = await tokenService.CreateTokenAsync(user);
        var loginResponse = new LoginResponseDto(user.UserName!, user.Email!, jwtToken);

        if (!emailResult.IsSuccess)
            return Ok(Result.Success(loginResponse, "Successful Login, Unsuccessful Email confirmation"));

        return Ok(Result.Success(loginResponse, "Successful Login"));
    }


    [HttpGet("verify-email")]
    public async Task<ActionResult<Result>> VerifyEmail(string token, string email)
    {
        User? user = await userManager.FindByEmailAsync(email);
        if (user == null) 
            return NotFound(Result.Failure("User not found"));

        await userManager.ConfirmEmailAsync(user, token);

        return Ok(Result.Success(user, "Email confirmed successfully"));
    }


    [HttpGet("collector")]
    public async Task<IActionResult> GetTopCollectors()
    {
        var users = (await userManager.Users
            .Include(u => u.Avatar)
            .Include(u => u.Auctions)
            .Include(u => u.Collections)
            .Include(u => u.Followers)
            .OrderByDescending(u => u.Auctions.Sum(a => a.Price))
            .Take(12)
            .ToListAsync())
                    .Select(u => u.ToDetailDto());

        return Ok(Result.Success(users));
    }


    [HttpGet("user/{id}")]
    public async Task<ActionResult<Result>> GetUser([FromRoute] string id)
    {
        var user = await userManager.Users
            .Include(u => u.Avatar)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
            return Unauthorized(Result.Failure("User not authorized!"));

        return Ok(Result.Success(user.ToDetailDto()));
    }


    [Authorize]
    [HttpGet("profile")]
    public async Task<ActionResult<Result>> GetProfile()
    {
        var user = await userManager.GetUserAsync(User);

        if (user == null)
            return Unauthorized(Result.Failure("User not authorized!"));

        return Ok(Result.Success(user.ToDetailDto()));
    }


    [Authorize]
    [HttpPut("profile")]
    public async Task<ActionResult<Result>> UpdateProfile([FromBody] UserUpdateDto updatedUser)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized(Result.Failure("User not authenticated!"));

        user.UpdateEntity(updatedUser);
        var updatedResult = await userManager.UpdateAsync(user);

        if (!updatedResult.Succeeded)
            return BadRequest(Result.Failure(updatedResult.Errors.ToString() ?? "Unknown error"));

        var jwtToken = await tokenService.CreateTokenAsync(user);
        var loginResponse = new LoginResponseDto(user.UserName!, user.Email!, jwtToken);

        return Ok(Result.Success(loginResponse, "Profile updated successfully"));
    }


    [HttpDelete("user/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound(Result.Failure("User not found!"));

        var result = await userManager.DeleteAsync(user);
        if (!result.Succeeded)
            return BadRequest(result);

        return Ok(result);
    }


    [HttpPut("user/{id}/update-password")]
    [Authorize]
    public async Task<ActionResult<Result>> UpdatePassword([FromRoute] string id, [FromBody] UpdatePasswordDto updatePasswordDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (updatePasswordDto.NewPassword != updatePasswordDto.ConfirmPassword)
            return BadRequest(Result.Failure("New password and confirmation do not match."));

        var user = await userManager.FindByIdAsync(id);
        if (user == null)
            return Unauthorized(Result.Failure("User not authorized"));

        var changePasswordResult = await userManager.ChangePasswordAsync(user, updatePasswordDto.CurrentPassword, updatePasswordDto.NewPassword);
        if (!changePasswordResult.Succeeded)
            return BadRequest(Result.Failure(changePasswordResult.Errors.ToString() ?? "Unknown error"));

        return Ok(Result.Success(user, "Password has been updated successfully"));
    }


    [HttpPost("forgot-password")]
    public async Task<ActionResult<Result>> ForgotPassword(string email)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
            return NotFound(Result.Failure("User not found"));

        string token = await userManager.GeneratePasswordResetTokenAsync(user);
        string resetUrl = $"http://localhost:5179/api/auth/reset-password-with-token?email={user.Email}&token={HttpUtility.UrlEncode(token)}";

        string otpCode = new Random().Next(000001, 999999).ToString();
        user.ResetPasswordOtp = otpCode;
        user.OtpExpiryTime = DateTime.UtcNow.AddMinutes(15);
        await userManager.UpdateAsync(user);

        string subject = "Reset Your Password";
        string message = $"Please reset your password by clicking here: {resetUrl} or enter verification code {otpCode}";
        Console.WriteLine(message);

        var result = await emailService.SendEmailAsync(user.Email!, subject, message);
        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(Result.Success(user, "A password reset link has been sent to your email"));
    }


    [HttpPost("reset-password-with-token")]
    public async Task<ActionResult<Result>> ResetPasswordWithToken(string email, string token, [FromBody] ResetPasswordDto resetPasswordDto)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
            return BadRequest(Result.Failure("Email and Token required"));

        if (resetPasswordDto.NewPassword != resetPasswordDto.ConfirmPassword)
            return BadRequest(Result.Failure("New password and confirmation do not match."));

        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
            return NotFound(Result.Failure("User Not Found"));

        token = HttpUtility.UrlDecode(token);

        var result = await userManager.ResetPasswordAsync(user, token, resetPasswordDto.NewPassword);
        if (!result.Succeeded)
            return BadRequest(Result.Failure(result.Errors.ToString() ?? "Unknown error"));

        await userManager.UpdateSecurityStampAsync(user);
        return Ok(Result.Success(user, "Password has been updated successfully"));
    }


    [HttpPost("verify-otp")]
    public async Task<ActionResult<Result>> VerifyOtp(string email, string otpCode)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
            return NotFound(Result.Failure("User not found"));

        if (user.ResetPasswordOtp != otpCode || DateTime.UtcNow > user.OtpExpiryTime)
            return BadRequest(Result.Failure("Invalid OTP code"));

        return Ok(Result.Success(user, "Valid OTP"));
    }


    [HttpPost("reset-password-with-otp")]
    public async Task<ActionResult<Result>> ResetPasswordWithOtp(string email, string otpCode, [FromBody] ResetPasswordDto resetPasswordDto)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(otpCode))
            return BadRequest(Result.Failure("Email and OTP code required"));

        if (resetPasswordDto.NewPassword != resetPasswordDto.ConfirmPassword)
            return BadRequest(Result.Failure("New password and confirmation do not match."));

        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
            return BadRequest(Result.Failure("User Not Found"));

        if (user.ResetPasswordOtp != otpCode || user.OtpExpiryTime < DateTime.UtcNow)
            return BadRequest(Result.Failure("Invalid OTP code"));

        var removeResult = await userManager.RemovePasswordAsync(user);
        if (!removeResult.Succeeded)
            return BadRequest(Result.Failure(removeResult.Errors.ToString() ?? "Unknown error"));

        var addResult = await userManager.AddPasswordAsync(user, resetPasswordDto.NewPassword);
        if (!addResult.Succeeded)
            return BadRequest(Result.Failure(addResult.Errors.ToString() ?? "Unknown error"));

        user.ResetPasswordOtp = null;
        user.OtpExpiryTime = null;
        await userManager.UpdateAsync(user);

        return Ok(Result.Success(user, "Password has been updated successfully"));
    }


    [Authorize]
    [HttpPost("follow")]
    public async Task<ActionResult<Result>> Follow(FollowDto followDto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) 
            return Unauthorized(Result.Failure("User not authorized"));

        if (userId == followDto.FollowingUserId)
            return BadRequest(Result.Failure("You cannot follow yourself."));

        var result = await userRepo.FollowUser(userId, followDto.FollowingUserId);
        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }


    [Authorize]
    [HttpGet("isfollowing/{userId}")]
    public async Task<ActionResult<Result>> IsFollowing(string userId)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId == null)
            return Unauthorized(Result.Failure("User not authorized"));

        var isFollowing = await userRepo.IsFollowing(currentUserId, userId);
        return Ok(Result.Success(isFollowing));
    }
}
