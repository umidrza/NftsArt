﻿using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using NftsArt.Model.Dtos.Avatar;
using NftsArt.Model.Dtos.User;
using NftsArt.Web.Authentication;
using NftsArt.Model.Mapping;
using NftsArt.Model.Helpers;
using NftsArt.Web.Services;

namespace NftsArt.Web.Components.Pages.Auth;

public partial class UpdateProfile
{
    [Inject] ApiClient ApiClient { get; set; }
    [Inject] NavigationManager NavigationManager { get; set; }
    [Inject] AuthenticationStateProvider AuthStateProvider { get; set; }
    [Inject] MessageService MessageService { get; set; }


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
        var res = await ApiClient.GetFromJsonAsync<UserDetailDto>($"api/auth/profile");

        if (res != null && res.IsSuccess && res.Data != null)
        {
            User = res.Data;
            UpdateDto = User.ToUpdateDto();
        }
        else
        {
            MessageService.ShowMessage(Message.Error(res?.Message ?? "Error"));
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

            MessageService.ShowMessage(Message.Success(res.Message));
        }
        else
        {
            updateProfileError = res != null ? res.Message : "Update Profile Error";
        }
    }

    private async Task HandleLogout()
    {
        await ((CustomAuthStateProvider)AuthStateProvider).MarkUserAsLoggedOut();

        MessageService.ShowMessage(Message.Success("You've been logged out"));
        NavigationManager.NavigateTo("/");
    }
}
