using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using NftsArt.Model.Dtos.Avatar;
using NftsArt.Model.Dtos.User;
using NftsArt.Web.Authentication;

namespace NftsArt.Web.Components.Pages.Auth;

public partial class LoginRegister
{
    [Inject] ApiClient ApiClient { get; set; }
    [Inject] NavigationManager NavigationManager { get; set; }
    [Inject] AuthenticationStateProvider AuthStateProvider { get; set; }


    [SupplyParameterFromForm(FormName = "Register")]
    private RegisterDto registerDto { get; set; } = new RegisterDto();

    [SupplyParameterFromForm(FormName = "Login")]
    private LoginDto loginDto { get; set; } = new LoginDto();


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

        if (res.IsSuccess && res.Data != null)
        {
            Avatars = res.Data;
        }
    }


    private async Task HandleRegister()
    {
        var res = await ApiClient.PostAsync<LoginResponseDto, RegisterDto>("/api/auth/register", registerDto);
        if (!res.IsSuccess || res.Data == null)
        {
            registerError = res.Message;
            return;
        }

        await ((CustomAuthStateProvider)AuthStateProvider).MarkUserAsAuthenticated(res.Data);

        NavigationManager.NavigateTo("/");
    }

    private async Task HandleLogin()
    {
        var res = await ApiClient.PostAsync<LoginResponseDto, LoginDto>("/api/auth/login", loginDto);
        if (!res.IsSuccess || res.Data == null)
        {
            loginError = res.Message;
            return;
        }

        await ((CustomAuthStateProvider)AuthStateProvider).MarkUserAsAuthenticated(res.Data);

        NavigationManager.NavigateTo("/");
    }


    private void SelectAvatar(AvatarSummaryDto avatar)
    {
        selectedAvatar = avatar.ImageUrl;
        registerDto.AvatarId = avatar.Id;
    }

    private void ToggleLoginPopup()
    {
        isLoginPopupVisible = !isLoginPopupVisible;
    }
}
