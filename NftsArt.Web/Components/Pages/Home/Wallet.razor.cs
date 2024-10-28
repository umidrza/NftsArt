using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using NftsArt.Model.Dtos.Provider;
using NftsArt.Model.Dtos.User;
using NftsArt.Model.Dtos.Wallet;
using NftsArt.Model.Enums;

namespace NftsArt.Web.Components.Pages.Home;

public partial class Wallet
{
    [Inject] ApiClient ApiClient { get; set; }
    [Inject] NavigationManager NavigationManager { get; set; }

    private List<ProviderSummaryDto>? Providers { get; set; }
    private WalletDetailDto? WalletDetailDto { get; set; }
    private WalletCreateDto WalletCreateDto { get; set; } = new WalletCreateDto();

    private ProviderSummaryDto? SelectedProvider { get; set; }

    private bool isPopupActive = false;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await LoadProviders();
    }

    protected async Task LoadProviders()
    {
        var res = await ApiClient.GetFromJsonAsync<List<ProviderSummaryDto>>($"api/provider");

        if (res != null && res.IsSuccess)
        {
            Providers = res.Data;
        }
    }

    private async Task HandleWallet(int providerId)
    {
        SelectedProvider = Providers?.FirstOrDefault(p => p.Id == providerId);
        WalletCreateDto.ProviderId = providerId;
        WalletCreateDto.Currency = WalletCreateDto.Blockchain == Blockchain.Bitcoin ? Currency.BTC : Currency.ETH;

        await LoadWallet();

        isPopupActive = true;
    }

    protected async Task LoadWallet()
    {
        var res = await ApiClient.GetFromJsonAsync<WalletDetailDto>(
            $"api/wallet/my-wallet" +
            $"?BlockchainName={WalletCreateDto.Blockchain}" +
            $"&CurrencyName={WalletCreateDto.Currency}" +
            $"&ProviderId={WalletCreateDto.ProviderId}");

        if (res != null && res.IsSuccess && res.Data != null)
        {
            WalletDetailDto = res.Data;
        }
    }

    private async Task ConnectWallet()
    {
        if (WalletDetailDto == null)
        {
            var res = await CreateWallet();
            if (!res)
            {
                Console.WriteLine("Wallet create failed");
                return;
            }
        }

        NavigationManager.NavigateTo("/");
    }

    protected async Task<bool> CreateWallet()
    {
        var res = await ApiClient.PostAsync<WalletSummaryDto, WalletCreateDto>($"api/wallet", WalletCreateDto);

        return res != null && res.IsSuccess;
    }

    private bool IsTruncated { get; set; } = true;
    private string GetExtraContent(string text, int maxLength)
    {
        return IsTruncated && text.Length > maxLength
            ? text.Substring(0, maxLength) + "..."
            : text;
    }

    private string ButtonText => IsTruncated ? "Show more" : "Show less";
}
