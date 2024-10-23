using System.ComponentModel.DataAnnotations.Schema;

namespace NftsArt.Model.Entities;

    public class Follow
    {
        public int Id { get; set; }

        [ForeignKey("Follower")]
        public string FollowerId { get; set; }
        public User Follower { get; set; }

        [ForeignKey("Following")]
        public string FollowingId { get; set; }
        public User Following { get; set; }

        public DateTime FollowedOn { get; set; } = DateTime.UtcNow;

        public bool IsDeleted { get; set; } = false;
}
