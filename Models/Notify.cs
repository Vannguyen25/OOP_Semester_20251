using OOP_Semester.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OOP_Semester.Models 
{
    [Table("notify")] 
    public class Notify
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] 
        public int NotifyID { get; set; }

        [Required]
        [ForeignKey("User")] 
        public int UserID { get; set; }

        [Required]
        public DateTime NotifyTime { get; set; }

        [Required]
        [Column(TypeName = "text")] 
        public string Message { get; set; }

        public bool IsRead { get; set; }

        public DateTime CreatedAt { get; set; }


        public virtual User User { get; set; }

        public Notify()
        {
            IsRead = false;          
            CreatedAt = DateTime.Now; 
        }
    }
}