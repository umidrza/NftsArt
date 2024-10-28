namespace NftsArt.Model.Dtos.Bid;

public record class BidSummaryDto(
    int Id,
    decimal Amount,
    DateTime StartTime,
    DateTime EndTime,
    int Quantity,
    int NftId,
    string BidderId
);
