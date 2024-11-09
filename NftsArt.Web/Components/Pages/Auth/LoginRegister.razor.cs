using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using NftsArt.Model.Dtos.Avatar;
using NftsArt.Model.Dtos.User;
using NftsArt.Model.Helpers;
using NftsArt.Web.Authentication;
using NftsArt.BL.Services;

namespace NftsArt.Web.Components.Pages.Auth;

public partial class LoginRegister
{
    [Inject] ApiClient ApiClient { get; set; }
    [Inject] NavigationManager Navigation { get; set; }
    [Inject] AuthenticationStateProvider AuthStateProvider { get; set; }
    [Inject] MessageService MessageService { get; set; }


    [SupplyParameterFromForm(FormName = "Register")]
    private RegisterDto RegisterDto { get; set; } = new RegisterDto();

    [SupplyParameterFromForm(FormName = "Login")]
    private LoginDto LoginDto { get; set; } = new LoginDto();


    private List<AvatarSummaryDto>? Avatars { get; set; }
    private string? selectedAvatar;

    private bool isPasswordVisible = false;
    private bool isLoginPopupVisible = false;
    private bool isForgotPasswordPopupVisible = false;
    private bool isVerifyCodePopupVisible = false;
    private bool isResetPasswordPopupVisible = false;

    private string? loginError;
    private string? registerError;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await LoadAvatars();

        if (Avatars != null)
        {
            var firstAvatar = Avatars.FirstOrDefault();
            if (firstAvatar != null)
                SelectAvatar(firstAvatar);
        }
    }

    protected async Task LoadAvatars()
    {
        var res = await ApiClient.GetFromJsonAsync<List<AvatarSummaryDto>>($"api/avatar");

        if (res != null && res.IsSuccess && res.Data != null)
        {
            Avatars = res.Data;
        }
    }


    private async Task HandleRegister()
    {
        var res = await ApiClient.PostAsync<LoginResponseDto, RegisterDto>("/api/auth/register", RegisterDto);

        if (res != null && res.IsSuccess && res.Data != null)
        {
            await ((CustomAuthStateProvider)AuthStateProvider).MarkUserAsAuthenticated(res.Data);
            MessageService.ShowMessage(Message.Success("You are logged in"));
            Navigation.NavigateTo("/");
        }
        else
        {
            registerError = res != null ? res.Message : "Register Error";
        }
    }

    private async Task HandleLogin()
    {
        var res = await ApiClient.PostAsync<LoginResponseDto, LoginDto>("/api/auth/login", LoginDto);

        if (res != null && res.IsSuccess && res.Data != null)
        {
            await ((CustomAuthStateProvider)AuthStateProvider).MarkUserAsAuthenticated(res.Data);
            MessageService.ShowMessage(Message.Success("You are logged in"));
            Navigation.NavigateTo("/");
        }
        else
        {
            loginError = res != null ? res.Message : "Error logging you in";
        }
    }


    private void SelectAvatar(AvatarSummaryDto avatar)
    {
        selectedAvatar = avatar.ImageUrl;
        RegisterDto.AvatarId = avatar.Id;
    }


    // Reset Password

    [SupplyParameterFromForm(FormName = "ForgotPassword")]
    private ForgotPasswordDto ForgotPasswordDto { get; set; } = new ForgotPasswordDto();

    [SupplyParameterFromForm(FormName = "VerifyCode")]
    private VerifyCodeDto VerifyCodeDto { get; set; } = new VerifyCodeDto();

    [SupplyParameterFromForm(FormName = "ResetPassword")]
    private ResetPasswordDto ResetPasswordDto { get; set; } = new ResetPasswordDto();


    private async Task SubmitForgotPassword()
    {
        var res = await ApiClient.PostAsync<UserSummaryDto>($"api/auth/forgot-password?email={ForgotPasswordDto.Email}", null!);
        
        if (res != null && res.IsSuccess && res.Data != null)
        {
            MessageService.ShowMessage(Message.Success(res.Message));
            isVerifyCodePopupVisible = true;
        }
        else
        {
            MessageService.ShowMessage(Message.Error(res?.Message ?? "Error"));
        }
    }

    private async Task SubmitResetPassword()
    {
        var res = await ApiClient.PostAsync<UserSummaryDto, ResetPasswordDto>($"api/auth/reset-password-with-otp?email={ForgotPasswordDto.Email}&otpCode={VerifyCodeDto.OtpCode}", ResetPasswordDto);

        if (res != null && res.IsSuccess && res.Data != null)
        {
            MessageService.ShowMessage(Message.Success(res.Message));
            Navigation.NavigateTo("/");
        }
        else
        {
            MessageService.ShowMessage(Message.Error(res?.Message ?? "Error"));
        }
    }

    private async Task SubmitVerifyCode() 
    {
        var res = await ApiClient.PostAsync<UserSummaryDto>($"api/auth/verify-otp?email={ForgotPasswordDto.Email}&otpCode={VerifyCodeDto.OtpCode}", null!);

        if (res != null && res.IsSuccess && res.Data != null)
        {
            isResetPasswordPopupVisible = true;
        }
        else
        {
            MessageService.ShowMessage(Message.Error(res?.Message ?? "Invalid Code"));  
        }
    }
}
