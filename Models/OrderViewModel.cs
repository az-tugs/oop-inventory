using SchoolSuppliesInventory.Entities;
using System.ComponentModel.DataAnnotations;

namespace SchoolSuppliesInventory.Models
{
    public class OrderViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Order Date")]
        [DataType(DataType.Date)]
        public DateTime OrderDate { get; set; }

        [Display(Name = "Total Amount")]
        public decimal TotalAmount { get; set; }

        public List<OrderLineViewModel> OrderLines { get; set; } = new List<OrderLineViewModel>();

        public List<ProductViewModel> AvailableProducts { get; set; } = new List<ProductViewModel>();
    }
}
