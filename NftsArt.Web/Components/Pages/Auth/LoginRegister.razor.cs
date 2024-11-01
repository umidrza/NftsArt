using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using NftsArt.Model.Dtos.Avatar;
using NftsArt.Model.Dtos.User;
using NftsArt.Model.Helpers;
using NftsArt.Web.Authentication;
using NftsArt.Web.Services;

namespace NftsArt.Web.Components.Pages.Auth;

public partial class LoginRegister
{
    [Inject] ApiClient ApiClient { get; set; }
    [Inject] NavigationManager NavigationManager { get; set; }
    [Inject] AuthenticationStateProvider AuthStateProvider { get; set; }
    [Inject] MessageService MessageService { get; set; }


    [SupplyParameterFromForm(FormName = "Register")]
    private RegisterDto RegisterDto { get; set; } = new RegisterDto();

    [SupplyParameterFromForm(FormName = "Login")]
    private LoginDto LoginDto { get; set; } = new LoginDto();


    private List<AvatarSummaryDto>? Avatars { get; set; }
    private string? selectedAvatar;

    private bool isLoginPopupVisible = false;
    private bool isPasswordVisible = false;

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
            NavigationManager.NavigateTo("/");
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
            NavigationManager.NavigateTo("/");
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

    private void ToggleLoginPopup()
    {
        isLoginPopupVisible = !isLoginPopupVisible;
    }
}
