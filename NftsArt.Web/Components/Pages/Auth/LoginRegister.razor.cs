using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Newtonsoft.Json;
using NftsArt.Model.Dtos.Avatar;
using NftsArt.Model.Dtos.Collection;
using NftsArt.Model.Dtos.User;
using NftsArt.Model.Helpers;
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
        var res = await ApiClient.GetFromJsonAsync<Result>($"api/avatar");

        if (res.IsSuccess && res.Data != null)
        {
            Avatars = JsonConvert.DeserializeObject<List<AvatarSummaryDto>>(res.Data.ToString());
        }
    }


    private async Task HandleRegister()
    {
        var result = await ApiClient.PostAsync<Result, RegisterDto>("/api/auth/register", registerDto);
        if (!result.IsSuccess || result.Data == null)
        {
            registerError = result.Message;
            return;
        }

        LoginResponseDto loginResponse = JsonConvert.DeserializeObject<LoginResponseDto>(result.Data.ToString());

        await ((CustomAuthStateProvider)AuthStateProvider).MarkUserAsAuthenticated(loginResponse);

        NavigationManager.NavigateTo("/");
    }

    private async Task HandleLogin()
    {
        var result = await ApiClient.PostAsync<Result, LoginDto>("/api/auth/login", loginDto);
        if (!result.IsSuccess || result.Data == null)
        {
            loginError = result.Message;
            return;
        }

        LoginResponseDto loginResponse = JsonConvert.DeserializeObject<LoginResponseDto>(result.Data.ToString());

        await ((CustomAuthStateProvider)AuthStateProvider).MarkUserAsAuthenticated(loginResponse);

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
