using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OOP_Semester.Models
{
    [Table("userchallenges")]
    public class UserChallenge
    {
        // Khóa chính phức hợp (Composite Key) sẽ được cấu hình trong AppDbContext
        public int UserID { get; set; }
        public int ChallengesID { get; set; }

        public decimal Progress { get; set; } // 0 -> 100

        public DateTime JoinDate { get; set; } = DateTime.Now;

        public string Status { get; set; } = "Ongoing"; // "Ongoing", "Completed", "Expired"

        public bool IsRewardClaimed { get; set; }

        // --- NAVIGATION PROPERTIES (QUAN TRỌNG ĐỂ DÙNG .Include()) ---

        [ForeignKey("UserID")]
        public virtual User User { get; set; }

        [ForeignKey("ChallengesID")]
        public virtual Challenge Challenge { get; set; }
    }
}