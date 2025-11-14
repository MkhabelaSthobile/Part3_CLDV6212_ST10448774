using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ABC_Retail_App.Models
{
    [Table("Users")]
    public class User
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Username is required")]
        [StringLength(100)]
        [Column("Username")]
        [Display(Name = "Username")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(256)]
        [Column("PasswordHash")]
        [Display(Name = "Password")]
        public string PasswordHash { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role is required")]
        [StringLength(20)]
        [Column("Role")]
        [Display(Name = "Role")]
        public string Role { get; set; } = "Customer"; // "Customer" or "Admin"

        // Helper properties (not in database)
        [NotMapped]
        public bool IsAdmin => Role == "Admin";

        [NotMapped]
        public bool IsCustomer => Role == "Customer";
    }

    public enum UserRole
    {
        Customer,
        Admin
    }
}