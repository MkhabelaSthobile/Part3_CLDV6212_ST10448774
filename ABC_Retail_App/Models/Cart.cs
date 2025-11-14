using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ABC_Retail_App.Models
{
    [Table("Cart")]
    public class Cart
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Column("CustomerUsername")]
        [Display(Name = "Customer Username")]
        public string CustomerUsername { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Column("ProductId")]
        [Display(Name = "Product ID")]
        public string ProductId { get; set; } = string.Empty;

        [Required]
        [Column("Quantity")]
        [Display(Name = "Quantity")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }

        // Helper properties (not in database - populated from Product API)
        [NotMapped]
        public string ProductName { get; set; } = string.Empty;

        [NotMapped]
        public double UnitPrice { get; set; }

        [NotMapped]
        public string? ProductImageUrl { get; set; }

        [NotMapped]
        public int StockAvailable { get; set; }

        [NotMapped]
        [Display(Name = "Total Price")]
        [DataType(DataType.Currency)]
        public double TotalPrice => UnitPrice * Quantity;
    }
}