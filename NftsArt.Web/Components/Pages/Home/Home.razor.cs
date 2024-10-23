using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using NftsArt.Model.Dtos.User;
using NftsArt.Model.Helpers;

namespace NftsArt.Web.Components.Pages.Home;

public partial class Home
{
    [Inject] ApiClient ApiClient { get; set; }
    [Inject] IJSRuntime JS { get; set; }

    private List<UserDetailDto>? Collectors { get; set; }

    private bool isTermsAccepted = true;


    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await LoadCollectors();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await JS.InvokeVoidAsync("InitScript");
        }
    }

    private async Task LoadCollectors()
    {
        var res = await ApiClient.GetFromJsonAsync<Result>($"api/auth/collector");

        if (res.IsSuccess && res.Data != null)
        {
            Collectors = JsonConvert.DeserializeObject<List<UserDetailDto>>(res.Data.ToString());
        }
    }

    private string GetNftUrl(int collectionId, int nftId)
    {
        return $"/collection/{collectionId}/nft/{nftId}";
    }

    private string GetCollectionUrl(int collectionId)
    {
        return $"/collection/{collectionId}";
    }
}
