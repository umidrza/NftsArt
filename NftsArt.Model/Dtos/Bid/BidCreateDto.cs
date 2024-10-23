using System.ComponentModel.DataAnnotations;

namespace NftsArt.Model.Dtos.Bid;

public record class BidCreateDto
{
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be valid.")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "Expiration date is required.")]
    public DateTime EndTime { get; set; }
     
    public int Quantity { get; set; } = 1;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (EndTime < DateTime.Now)
        {
            yield return new ValidationResult("Expiration date cannot be in the past", new[] { nameof(EndTime) });
        }
    }
}
