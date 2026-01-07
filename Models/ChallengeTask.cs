using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OOP_Semester.Models
{
    [Table("tasks")]
    public class ChallengeTask
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TaskID { get; set; }

        public int ChallengesID { get; set; }

        public string? Description { get; set; }

        [StringLength(50)]
        public string? DailySession { get; set; } = "Anytime";

        [ForeignKey("ChallengesID")]
        public virtual Challenge? Challenges { get; set; }

        // --- [QUAN TRỌNG] Thêm dòng này để fix lỗi chọn RadioButton ---
        [NotMapped]
        public Guid TempGroupId { get; } = Guid.NewGuid();
    }
}