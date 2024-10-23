using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NftsArt.Model.Entities;

namespace NftsArt.Database.Data;

public class AppDbContext : IdentityDbContext<User>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    //public DbSet<User> Users { get; set; }
    public DbSet<Avatar> Avatars { get; set; }
    public DbSet<Collection> Collections { get; set; }
    public DbSet<Nft> Nfts { get; set; }
    public DbSet<Auction> Auctions { get; set; }
    public DbSet<Bid> Bids { get; set; }
    public DbSet<Wallet> Wallets { get; set; }
    public DbSet<Provider> Providers { get; set; }
    public DbSet<CollectionNft> CollectionNfts { get; set; }
    public DbSet<NftCollector> NftCollectors { get; set; }
    public DbSet<Follow> Follows { get; set; }
    public DbSet<Like> Likes { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<User>()
            .HasOne(u => u.Avatar)
            .WithMany()
            .HasForeignKey(u => u.AvatarId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Collection>()
            .HasOne(c => c.Creator)
            .WithMany(u => u.Collections)
            .HasForeignKey(c => c.CreatorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<CollectionNft>()
            .HasOne(cn => cn.Collection)
            .WithMany(c => c.CollectionNfts)
            .HasForeignKey(cn => cn.CollectionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<CollectionNft>()
            .HasOne(cn => cn.Nft)
            .WithMany(n => n.CollectionNfts)
            .HasForeignKey(cn => cn.NftId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Nft>()
            .HasOne(n => n.Creator)
            .WithMany(u => u.Nfts)
            .HasForeignKey(n => n.CreatorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<NftCollector>()
            .HasOne(nc => nc.Collector)
            .WithMany(c => c.NftCollectors)
            .HasForeignKey(nc => nc.CollectorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<NftCollector>()
            .HasOne(nc => nc.Nft)
            .WithMany(n => n.NftCollectors)
            .HasForeignKey(nc => nc.NftId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Auction>()
            .HasOne(a => a.Nft)
            .WithOne(n => n.Auction)
            .HasForeignKey<Auction>(a => a.NftId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Auction>()
            .HasOne(a => a.Seller)
            .WithMany(u => u.Auctions)
            .HasForeignKey(a => a.SellerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Bid>()
            .HasOne(b => b.Auction)
            .WithMany(n => n.Bids)
            .HasForeignKey(b => b.AuctionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Bid>()
            .HasOne(b => b.Bidder)
            .WithMany(u => u.Bids)
            .HasForeignKey(b => b.BidderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Follow>()
            .HasOne(f => f.Follower)
            .WithMany(u => u.Followers)
            .HasForeignKey(f => f.FollowerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Follow>()
            .HasOne(f => f.Following)
            .WithMany()
            .HasForeignKey(f => f.FollowingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Like>()
            .HasOne(l => l.Nft)
            .WithMany(n => n.Likes)
            .HasForeignKey(l => l.NftId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Like>()
            .HasOne(l => l.User)
            .WithMany()
            .HasForeignKey(l => l.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Wallet>()
            .HasOne(w => w.User)
            .WithMany(u => u.Wallets)
            .HasForeignKey(w => w.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.Entity<Wallet>()
            .HasOne(w => w.Provider)
            .WithOne(p => p.Wallet)
            .HasForeignKey<Wallet>(w => w.ProviderId)
            .OnDelete(DeleteBehavior.Restrict);

        List<IdentityRole> roles = new List<IdentityRole>
        {
            new IdentityRole { Id = "1", Name = "Admin", NormalizedName = "ADMIN" },
            new IdentityRole { Id = "2", Name = "User", NormalizedName = "USER" }
        };
        builder.Entity<IdentityRole>().HasData(roles);

        List<Avatar> avatars = new List<Avatar>
        {
            new Avatar {Id = 1, Url = "https://localhost:7152/img/avatars/avatar.jpeg"},
            new Avatar {Id = 2, Url = "https://localhost:7152/img/avatars/avatar2.jpeg"},
            new Avatar {Id = 3, Url = "https://localhost:7152/img/avatars/avatar3.jpeg"},
            new Avatar {Id = 4, Url = "https://localhost:7152/img/avatars/avatar4.jpeg"},
        };
        builder.Entity<Avatar>().HasData(avatars);

        var hasher = new PasswordHasher<User>();

        List<User> users = new List<User>
        {
            new User
            {
                Id = "1",
                UserName = "hope",
                FullName = "Hope",
                Email = "umidrza47@gmail.com",
                NormalizedUserName = "HOPE",
                NormalizedEmail = "UMIDRZA47@GMAIL.COM",
                PasswordHash = hasher.HashPassword(null!, "Hope444)"),
                EmailConfirmed = true,
                AvatarId = 1
            }
        };
        builder.Entity<User>().HasData(users);

        List<IdentityUserRole<string>> userRoles = new List<IdentityUserRole<string>>
        {
            new IdentityUserRole<string>
            {
                UserId = "1",
                RoleId = "1"
            }
        };
        builder.Entity<IdentityUserRole<string>>().HasData(userRoles);

        List<Provider> providers = new List<Provider>
        {
            new Provider { Id = 1, Name = "Coinbase", Image= "https://localhost:7152/img/providers/coinbase-coin.png" },
            new Provider { Id = 2, Name = "Metamask", Image= "https://localhost:7152/img/providers/metamask.png" },
            new Provider { Id = 3, Name = "Phantom", Image= "https://localhost:7152/img/providers/phantom.png" },
            new Provider { Id = 4, Name = "Trust Wallet", Image= "https://localhost:7152/img/providers/trust-wallet.png" },
            new Provider { Id = 5, Name = "Wallet Connect", Image= "https://localhost:7152/img/providers/walletconnect.png" },
        };
        builder.Entity<Provider>().HasData(providers);


        base.OnModelCreating(builder);
    }
}

// dotnet ef migrations add Init --project ./NftsArt.Database --startup-project ./NftsArt.AppHost
// dotnet ef database update --project ./NftsArt.Database --startup-project ./NftsArt.AppHost