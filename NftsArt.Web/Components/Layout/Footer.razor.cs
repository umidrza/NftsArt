using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace NftsArt.Web.Components.Layout;

public partial class Footer
{
    [Inject] IJSRuntime JS { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await JS.InvokeVoidAsync("ThemeScript");
        }
    }
}
