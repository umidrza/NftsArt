using Microsoft.EntityFrameworkCore;
using NftsArt.Model.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace NftsArt.Model.Entities;

public class Wallet
{
    public int Id { get; set; }
    public string Key { get; set; } = new string(Enumerable.Range(0, 15).Select(x => (char)('0' + new Random().Next(10))).ToArray());

    [Precision(18, 2)]
    public decimal Balance { get; set; } = 0;
    public DateTime Expiration { get; set; } = DateTime.Now.AddYears(3);
    public Blockchain Blockchain { get; set; }
    public Currency Currency { get; set; }

    [ForeignKey("User")]
    public string UserId { get; set; }
    public User User { get; set; }

    [ForeignKey("Provider")]
    public int ProviderId { get; set; }
    public Provider Provider { get; set; }
}
