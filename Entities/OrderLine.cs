using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolSuppliesInventory.Entities
{
    public class OrderLine
    {
        public int Id { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal LineTotal { get; set; }

        [Required]
        public int? OrderId { get; set; }
        public Order? Order { get; set; }

        [Required]

        public int? ProductId { get; set; }
        public Product? Product { get; set; }
    }
}
