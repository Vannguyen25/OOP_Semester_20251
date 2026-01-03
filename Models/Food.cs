using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OOP_Semester.Models
{
    [Table("food")]
    public class Food
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int FoodID { get; set; }

        [StringLength(100)]
        public string? Name { get; set; }

        public string? Description { get; set; }

        // Kinh nghiệm nhận được khi sử dụng
        public int ExperiencePerUnit { get; set; } = 0;

        // Giá mua
        public int Price { get; set; } = 0;

        // Đường dẫn ảnh (varchar 255)
        [StringLength(255)]
        public string? ImagePath { get; set; }
    }
}
