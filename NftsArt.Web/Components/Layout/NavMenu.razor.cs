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

        var authState = await AuthStateProvider.GetAuthenticationStateAsync();
        var userId = authState.User.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
        if (userId != null)
        {
            await LoadUser(userId); 
        }
    }

    private async Task LoadUser(string userId)
    {
        var res = await ApiClient.GetFromJsonAsync<UserDetailDto>($"api/auth/user/{userId}");

        if (res.IsSuccess && res.Data != null)
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
