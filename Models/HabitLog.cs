using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OOP_Semester.Models
{
    [Table("habitlogs")]
    public class HabitLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int LogID { get; set; }

        public int HabitID { get; set; }

        [DataType(DataType.Date)]
        public DateTime LogDate { get; set; }

        // Enum (Morning, Afternoon, Evening)
        [StringLength(20)]
        public string? TimeOfDay { get; set; }

        // tinyint(1) -> bool (Đã hoàn thành hay chưa)
        public bool Completed { get; set; }

        // Số lượng thực hiện (decimal 10,2)
        public decimal Quantity { get; set; } = 0;

        public bool Skipped { get; set; }

        // --- Navigation Property ---
        [ForeignKey("HabitID")]
        public virtual Habit? Habit { get; set; }
    }
}