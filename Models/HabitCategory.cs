using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OOP_Semester.Models
{
    [Table("habitcategories")]
    public class HabitCategory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CategoryID { get; set; }

        [StringLength(100)]
        public string? Name { get; set; }

        // Kiểu "text" trong SQL map sang string
        public string? Description { get; set; }
    }
}
