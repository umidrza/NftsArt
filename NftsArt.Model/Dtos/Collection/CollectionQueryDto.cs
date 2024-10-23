namespace NftsArt.Model.Dtos.Collection;

public class CollectionQueryDto
{
    public string? SearchTerm { get; set; }
    public string? Categories { get; set; }
    public string? BlockchainName { get; set; }
    public string? SortBy { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 12;
}
