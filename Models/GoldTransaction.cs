using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OOP_Semester.Models
{
    [Table("goldtransactions")]
    public class GoldTransaction
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TransactionID { get; set; }

        // Khóa ngoại trỏ về bảng User
        public int UserID { get; set; }

        // Số lượng vàng thay đổi (Dương là cộng, Âm là trừ)
        public int Amount { get; set; }

        // --- Quan hệ (Navigation Property) ---
        // Giúp em truy vấn xem giao dịch này của ai
        [ForeignKey("UserID")]
        public virtual User? User { get; set; }
    }
}
