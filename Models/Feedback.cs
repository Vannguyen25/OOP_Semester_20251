using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OOP_Semester.Models
{
    [Table("feedback")]
    public class Feedback
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("feedbackid")] // Map với cột viết thường trong MySQL (nếu cần)
        public int FeedbackID { get; set; }

        [Column("userid")]
        public int UserID { get; set; }

        // "text" trong SQL -> string trong C#
        [Column("content")]
        public string? Content { get; set; }

        // "tinyint" trong SQL -> byte trong C# (lưu giá trị từ 0-255)
        [Column("rating")]
        public byte Rating { get; set; }

        [Column("createdat")]
        public DateTime CreatedAt { get; set; } = DateTime.Now; // Mặc định là thời điểm hiện tại

        // --- Quan hệ (Foreign Key) ---
        // Liên kết ngược về bảng User để biết ai là người feedback
        [ForeignKey("UserID")]
        public virtual User? User { get; set; }
    }
}
