using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SchoolSuppliesInventory.Entities;

namespace SchoolSuppliesInventory.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderLine> OrderLines { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Category> Categories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Configure entity relationships and constraints here
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.SetNull); 
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Supplier)
                .WithMany(s => s.Products)
                .HasForeignKey(p => p.SupplierId)
                .OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<OrderLine>()
                .HasOne(ol => ol.Order)
                .WithMany(o => o.OrderLines)
                .HasForeignKey(ol => ol.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<OrderLine>()
                .HasOne(ol => ol.Product)
                .WithMany(p => p.OrderLines)
                .HasForeignKey(ol => ol.ProductId)
                .OnDelete(DeleteBehavior.Restrict);


            // Db Seed
            // Seeding
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Writing Instruments" },
                new Category { Id = 2, Name = "Paper Products" },
                new Category { Id = 3, Name = "Art Supplies" },
                new Category { Id = 4, Name = "Office Supplies" },
                new Category { Id = 5, Name = "Electronics" }
            );

            modelBuilder.Entity<Supplier>().HasData(
                new Supplier
                {
                    Id = 1,
                    Name = "National Book Store",
                    ContactPerson = "Maria Santos",
                    ContactInfo = "0917-123-4567 / maria@nationalbookstore.com",
                    Location = "Quezon City, Metro Manila"
                },
                new Supplier
                {
                    Id = 2,
                    Name = "Paperline PH",
                    ContactPerson = "Juan Dela Cruz",
                    ContactInfo = "0922-888-9090 / juan@paperline.ph",
                    Location = "Makati City, Metro Manila"
                 },
                 new Supplier
                 {
                    Id = 3,
                    Name = "Edu Supplies Corp.",
                    ContactPerson = "Liza Manalo",
                    ContactInfo = "liza@edusupplies.ph",
                    Location = "Cebu City, Cebu"
                 },
                new Supplier
                {
                    Id = 4,
                    Name = "TechGear Solutions",
                    ContactPerson = "Andres Reyes",
                    ContactInfo = "andres@techgear.ph",
                    Location = "Davao City, Davao del Sur"
                }
            );

            modelBuilder.Entity<Product>().HasData(
                new Product
                {
                    Id = 1,
                    Name = "Ballpen - Blue",
                    Description = "Standard blue ink ballpoint pen",
                    AvailableQuantity = 500,
                    UnitPrice = 8.50m,
                    CategoryId = 1,
                    SupplierId = 1
                },
                new Product
                {
                    Id = 2,
                    Name = "Intermediate Pad Paper",
                    Description = "200 sheets, 8.5 x 11 inches",
                    AvailableQuantity = 300,
                    UnitPrice = 35.00m,
                    CategoryId = 2,
                    SupplierId = 2
                },
                new Product
                {
                    Id = 3,
                    Name = "Crayons - 24 Colors",
                    Description = "Non-toxic wax crayons for children",
                    AvailableQuantity = 120,
                    UnitPrice = 75.00m,
                    CategoryId = 3,
                    SupplierId = 1
                },
                new Product
                {
                    Id = 4,
                    Name = "Stapler",
                    Description = "Heavy-duty stapler with staples included",
                    AvailableQuantity = 80,
                    UnitPrice = 110.00m,
                    CategoryId = 4,
                    SupplierId = 3
                },
                new Product
                {
                    Id = 5,
                    Name = "Scientific Calculator",
                    Description = "Casio FX-82MS or equivalent",
                    AvailableQuantity = 45,
                    UnitPrice = 550.00m,
                CategoryId = 5,
                SupplierId = 4
                },
                new Product
                {
                    Id = 6,
                    Name = "Notebook - Spiral",
                    Description = "80 leaves, college ruled",
                    AvailableQuantity = 200,
                    UnitPrice = 29.00m,
                    CategoryId = 2,
                    SupplierId = 2
                }
            );

            modelBuilder.Entity<Order>().HasData(
                new Order { Id = 1, OrderDate = DateTime.Today, TotalAmount = 2300 }
            );

            modelBuilder.Entity<OrderLine>().HasData(
                new OrderLine { Id = 1, OrderId = 1, ProductId = 1, Quantity = 1, UnitPrice = 1200, LineTotal = 1200 },
                new OrderLine { Id = 2, OrderId = 1, ProductId = 2, Quantity = 1, UnitPrice = 300, LineTotal = 300 },
                new OrderLine { Id = 3, OrderId = 1, ProductId = 3, Quantity = 1, UnitPrice = 800, LineTotal = 800 }
            );
        }
    }
}
