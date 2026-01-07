using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OOP_Semester.Models
{
    [Table("userchallengetasks")]
    public class UserChallengeTask
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserChallengeTaskID { get; set; }

        public int UserID { get; set; }
        public int ChallengesID { get; set; }
        public int TaskID { get; set; }

        public DateTime LogDate { get; set; } // Ngày lưu lịch sử
        public bool IsCompleted { get; set; }
    }
}