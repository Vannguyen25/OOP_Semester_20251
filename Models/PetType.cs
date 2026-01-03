using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OOP_Semester.Models
{
    [Table("pettype")]
    public class PetType
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PetTypeID { get; set; }

        public int Level { get; set; }

        public int ExperienceRequired { get; set; }

        [StringLength(255)]
        public string? AppearanceWhenHungry { get; set; }

        [StringLength(255)]
        public string? AppearanceWhenHappy { get; set; }
        [Column(TypeName = "text")] // Ánh xạ với cột TEXT trong SQL
        public string? NotifyWhenHungry { get; set; }

        [Column(TypeName = "text")] // Ánh xạ với cột TEXT trong SQL
        public string? RandomNotify { get; set; }
    }
}