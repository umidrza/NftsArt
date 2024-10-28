namespace NftsArt.Model.Entities;

public class Provider
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Image { get; set; }

    public List<Wallet> Wallets { get; set; } = [];
}
