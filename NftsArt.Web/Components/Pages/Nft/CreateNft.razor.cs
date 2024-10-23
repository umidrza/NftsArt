using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components;
using NftsArt.Model.Dtos.Nft;
using NftsArt.Model.Dtos.Collection;

namespace NftsArt.Web.Components.Pages.Nft;

public partial class CreateNft
{
    [Inject] ApiClient ApiClient { get; set; }
    [Inject] NavigationManager NavigationManager { get; set; }
    //[Inject] IJSRuntime JS {  get; set; }

    [SupplyParameterFromForm]
    private NftCreateDto NftCreateDto { get; set; } = new NftCreateDto();

    private List<CollectionSummaryDto>? Collections { get; set; }

    private bool hasImage = false;
    private string uploadedImageSrc;

    private async Task HandleValidSubmit()
    {
        var res = await ApiClient.PostAsync<NftSummaryDto, NftCreateDto>("api/nft", NftCreateDto);

        if (res.IsSuccess && res.Data != null)
        {
            var nft = res.Data;
            NavigationManager.NavigateTo($"/nft/{nft.Id}");
        }
    }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await LoadCollections();
    }

    protected async Task LoadCollections()
    {
        var res = await ApiClient.GetFromJsonAsync<List<CollectionSummaryDto>>($"api/collection/my-collections");

        if (res.IsSuccess && res.Data != null)
        {
            Collections = res.Data;
        }
    }

    private async Task HandleImageChange(InputFileChangeEventArgs e)
    {
        var file = e.File;
        if (file != null)
        {
            // Open the stream and read the file completely using a memory stream
            using var memoryStream = new MemoryStream();
            await file.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024).CopyToAsync(memoryStream);

            // Convert the image to a base64 string for display
            var imageBytes = memoryStream.ToArray();
            uploadedImageSrc = $"data:{file.ContentType};base64,{Convert.ToBase64String(imageBytes)}";
            NftCreateDto.ImageUrl = uploadedImageSrc;
            Console.WriteLine(NftCreateDto.ImageUrl);
            hasImage = true;
        }
        else
        {
            hasImage = false;
        }
    }

    private void HandleCollectionChange(ChangeEventArgs e, int collectionId)
    {
        bool isChecked = (bool)e.Value;

        if (isChecked)
        {
            NftCreateDto.Collections.Add(collectionId);
        }
        else
        {
            NftCreateDto.Collections.Remove(collectionId);
        }
    }
}
