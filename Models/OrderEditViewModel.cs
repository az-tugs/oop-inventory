using SchoolSuppliesInventory.Entities;

namespace SchoolSuppliesInventory.Models
{
    public class OrderEditViewModel
    {
        public Order Order { get; set; }
        public ICollection<Product> Products { get; set; } = new List<Product>();
        public ICollection<OrderLine> ExistingOrderLines { get; set; } = new List<OrderLine>();
    }
}
