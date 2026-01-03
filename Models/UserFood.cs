using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OOP_Semester.Models
{
    [Table("userfood")] // Map đúng tên bảng viết thường trong MySQL
    public class UserFood
    {
        // Phần 1 của Khóa chính
        [Key, Column(Order = 0)]
        public int UserID { get; set; }

        // Phần 2 của Khóa chính
        [Key, Column(Order = 1)]
        public int FoodID { get; set; }

        // Số lượng (Cho phép null, mặc định là 0 như phong cách em thích)
        public int? Quantity { get; set; } = 0;

        // --- Navigation Properties (Liên kết bảng) ---

        // Liên kết về bảng User
        [ForeignKey("UserID")]
        public virtual User? User { get; set; }

        // Liên kết về bảng Food
        [ForeignKey("FoodID")]
        public virtual Food? Food { get; set; }
    }
}