using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using NftsArt.Model.Dtos.Auction;
using NftsArt.Model.Dtos.Nft;
using NftsArt.Model.Dtos.Wallet;
using NftsArt.Model.Helpers;
using NftsArt.Web.Services;
using System.Globalization;

namespace NftsArt.Web.Components.Pages.Nft;

public partial class SellNft
{
    [Inject] ApiClient ApiClient { get; set; }
    [Inject] IJSRuntime JS {  get; set; }
    [Inject] NavigationManager Navigation { get; set; }
    [Inject] MessageService MessageService { get; set; }

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
            else
            {
                MessageService.ShowMessage(Message.Error(res?.Message ?? "Error"));
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

            AuctionCreateDto.StartTime = DateTime.Now;
            UpdateEndDate();
        }
    }

    protected async Task LoadNft()
    {
        var res = await ApiClient.GetFromJsonAsync<NftDetailDto>($"api/nft/{Id}");

        if (res != null && res.IsSuccess)
        {
            Nft = res.Data;
        }
        else
        {
            MessageService.ShowMessage(Message.Error(res?.Message ?? "Error"));
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
            Navigation.NavigateTo("wallet");
            MessageService.ShowMessage(Message.Info($"{Nft?.Blockchain} blockchain wallet needed to list this NFT ", 5000));
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


    private string _selectedSchedule = "6-month";
    public string SelectedSchedule
    {
        get => _selectedSchedule;
        set
        {
            _selectedSchedule = value;
            UpdateEndDate();
        }
    }
    private DateTime MinEndDate => AuctionCreateDto.StartTime.AddDays(1);
    private void UpdateEndDate()
    {
        var scheduleParts = SelectedSchedule.Split('-');
        int duration = int.Parse(scheduleParts[0], CultureInfo.InvariantCulture);
        string unit = scheduleParts[1];

        AuctionCreateDto.EndTime = unit switch
        {
            "month" => AuctionCreateDto.StartTime.AddMonths(duration),
            "year" => AuctionCreateDto.StartTime.AddYears(duration),
            _ => AuctionCreateDto.EndTime
        };
    }


    private bool isKeyCopied = false;
    private string TruncatedWalletKey => $"0x{Wallet?.Key[..7]}...K{Wallet?.Key[^3..]}";
    private async Task CopyToClipboard()
    {
        await JS.InvokeVoidAsync("navigator.clipboard.writeText", Wallet?.Key);
        isKeyCopied = true;
    }
}
