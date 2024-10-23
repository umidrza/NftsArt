using NftsArt.Model.Enums;
using System.ComponentModel.DataAnnotations;

namespace NftsArt.Model.Dtos.Wallet;

public record class WalletCreateDto
{
    public decimal Balance { get; set; } = 0;

    [Required(ErrorMessage = "Wallet Blockchain is required.")]
    [EnumDataType(typeof(Blockchain))]
    public Blockchain Blockchain { get; set; }

    [Required(ErrorMessage = "Wallet Provider is required.")]
    public int ProviderId { get; set; }

    [Required(ErrorMessage = "Wallet Currency is required.")]
    [EnumDataType(typeof(Currency))]
    public Currency Currency { get; set; }
}
