using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YourNamespace.Models
{
    [Table("habittemplate")]
    public class HabitTemplate
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TemplateID { get; set; }

        public int CategoryID { get; set; }

        [Required]
        [StringLength(100)]
        public string? Name { get; set; }

        public string? Description { get; set; }

        // --- Các thuộc tính có trong Hình 2 ---

        [StringLength(10)]
        public string? IconCode { get; set; } // Mã icon (ví dụ FontAwesome)

        [StringLength(50)]
        public string? DefaultGoalUnitType { get; set; } // Loại đơn vị (Time, Count, Checkbox)

        [StringLength(20)]
        public string? DefaultGoalUnitName { get; set; } // Tên đơn vị (phút, lần, km)

        public decimal DefaultGoalValuePerDay { get; set; }

        public int SortOrder { get; set; } // Thứ tự sắp xếp

        public bool IsActive { get; set; } // Trạng thái kích hoạt

        // --- CÁC THUỘC TÍNH MỚI (Dựa trên yêu cầu cải thiện) ---

        [StringLength(9)]
        public string? ColorHex { get; set; } // Lưu mã màu hex (ví dụ: #FF5733) để hiển thị UI

        public bool IsRepeatable { get; set; } // Nút bật/tắt lặp lại

        [StringLength(20)]
        public string? RepeatType { get; set; } // Daily, Weekly, Monthly (nếu IsRepeatable = true)

        [StringLength(20)]
        public string? ViewShape { get; set; } // Lưu tùy chọn hiển thị: "Circle", "Rounded", "Square"
    }
}