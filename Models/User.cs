using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OOP_Semester.Models
{

    // 2. Class User
    [Table("users")]
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("UserID")] // Thêm dòng này nếu DB dùng snake_case (user_id)
        public int UserID { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("Account")] // Ánh xạ nếu tên cột trong DB viết thường
        public string Account { get; set; }

        [Required]
        [MaxLength(255)]
        [Column("Password")]
        public string Password { get; set; }

        [MaxLength(100)]
        [Column("Name")]
        public string? Name { get; set; }

        // Lưu ý: Cần config Conversion trong DbContext để nó lưu thành chữ "User"/"Admin"
        [Column("Role", TypeName = "enum('User','Admin')")]
        public UserRole Role { get; set; } = UserRole.User;

        // MySQL tinyint(1) -> C# bool là chuẩn rồi
        [Column("VacationMode")]
        public bool VacationMode { get; set; }

        // MySQL Time -> C# TimeSpan là chuẩn
        [Column("MorningTime")]
        public TimeSpan? MorningTime { get; set; }

        [Column("AfternoonTime")]
        public TimeSpan? AfternoonTime { get; set; }

        [Column("EveningTime")]
        public TimeSpan? EveningTime { get; set; }

        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [MaxLength(255)]
        [Column("Avatar")]
        public string? Avatar { get; set; }

        [Column("GoldAmount")]
        public int? GoldAmount { get; set; } = 0; // Nên gán mặc định là 0
    }
}