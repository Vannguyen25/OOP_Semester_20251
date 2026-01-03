using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OOP_Semester.Models
{
    [Table("tasks")] // Map vào bảng tasks trong DB
    public class ChallengeTask
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TaskID { get; set; }

        public int ChallengesID { get; set; }

        public string? Description { get; set; }

        // Enum -> string
        [StringLength(50)]
        public string? DailySession { get; set; }

        // --- Navigation Property ---
        [ForeignKey("ChallengesID")]
        public virtual Challenge? Challenges { get; set; }
    }
}