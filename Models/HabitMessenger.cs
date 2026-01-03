using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OOP_Semester.Models
{
    [Table("habitmessenger")]
    public class HabitMessenger
    {
        // Khóa chính phần 1
        [Key, Column(Order = 0)]
        public int HabitID { get; set; }

        // Khóa chính phần 2
        [Key, Column(Order = 1)]
        public int MesID { get; set; }

        // "time" trong SQL -> TimeSpan trong C#
        public TimeSpan ReminderTime { get; set; }

        // --- Navigation Property ---
        [ForeignKey("HabitID")]
        public virtual Habit? Habit { get; set; }
    }
}