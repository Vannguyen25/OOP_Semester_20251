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

        public int UserID { get; set; }

        public int Amount { get; set; }

        // Cột mới thêm để lưu thời gian giao dịch
        public DateTime TransactionDate { get; set; } = DateTime.Now;

        // Các cột bổ sung để hiển thị giống hình 4 (Nếu bạn muốn)
        public string? Source { get; set; } = "Cửa hàng"; // Nguồn: "Nhiệm vụ", "Cửa hàng"...
        public string? Note { get; set; } = "Mua đồ"; // Ghi chú chi tiết
    }
}