
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/createorder")]
public class CreateOrderController : ControllerBase
{
    private readonly OrderService _orderService;
    private readonly OrderDetailService _orderDetailService;
    private readonly AppDbContext _context;
    public CreateOrderController(
        OrderService orderService,
        OrderDetailService orderDetailService,
        AppDbContext context)
    {
        _orderService = orderService;
        _orderDetailService = orderDetailService;
        _context = context;
    }



    [HttpPost("create")]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderInput input)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // Step 1: Check Stock
            var productIds = input.OrderDetails.Select(od => od.Pid).ToList();
            var products = await _context.Products
                .Where(p => productIds.Contains(p.Pid))
                .ToDictionaryAsync(p => p.Pid);

            foreach (var item in input.OrderDetails)
            {
                if (!products.ContainsKey(item.Pid))
                    return BadRequest($"Product with ID {item.Pid} does not exist.");

                if (products[item.Pid].Stock < item.Quantity)
                    return BadRequest($"Not enough stock for product {products[item.Pid].Pname}.");
            }

            // Step 2: Create Order
            var order = new Order
            {
                Uid = input.Uid,
                Date = DateTime.UtcNow
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Step 3: Create OrderDetails and Update Stock
            foreach (var item in input.OrderDetails)
            {
                _context.OrderDetails.Add(new OrderDetail
                {
                    Oid = order.Oid,
                    ProductId = item.Pid,
                    Quantity = item.Quantity
                });

                products[item.Pid].Stock -= item.Quantity;
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(order);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, "Internal server error: " + ex.Message);
        }
    }
}