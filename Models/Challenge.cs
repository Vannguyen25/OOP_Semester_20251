using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OOP_Semester.Models
{
    [Table("challenges")]
    public class Challenge
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ChallengesID { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }

        // Có thể bạn đang để DateTime? (nullable). Nếu là DateTime thường thì không sao.
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public int RewardCoins { get; set; }

        // --- 👇 BỔ SUNG DÒNG NÀY ĐỂ SỬA LỖI "does not contain definition for ChallengeTasks" 👇 ---
        public virtual ICollection<ChallengeTask> ChallengeTasks { get; set; } = new List<ChallengeTask>();
    }
}