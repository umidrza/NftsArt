using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.JSInterop;
using NftsArt.Model.Dtos.User;

namespace NftsArt.Web.Components.Layout;

public partial class NavMenu
{
    [Inject] public ApiClient ApiClient { get; set; }
    [Inject] AuthenticationStateProvider AuthStateProvider { get; set; }
    [Inject] IJSRuntime JS { get; set; }

    public UserDetailDto? User { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await LoadUser(); 
    }

    private async Task LoadUser()
    {
        var authState = await AuthStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        if (user.Identity?.IsAuthenticated == false) return;

        var res = await ApiClient.GetFromJsonAsync<UserDetailDto>($"api/auth/profile");

        if (res != null && res.IsSuccess && res.Data != null)
        {
            User = res.Data;
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await JS.InvokeVoidAsync("NavMenuScript");
        }
    }
}
