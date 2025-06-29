using System.ComponentModel.DataAnnotations;

namespace SchoolSuppliesInventory.Entities
{
    public class Supplier
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string? Name { get; set; }

        [StringLength(100)]
        public string? ContactPerson { get; set; }

        [StringLength(200)]
        public string? ContactInfo { get; set; }

        [StringLength(150)]
        public string? Location { get; set; }

        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
