using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using NftsArt.Model.Dtos.Avatar;
using NftsArt.Model.Dtos.User;
using NftsArt.Web.Authentication;
using NftsArt.Model.Mapping;
using System.IdentityModel.Tokens.Jwt;

namespace NftsArt.Web.Components.Pages.Auth;

public partial class UpdateProfile
{
    [Inject] ApiClient ApiClient { get; set; }
    [Inject] NavigationManager NavigationManager { get; set; }
    [Inject] AuthenticationStateProvider AuthStateProvider { get; set; }


    [SupplyParameterFromForm(FormName = "UpdateProfile")]
    private UserUpdateDto UpdateDto { get; set; } = new UserUpdateDto();

    private UserDetailDto? User { get; set; }

    private List<AvatarSummaryDto>? Avatars { get; set; }
    private string? selectedAvatar;

    private string? updateProfileError;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await LoadUser();
        await LoadAvatars();

        if (User != null && Avatars != null)
        {
            var avatar = User.Avatar != null ? User.Avatar : Avatars.FirstOrDefault();
            if (avatar != null)
                SelectAvatar(avatar);
        }
    }

    protected async Task LoadUser()
    {
        var authState = await AuthStateProvider.GetAuthenticationStateAsync();
        var userId = authState.User.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
        if (userId == null) return;

        var res = await ApiClient.GetFromJsonAsync<UserDetailDto>($"api/auth/user/{userId}");

        if (res != null && res.IsSuccess && res.Data != null)
        {
            User = res.Data;
            UpdateDto = User.ToUpdateDto();
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

    private void SelectAvatar(AvatarSummaryDto avatar)
    {
        selectedAvatar = avatar.ImageUrl;
        UpdateDto.AvatarId = avatar.Id;
    }


    private async Task HandleSubmit()
    {
        var res = await ApiClient.PutAsync<LoginResponseDto, UserUpdateDto>("/api/auth/profile", UpdateDto);
        if (res != null && res.IsSuccess && res.Data != null)
        {
            await ((CustomAuthStateProvider)AuthStateProvider).MarkUserAsAuthenticated(res.Data);
            NavigationManager.NavigateTo("/");
        }
        else
        {
            updateProfileError = res != null ? res.Message : "Update Profile Error";
        }
    }

    private async Task HandleLogout()
    {
        await ((CustomAuthStateProvider)AuthStateProvider).MarkUserAsLoggedOut();

        NavigationManager.NavigateTo("/");
    }
}
