using System.ComponentModel.DataAnnotations;

namespace NftsArt.Model.Dtos.Wallet;

public record class WalletUpdateDto
{
    [Required(ErrorMessage = "Enter balance")]
    public decimal Balance { get; set; }
}
