using System.ComponentModel.DataAnnotations;

namespace SchoolSuppliesInventory.Models
{
    public class OrderLineViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Product")]
        public int ProductId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal LineTotal { get; set; }

        public string? ProductName { get; set; }

        public int AvailableQuantity { get; set; }
    }
}
