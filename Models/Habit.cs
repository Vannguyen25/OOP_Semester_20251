using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OOP_Semester.Models
{
    [Table("habits")]
    public class Habit
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int HabitID { get; set; }

        public int UserID { get; set; }

        public int? CategoryID { get; set; }

        [StringLength(100)]
        public string? Name { get; set; }

        // "tinyint(1)" -> bool (Cờ bật/tắt mục tiêu)
        public bool UseGoal { get; set; }

        // "decimal(10,2)" -> decimal
        public decimal? GoalValuePerDay { get; set; }

        [StringLength(50)]
        public string GoalUnitType { get; set; } = "Lần";

        // Cờ bật/tắt điều kiện kết thúc
        public bool UseEndCondition { get; set; }

        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }

        public decimal? TargetTotalAmount { get; set; }

        // Cờ lặp lại hàng ngày
        public bool RepeatEveryday { get; set; }

        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; } = DateTime.Now;

        // Các chỉ số Streak mặc định là 0
        public int CurrentStreak { get; set; } = 0;
        public int BestStreak { get; set; } = 0;

        [DataType(DataType.Date)]
        public DateTime? LastStreakDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Enum Status (Active, Archived, Deleted) lưu dưới dạng string
        [StringLength(20)]
        public string? Status { get; set; } = "Active";

        [StringLength(50)]
        public string? Icon { get; set; }

        [StringLength(20)]
        public string? ColorHex { get; set; }

        // --- Navigation Properties ---
        [ForeignKey("UserID")]
        public virtual User? User { get; set; }

        [ForeignKey("CategoryID")]
        public virtual HabitCategory? Category { get; set; }
    }
}