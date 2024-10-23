namespace NftsArt.Model.Dtos.Nft;

public class NftQueryDto
{
    public string? SearchTerm { get; set; }
    public string? Statuses { get; set; }
    public string? CurrencyName { get; set; }
    public string? Quantity { get; set; }
    public string? MinPrice { get; set; }
    public string? MaxPrice { get; set; }
    public string? SortBy { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 12;
}
