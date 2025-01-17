using Microsoft.AspNetCore.Mvc;
using backend.Data;
using backend.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ComplexOrderController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ComplexOrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("create-order")]
        public async Task<IActionResult> CreateOrder([FromBody] OrderRequestWrapper orderRequestWrapper)
        {
            if (orderRequestWrapper?.OrderRequest == null)
            {
                return BadRequest("Order request is missing.");
            }

            var orderRequest = orderRequestWrapper.OrderRequest;

            if (_context.Customers == null || _context.Users == null)
            {
                return BadRequest("Customers or Users data is not available.");
            }

            // Begin transaction
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var customer = await _context.Customers
                    .Join(_context.Users.Where(u => u != null), // Ensure users are not null
                        customer => customer.uid,
                        user => user.uid,
                        (customer, user) => new { customer, user })
                    .Where(x => x.user.uid == int.Parse(orderRequest.Uid))
                    .Select(x => x.customer)
                    .FirstOrDefaultAsync();

                if (customer == null)
                {
                    return BadRequest("Customer not found.");
                }

                orderRequest.c_id = customer.c_id.ToString();

                var order = new Order
                {
                    c_id = customer.c_id,
                    o_date = DateTime.Now,
                    total_amount = 0,
                    customer = customer
                };

                if (_context.Orders == null)
                {
                    return BadRequest("Orders data is not available.");
                }
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                decimal totalAmount = 0;

                if (orderRequest.Products != null)
                {
                    foreach (var productItem in orderRequest.Products)
                    {
                        if (productItem == null) continue; // Skip if product item is null

                        if (_context.Products == null)
                        {
                            return BadRequest("Products data is not available.");
                        }

                        var product = await _context.Products
                            .Where(p => p.p_id == productItem.ProductId)
                            .FirstOrDefaultAsync();

                        if (product == null)
                        {
                            return BadRequest($"Product with ID {productItem.ProductId} not found.");
                        }

                        if (product.stock_quantity < productItem.Quantity)
                        {
                            return BadRequest($"Not enough stock for product {productItem.ProductId}");
                        }

                        product.stock_quantity -= productItem.Quantity;

                        var orderDetail = new OrderDetail
                        {
                            o_id = order.o_id,
                            p_id = product.p_id,
                            quantity = productItem.Quantity,
                            price = product.price,
                            order = order,
                            product = product
                        };

                        if (_context.OrderDetails == null)
                        {
                            return BadRequest("OrderDetails data is not available.");
                        }
                        _context.OrderDetails.Add(orderDetail);
                        totalAmount += productItem.Quantity * product.price;
                    }
                }

                order.total_amount = totalAmount;
                await _context.SaveChangesAsync();

                // Commit transaction
                await transaction.CommitAsync();

                return Ok(new { message = "Order created successfully", orderId = order.o_id });
            }
            catch (Exception ex)
            {
                // Rollback transaction
                await transaction.RollbackAsync();
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

    }
    public class OrderRequestWrapper
    {
        public OrderRequest OrderRequest { get; set; } = new OrderRequest();
    }

    public class OrderRequest
    {
        public string Uid { get; set; } = string.Empty;
        public string? c_id { get; set; } // nullable
        public List<ProductItem>? Products { get; set; } = new List<ProductItem>();
    }


    public class ProductItem
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
