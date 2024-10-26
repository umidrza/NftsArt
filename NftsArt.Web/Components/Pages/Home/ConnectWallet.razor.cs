using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using NftsArt.Model.Dtos.Provider;
using NftsArt.Model.Dtos.User;
using NftsArt.Model.Dtos.Wallet;
using NftsArt.Model.Enums;

namespace NftsArt.Web.Components.Pages.Home;

public partial class ConnectWallet
{
    [Inject] ApiClient ApiClient { get; set; }
    [Inject] IJSRuntime JS { get; set; }


    private List<ProviderSummaryDto>? Providers { get; set; }

    private ProviderSummaryDto? SelectedProvider { get; set; }
    private Blockchain? SelectedBlockchain { get; set; }
    private WalletSummaryDto? Wallet { get; set; }
    private UserDetailDto? User { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await LoadProviders();

        if (Providers != null)
        {
            await JS.InvokeVoidAsync("WalletScript");
        }
    }

    protected async Task LoadProviders()
    {
        var res = await ApiClient.GetFromJsonAsync<List<ProviderSummaryDto>>($"api/provider");

        if (res != null && res.IsSuccess)
        {
            Providers = res.Data;
        }
    }
}
