using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using NftsArt.Model.Dtos.Avatar;
using NftsArt.Model.Dtos.User;
using NftsArt.Model.Helpers;
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

        var res = await ApiClient.GetFromJsonAsync<Result>($"api/auth/user/{userId}");

        if (res.IsSuccess && res.Data != null)
        {
            User = JsonConvert.DeserializeObject<UserDetailDto>(res.Data.ToString());

            if (User != null)
            {
                UpdateDto = User.ToUpdateDto();
            }
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

    private void SelectAvatar(AvatarSummaryDto avatar)
    {
        selectedAvatar = avatar.ImageUrl;
        UpdateDto.AvatarId = avatar.Id;
    }


    private async Task HandleSubmit()
    {
        var result = await ApiClient.PutAsync<Result, UserUpdateDto>("/api/auth/profile", UpdateDto);
        if (!result.IsSuccess || result.Data == null)
        {
            updateProfileError = result.Message;
            return;
        }

        LoginResponseDto loginResponse = JsonConvert.DeserializeObject<LoginResponseDto>(result.Data.ToString());

        await ((CustomAuthStateProvider)AuthStateProvider).MarkUserAsAuthenticated(loginResponse);

        NavigationManager.NavigateTo("/");
    }

    private async Task HandleLogout()
    {
        await ((CustomAuthStateProvider)AuthStateProvider).MarkUserAsLoggedOut();

        NavigationManager.NavigateTo("/");
    }
}
