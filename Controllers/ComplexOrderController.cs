// using Microsoft.AspNetCore.Mvc;
// using backend.Data;
// using backend.Models;
// using Microsoft.EntityFrameworkCore;
// using backend.Controllers.backend.Models;

// namespace backend.Controllers
// {
//     [Route("api/[controller]")]
//     [ApiController]
//     public class ComplexOrderController : ControllerBase
//     {
//         private readonly AppDbContext _context;

//         public ComplexOrderController(AppDbContext context)
//         {
//             _context = context;
//         }
// [HttpPost("create-order")]
// public async Task<IActionResult> CreateOrder([FromBody] OrderRequestWrapper orderRequestWrapper)
// {
//     if (orderRequestWrapper?.OrderRequest == null)
//     {
//         return BadRequest("Order request is missing.");
//     }

//     var orderRequest = orderRequestWrapper.OrderRequest;

//     if (_context.Customers == null || _context.Users == null || _context.Products == null)
//     {
//         return BadRequest("Required data is not available.");
//     }

//     using var transaction = await _context.Database.BeginTransactionAsync();
//     try
//     {
//         // Find the customer
//         var customer = await _context.Customers
//             .Where(c => c.uid == orderRequest.Uid)
//             .FirstOrDefaultAsync();

//         if (customer == null)
//         {
//             return BadRequest("Customer not found.");
//         }

//         decimal totalAmount = 0;

//         if (orderRequest.Products != null)
//         {
//             foreach (var productItem in orderRequest.Products)
//             {
//                 var product = await _context.Products
//                     .Where(p => p.pid == productItem.ProductId)
//                     .FirstOrDefaultAsync();

//                 if (product == null)
//                 {
//                     return BadRequest($"Product with ID {productItem.ProductId} not found.");
//                 }

//                 if (product.stock < productItem.Quantity)
//                 {
//                     return BadRequest($"Not enough stock for product {productItem.ProductId}.");
//                 }

//                 // Reduce stock
//                 product.stock -= productItem.Quantity;

//                 // Create the Order
//                 var order = new Order
//                 {
//                     cid = customer.cid,
//                     pid = product.pid,
//                     o_date = DateTime.Now,
//                     quantity = productItem.Quantity,
//                     price = product.price * productItem.Quantity, // Calculate total for this product
//                 };

//                 totalAmount += order.price;

//                 // Add order to the database
//                 _context.Orders.Add(order);
//             }
//         }

//         // Save changes to the database
//         await _context.SaveChangesAsync();

//         // Commit the transaction
//         await transaction.CommitAsync();

//         return Ok(new { message = "Order created successfully", totalAmount = totalAmount });
//     }
//     catch (Exception ex)
//     {
//         // Rollback in case of an error
//         await transaction.RollbackAsync();
//         return StatusCode(500, $"An error occurred: {ex.Message}");
//     }
// }




//     }
//     public class OrderRequestWrapper
//     {
//         public OrderRequest OrderRequest { get; set; } = new OrderRequest();
//     }

//     public class OrderRequest
//     {
//         public int Uid { get; set; }
//         public int cid { get; set; }
//         public List<ProductItem>? Products { get; set; } = new List<ProductItem>();
//     }


//     namespace backend.Models
//     {
//         public class OrderItem
//         {
//             public int OrderItemId { get; set; }
//             public int OrderId { get; set; }
//             public int ProductId { get; set; }
//             public int Quantity { get; set; }
//             public decimal Price { get; set; }

//             public Order Order { get; set; }
//             public Product Product { get; set; }
//         }
//     }


//     public class ProductItem
//     {
//         public int ProductId { get; set; }
//         public int Quantity { get; set; }
//     }
// }
