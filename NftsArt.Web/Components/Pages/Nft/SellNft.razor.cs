using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using NftsArt.Model.Dtos.Auction;
using NftsArt.Model.Dtos.Nft;
using NftsArt.Model.Dtos.Wallet;
using NftsArt.Model.Entities;

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

    private bool isDataLoaded;
    private bool isScriptsInitialized;

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
        }

        isDataLoaded = true;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!isScriptsInitialized && isDataLoaded)
        {
            if (Nft != null)
            {
                await JS.InvokeVoidAsync("NftSellScript");
            }

            isScriptsInitialized = true;
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
            $"?BlockchainName={Nft?.Blockchain}");
        
        if (res != null && res.IsSuccess && res != null)
        {
            Wallet = res.Data;
        }
        else
        {
            Console.WriteLine($"You don't have {Nft?.Blockchain} blockchain");
        }
    }

    private string CalculateCountdown(DateTime auctionEndTime)
    {
        var timeDifference = auctionEndTime - DateTime.Now;

        if (timeDifference.TotalMilliseconds > 0)
        {
            var days = timeDifference.Days;
            var hours = timeDifference.Hours;
            var minutes = timeDifference.Minutes;

            return $"{days}d : {hours:D2}h : {minutes:D2}m";
        }
        else
        {
            return "Expired";
        }
    }
}
