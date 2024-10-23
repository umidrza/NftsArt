using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace NftsArt.Web.Components.Pages.Home;

public partial class ConnectWallet
{
    [Inject] IJSRuntime JS { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await JS.InvokeVoidAsync("WalletScript");
        }
    }
}
