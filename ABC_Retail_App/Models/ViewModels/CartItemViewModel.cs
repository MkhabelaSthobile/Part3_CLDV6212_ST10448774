using System.ComponentModel.DataAnnotations;

namespace ABC_Retail_App.Models.ViewModels
{
    public class CartItemViewModel
    {
        public int Id { get; set; }
        public string ProductId { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string? ProductImageUrl { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }

        public double UnitPrice { get; set; }
        public double TotalPrice => UnitPrice * Quantity;
        public int StockAvailable { get; set; }
    }

    public class CartViewModel
    {
        public string CustomerUsername { get; set; } = string.Empty;
        public List<CartItemViewModel> CartItems { get; set; } = new();

        public int TotalItems => CartItems.Sum(ci => ci.Quantity);
        public double TotalPrice => CartItems.Sum(ci => ci.TotalPrice);

        public bool HasItems => CartItems.Any();
    }

    public class AddToCartViewModel
    {
        [Required]
        public string ProductId { get; set; } = string.Empty;

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; } = 1;
    }
}