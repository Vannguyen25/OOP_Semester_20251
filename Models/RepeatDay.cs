using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OOP_Semester.Models
{
    [Table("repeatday")]
    public class RepeatDay
    {
        // Vừa là khóa chính, vừa là khóa ngoại trỏ về Habit
        [Key, ForeignKey("Habit")]
        public int HabitID { get; set; }

        // "tinyint" -> bool (True = Có lặp, False = Không lặp)
        public bool Mon { get; set; }
        public bool Tue { get; set; }
        public bool Wed { get; set; }
        public bool Thu { get; set; }
        public bool Fri { get; set; }
        public bool Sat { get; set; }
        public bool Sun { get; set; }

        // --- Navigation Property ---
        public virtual Habit? Habit { get; set; }
    }
}