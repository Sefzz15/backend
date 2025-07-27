using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/orders")]
public class OrderController : ControllerBase
{
    private readonly OrderService _orderService;
    private readonly OrderDetailService _orderDetailService;
    private readonly AppDbContext _context;

    public OrderController(OrderService orderService, AppDbContext context, OrderDetailService orderDetailService)

    {
        _orderService = orderService;
        _orderDetailService = orderDetailService;
        _context = context;

    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetAllOrders()
    {
        var orders = await _orderService.GetAllOrders();

        var result = orders.Select(order => new
        {
            order.Oid,
            order.Uid,
            Date = order.Date.ToString("d/M/yyyy HH:mm:ss")
        });

        return Ok(result);
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
                Date = DateTime.Now
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Step 3: Create OrderDetails and Update Stock
            foreach (var item in input.OrderDetails)
            {
                _context.OrderDetails.Add(new OrderDetail
                {
                    Oid = order.Oid,
                    Pid = item.Pid,
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

    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrderById(int id)
    {
        var order = await _orderService.GetOrderById(id);
        if (order == null) return NotFound();

        var result = new
        {
            order.Oid,
            order.Uid,
            Date = order.Date.ToString("d/M/yyyy HH:mm:ss")
        };

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] Order order)
    {
        order.Date = DateTime.Now;

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        return Ok(order);
    }


    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateOrder(int id, [FromBody] Order order)
    {
        if (id != order.Oid) return BadRequest();
        await _orderService.UpdateOrder(order);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteOrder(int id)
    {
        await _orderService.DeleteOrder(id);
        return NoContent();
    }
}