using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OOP_Semester.Models
{
    [Table("userchallenges")]
    public class UserChallenge
    {
        [Key, Column(Order = 0)]
        public int UserID { get; set; }

        [Key, Column(Order = 1)]
        public int ChallengesID { get; set; }

        // "decimal(5,2)" -> decimal
        public decimal Progress { get; set; } = 0;

        // --- Navigation Properties ---
        [ForeignKey("UserID")]
        public virtual User? User { get; set; }

        [ForeignKey("ChallengesID")]
        public virtual Challenge? Challenge { get; set; }
    }
}