using System.ComponentModel.DataAnnotations;

namespace NftsArt.Model.Dtos.User;

public class VerifyCodeDto
{
    [Required(ErrorMessage = "Verification code is required")]
    [StringLength(6)]
    public string OtpCode { get; set; }
}
