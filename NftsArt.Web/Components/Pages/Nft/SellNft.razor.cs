using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using NftsArt.Model.Dtos.Auction;
using NftsArt.Model.Dtos.Nft;
using NftsArt.Model.Dtos.Wallet;

namespace NftsArt.Web.Components.Pages.Nft;

public partial class SellNft
{
    [Inject] ApiClient ApiClient { get; set; }
    [Inject] NavigationManager NavigationManager { get; set; }
    [Inject] IJSRuntime JS {  get; set; }

    [Parameter] public int Id { get; set; }

    [SupplyParameterFromForm]
    private AuctionCreateDto AuctionCreateDto { get; set; } = new AuctionCreateDto();

    private NftDetailDto? Nft { get; set; }
    private WalletDetailDto? Wallet { get; set; }

    private bool isListingPopupActive = false;
    private bool isCompletedPopupActive = false; 

    private async Task HandleValidSubmit()
    {

        if (Wallet != null)
        {
            var res = await ApiClient.PostAsync<AuctionSummaryDto, AuctionCreateDto>($"api/auction/{Id}", AuctionCreateDto);

            if (res != null && res.IsSuccess)
            {
                isCompletedPopupActive = true;
            }
        }
        else
        {
            isListingPopupActive = true;
        }
    }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await LoadNft();

        if (Nft != null)
        {
            await LoadWallet();

            await JS.InvokeVoidAsync("NftSellScript");
        }
    }

    protected async Task LoadNft()
    {
        var res = await ApiClient.GetFromJsonAsync<NftDetailDto>($"api/nft/{Id}");

        if (res != null && res.IsSuccess)
        {
            Nft = res.Data;
        }
    }

    protected async Task LoadWallet()
    {
        var res = await ApiClient.GetFromJsonAsync<WalletDetailDto>(
            $"api/wallet/my-wallet" +
            $"?BlockchainName={Nft?.BlockchainName}" +
            $"&CurrencyName={AuctionCreateDto.Currency}");

        if (res != null && res.IsSuccess)
        {
            Wallet = res.Data;
        }
    }
}
