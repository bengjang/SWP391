using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace lamlai.Models
{
    [Table("ProductImages", Schema = "dbo")]
    public partial class ProductImage
    {
        [Key]
        public int ImageID { get; set; }
        
        [Column("ProductID")]
        public int ProductId { get; set; }
        
        public string ImgUrl { get; set; }
        
        public int DisplayOrder { get; set; }
        
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }
    }
} 