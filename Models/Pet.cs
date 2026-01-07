using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OOP_Semester.Models
{
    [Table("pets")]
    public class Pet
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PetID { get; set; }

        public int UserID { get; set; }

        public int PetTypeID { get; set; }

        [StringLength(100)]
        public string? Name { get; set; }

        public int Level { get; set; } = 1; // Mặc định level 1

        public int Experience { get; set; } = 0;

        [StringLength(50)]
        public string? Status { get; set; } = "Happy";

        [DataType(DataType.DateTime)]
        public DateTime? LastFedDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // --- Navigation Properties ---
        [ForeignKey("UserID")]
        public virtual User? User { get; set; }

        [ForeignKey("PetTypeID")]
        public virtual PetType? PetType { get; set; }
    }
}