using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OOP_Semester.Models
{
    [Table("habit_reminders")]
    public class HabitReminder
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ReminderID { get; set; }

        public int HabitID { get; set; }

        public int UserID { get; set; }

        // Kiểu TimeSpan tương ứng với TIME trong MySQL
        public TimeSpan ReminderTime { get; set; }

        [StringLength(20)]
        public string? ReminderType { get; set; } // Ví dụ: "Daily", "Weekly"

        [StringLength(50)]
        public string? PeriodKey { get; set; } // Ví dụ để lưu chuỗi "Mon,Tue" nếu cần

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }

        // Navigation Properties
        [ForeignKey("HabitID")]
        public virtual Habit? Habit { get; set; }

        [ForeignKey("UserID")]
        public virtual User? User { get; set; }
    }
}