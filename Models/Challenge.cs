using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OOP_Semester.Models
{
    [Table("challenges")]
    public class Challenge
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ChallengesID { get; set; }

        // "varchar(100)" -> Giới hạn độ dài chuỗi là 100
        [StringLength(100)]
        public string? Title { get; set; }

        // "text" -> Lưu chuỗi dài thoải mái
        public string? Description { get; set; }

        // "date" -> Dùng DateTime (hoặc DateOnly trong .NET 6+)
        // Anh để nullable (?) để lỡ chưa có ngày bắt đầu/kết thúc thì không bị lỗi
        [DataType(DataType.Date)]
        public DateTime? StartDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }
    }
}
