using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolSuppliesInventory.Data;
using SchoolSuppliesInventory.Entities;
using SchoolSuppliesInventory.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SchoolSuppliesInventory.Controllers
{
    public class OrdersController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Orders
        public async Task<IActionResult> Index()
        {
            var orders = await _context.Orders
                .Include(o => o.OrderLines)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        // GET: Orders/Create
        public async Task<IActionResult> Create()
        {
            var products = await _context.Products
                .Where(p => p.AvailableQuantity > 0)
                .ToListAsync();

            var viewModel = new OrderViewModel
            {
                OrderDate = DateTime.Today,
                AvailableProducts = products.Select(p => new ProductViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    UnitPrice = p.UnitPrice,
                    AvailableQuantity = p.AvailableQuantity
                }).ToList()
            };

            return View(viewModel);
        }

        // POST: Orders/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OrderViewModel model)
        {
            if (ModelState.IsValid && model.OrderLines.Any())
            {
                // Validate stock availability
                foreach (var orderLine in model.OrderLines)
                {
                    var product = await _context.Products.FindAsync(orderLine.ProductId);
                    if (product == null || product.AvailableQuantity < orderLine.Quantity)
                    {
                        ModelState.AddModelError("", $"Insufficient stock for {product?.Name ?? "Unknown Product"}");
                        await LoadAvailableProducts(model);
                        return View(model);
                    }
                }

                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var order = new Order
                    {
                        OrderDate = model.OrderDate,
                        TotalAmount = model.TotalAmount
                    };

                    _context.Orders.Add(order);
                    await _context.SaveChangesAsync();

                    foreach (var orderLineVM in model.OrderLines)
                    {
                        var orderLine = new OrderLine
                        {
                            OrderId = order.Id,
                            ProductId = orderLineVM.ProductId,
                            Quantity = orderLineVM.Quantity,
                            UnitPrice = orderLineVM.UnitPrice,
                            LineTotal = orderLineVM.LineTotal
                        };

                        _context.OrderLines.Add(orderLine);

                        // Update product stock
                        var product = await _context.Products.FindAsync(orderLineVM.ProductId);
                        product.AvailableQuantity -= orderLineVM.Quantity;
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    TempData["Success"] = "Order created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch
                {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError("", "An error occurred while saving the order.");
                }
            }

            await LoadAvailableProducts(model);
            return View(model);
        }

        // GET: Orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.OrderLines)
                .ThenInclude(ol => ol.Product)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // GET: Orders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.OrderLines)
                .ThenInclude(ol => ol.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            var products = await _context.Products.ToListAsync();

            var viewModel = new OrderViewModel
            {
                Id = order.Id,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                OrderLines = order.OrderLines.Select(ol => new OrderLineViewModel
                {
                    Id = ol.Id,
                    ProductId = (int)ol.ProductId,
                    Quantity = ol.Quantity,
                    UnitPrice = ol.UnitPrice,
                    LineTotal = ol.LineTotal,
                    ProductName = ol.Product.Name,
                    AvailableQuantity = ol.Product.AvailableQuantity + ol.Quantity // Add back the current quantity
                }).ToList(),
                AvailableProducts = products.Select(p => new ProductViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    UnitPrice = p.UnitPrice,
                    AvailableQuantity = p.AvailableQuantity
                }).ToList()
            };

            return View(viewModel);
        }

        // POST: Orders/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, OrderViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid && model.OrderLines.Any())
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var existingOrder = await _context.Orders
                        .Include(o => o.OrderLines)
                        .FirstOrDefaultAsync(o => o.Id == id);

                    if (existingOrder == null)
                    {
                        return NotFound();
                    }

                    // Restore stock from existing order lines
                    foreach (var existingOrderLine in existingOrder.OrderLines)
                    {
                        var product = await _context.Products.FindAsync(existingOrderLine.ProductId);
                        product.AvailableQuantity += existingOrderLine.Quantity;
                    }

                    // Validate new stock requirements
                    foreach (var newOrderLine in model.OrderLines)
                    {
                        var product = await _context.Products.FindAsync(newOrderLine.ProductId);
                        if (product == null || product.AvailableQuantity < newOrderLine.Quantity)
                        {
                            ModelState.AddModelError("", $"Insufficient stock for {product?.Name ?? "Unknown Product"}");
                            await LoadAvailableProducts(model);
                            await transaction.RollbackAsync();
                            return View(model);
                        }
                    }

                    // Remove existing order lines
                    _context.OrderLines.RemoveRange(existingOrder.OrderLines);

                    // Update order
                    existingOrder.OrderDate = model.OrderDate;
                    existingOrder.TotalAmount = model.TotalAmount;

                    // Add new order lines and update stock
                    foreach (var orderLineVM in model.OrderLines)
                    {
                        var orderLine = new OrderLine
                        {
                            OrderId = existingOrder.Id,
                            ProductId = orderLineVM.ProductId,
                            Quantity = orderLineVM.Quantity,
                            UnitPrice = orderLineVM.UnitPrice,
                            LineTotal = orderLineVM.LineTotal
                        };

                        _context.OrderLines.Add(orderLine);

                        // Update product stock
                        var product = await _context.Products.FindAsync(orderLineVM.ProductId);
                        product.AvailableQuantity -= orderLineVM.Quantity;
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    TempData["Success"] = "Order updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch
                {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError("", "An error occurred while updating the order.");
                }
            }

            await LoadAvailableProducts(model);
            return View(model);
        }

        // GET: Orders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.OrderLines)
                .ThenInclude(ol => ol.Product)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderLines)
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (order != null)
                {
                    // Restore stock for all order lines
                    foreach (var orderLine in order.OrderLines)
                    {
                        var product = await _context.Products.FindAsync(orderLine.ProductId);
                        if (product != null)
                        {
                            product.AvailableQuantity += orderLine.Quantity;
                        }
                    }

                    _context.Orders.Remove(order);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    TempData["Success"] = "Order deleted successfully!";
                }
            }
            catch
            {
                await transaction.RollbackAsync();
                TempData["Error"] = "An error occurred while deleting the order.";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Products for AJAX
        [HttpGet]
        public async Task<IActionResult> GetProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            return Json(new
            {
                id = product.Id,
                name = product.Name,
                unitPrice = product.UnitPrice,
                availableQuantity = product.AvailableQuantity
            });
        }

        private async Task LoadAvailableProducts(OrderViewModel model)
        {
            var products = await _context.Products
                .Where(p => p.AvailableQuantity > 0)
                .ToListAsync();

            model.AvailableProducts = products.Select(p => new ProductViewModel
            {
                Id = p.Id,
                Name = p.Name,
                UnitPrice = p.UnitPrice,
                AvailableQuantity = p.AvailableQuantity
            }).ToList();
        }
    }

}
