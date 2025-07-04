﻿using System.ComponentModel.DataAnnotations;

namespace SchoolSuppliesInventory.Entities
{
    public class Category
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string? Name { get; set; }

        public ICollection<Product> Products { get; set; } = new List<Product>();

    }
}
