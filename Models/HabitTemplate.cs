using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OOP_Semester.Models
{
    [Table("habittemplate")]
    public class HabitTemplate
    {
        public int TemplateID { get; set; }
        public int CategoryID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        // Dòng DefaultGoalUnitType đã bị xóa
        public string DefaultGoalUnitName { get; set; }
        public decimal DefaultGoalValuePerDay { get; set; }
    }
}
