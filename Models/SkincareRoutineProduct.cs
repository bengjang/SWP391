using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace lamlai.Models
{
    [Table("SkincareRoutineProducts", Schema = "dbo")]
    public partial class SkincareRoutineProduct
    {
        [Key]
        public int Id { get; set; }
        
        public int RoutineId { get; set; }
        
        public string StepName { get; set; }
        
        public int ProductID { get; set; }
        
        public int OrderIndex { get; set; }
        
        public string CustomDescription { get; set; }
        
        [ForeignKey("RoutineId")]
        public virtual SkincareRoutine Routine { get; set; }
        
        [ForeignKey("ProductID")]
        public virtual Product Product { get; set; }
    }
}
