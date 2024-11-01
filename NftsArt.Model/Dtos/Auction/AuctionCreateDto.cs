using NftsArt.Model.Enums;
using System.ComponentModel.DataAnnotations;

namespace NftsArt.Model.Dtos.Auction;

public record class AuctionCreateDto : IValidatableObject
{
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be valid.")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "Start time is required.")]
    public DateTime StartTime { get; set; } = DateTime.Now;

    [Required(ErrorMessage = "End time is required.")]
    public DateTime EndTime { get; set; }

    [Required(ErrorMessage = "Currency is required.")]
    [EnumDataType(typeof(Currency))]
    public Currency Currency { get; set; }

    public int Quantity { get; set; } = 1;



    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (StartTime.AddDays(1) < DateTime.Now)
        {
            yield return new ValidationResult("Start time cannot be in the past.", new[] { nameof(StartTime) });
        }

        if (EndTime <= StartTime)
        {
            yield return new ValidationResult("End time must be later than start time.", new[] { nameof(EndTime) });
        }
    }
}