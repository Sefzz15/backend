using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/orders")]
public class OrderController : ControllerBase
{
    private readonly OrderService _orderService;
    private readonly OrderItemService _orderItemService;
    private readonly AppDbContext _context;

    public OrderController(OrderService orderService, AppDbContext context, OrderItemService orderItemService)

    {
        _orderService = orderService;
        _orderItemService = orderItemService;
        _context = context;

    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Order>>> GetAllOrders()
    {
        var orders = await _orderService.GetAllOrdersAsync();
        return Ok(orders);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrderById(int id)
    {
        var order = await _orderService.GetOrderById(id);
        if (order == null) return NotFound();
        return Ok(order);
    }

[HttpPost]
public async Task<IActionResult> CreateOrder([FromBody] Order order)
{
    // Check if order and order items are valid (uncomment validation if needed)
    // if (order == null  order.OrderItems == null  !order.OrderItems.Any())
    // {
    //     return BadRequest("Invalid order request.");
    // }

    order.Date = DateTime.UtcNow; // Set order date

    try
    {
        // Set OrderId for each OrderItem and handle stock check and creation
        var orderItems = await _orderItemService.CreateOrderItemsAsync(order);

        // Set the OrderId for each OrderItem to the Oid of the Order
        foreach (var orderItem in orderItems)
        {
            orderItem.Oid = order.Oid;  // Ensure OrderId is set to the Order's Oid
        }

        // Add the order to the context
        _context.Orders.Add(order);

        // Add the order items to the context
        _context.OrderItems.AddRange(orderItems);

        // Save all changes to the database
        await _context.SaveChangesAsync();

        return Ok(new { Message = "Order created successfully!", OrderId = order.Oid });
    }
    catch (ArgumentException ex)
    {
        return BadRequest(ex.Message);  // Handle validation failures
    }
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